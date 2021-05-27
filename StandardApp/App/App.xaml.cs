// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Arc.Text;
using Arc.WPF;
using DryIoc; // alternative: SimpleInjector
using Serilog;
using StandardApp;
using StandardApp.Views;
using StandardApp.ViewServices;
using Tinyhand;

#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1600 // Elements should be documented

namespace Application
{
    /// <summary>
    /// Application-wide class.
    /// </summary>
    public static partial class App
    {
        private static System.Threading.Mutex appMutex = new System.Threading.Mutex(false, AppConst.MutexName);

        static App()
        {
        }

        public static bool Initialized { get; set; } = false; // Application initialized

        public static bool SessionEnding { get; set; } = false; // Session ending

        public static Dispatcher UI { get; } = Dispatcher.CurrentDispatcher; // UI dispatcher

        public static Container Container { get; } = new DryIoc.Container(); // DI container

        public static C4 C4 { get; } = C4.Instance;

        public static AppSettings Settings { get; private set; } = default!;

        public static AppOptions Options { get; private set; } = default!;

        public static string Version { get; private set; } = default!;

        public static string Title { get; private set; } = default!;

        public static string LocalDataFolder { get; private set; } = default!;

        public static TService Resolve<TService>() => Container.Resolve<TService>();

        /// <summary>
        /// Executes an action on the UI thread.
        /// If this method is called from the UI thread, the action is executed immendiately.
        /// If the method is called from another thread, the action will be enqueued on the UI thread's dispatcher and executed asynchronously.
        /// </summary>
        /// <param name="action">The action that will be executed on the UI thread.</param>
        public static void InvokeAsyncOnUI(Action action)
        {
            if (UI.CheckAccess())
            {
                action();
            }
            else
            {
                UI.InvokeAsync(action);
            }
        }

        /// <summary>
        /// Open url with default browser.
        /// </summary>
        /// <param name="url">URL.</param>
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                }
            }
        }

        private static void Bootstrap()
        {// Register your types
            // Views
            App.Container.Register<MainWindow>(Reuse.Singleton);
            App.Container.Register<SettingsWindow>(Reuse.Transient);

            // ViewServices
            App.Container.RegisterMapping<IMainViewService, MainWindow>(); // App.Container.RegisterMany<MainWindow>(Reuse.Singleton);

            // ViewModels
            App.Container.Register<MainViewModel>(Reuse.Singleton);

            var errors = App.Container.Validate();
            if (errors.Length > 0)
            {
                throw new InvalidProgramException();
            }
        }

        [STAThread]
        private static void Main()
        {
            // Folder
            try
            {
                // UWP
                LocalDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            catch
            {
                // not UWP
                LocalDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppConst.AppDataFolder);
            }

            try
            {
                Directory.CreateDirectory(LocalDataFolder);
            }
            catch
            {
            }

            // C4
            try
            {
                App.C4.SetDefaultCulture(AppConst.DefaultCulture); // default culture
                App.C4.LoadAssembly("ja", "Resources.license.tinyhand"); // license
                App.C4.LoadAssembly("ja", "Resources.strings-ja.tinyhand");
                App.C4.LoadAssembly("en", "Resources.strings-en.tinyhand");
            }
            catch
            {
            }

            // Version
            try
            {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                Version = $"{version.Major}.{version.Minor}.{version.Build}";
            }
            catch
            {
                Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
            }

            // Title
            Title = App.C4["app.name"] + " " + App.Version;

            // Prevents multiple instances.
            if (!appMutex.WaitOne(0, false))
            {
                appMutex.Close(); // Release mutex.

                var prevProcess = Arc.WinAPI.Methods.GetPreviousProcess();
                if (prevProcess != null)
                {
                    var handle = prevProcess.MainWindowHandle; // The window handle that associated with the previous process.
                    if (handle == IntPtr.Zero)
                    {
                        handle = Arc.WinAPI.Methods.GetWindowHandle(prevProcess.Id, Title); // Get handle.
                    }

                    if (handle != IntPtr.Zero)
                    {
                        Arc.WinAPI.Methods.ActivateWindow(handle);
                    }
                }

                return; // Exit.
            }

            // UI Dispatcher
            Transformer.UIDispatcher = Dispatcher.CurrentDispatcher;

            // Logger: Debug, Information, Warning, Error, Fatal
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(LocalDataFolder, "log.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromMilliseconds(1000))
            /*.WriteTo.File(
                new Serilog.Formatting.Json.JsonFormatter(renderMessage: true),
                Path.Combine(LocalDataFolder, "log.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromMilliseconds(1000))*/
            .CreateLogger();

            Log.Information("App startup.");

            // Load
            var data = AppData.Load();
            Settings = data.Settings;
            Options = data.Options;

            // Set culture
            try
            {
                if (App.Settings.Culture == string.Empty)
                {
                    if (CultureInfo.CurrentUICulture.Name != "ja-JP")
                    {
                        App.Settings.Culture = "en"; // English
                    }
                }

                App.C4.ChangeCulture(App.Settings.Culture);
            }
            catch
            {
                App.Settings.Culture = AppConst.DefaultCulture;
            }

            Bootstrap();
            RunApplication();
        }

        private static void RunApplication()
        {
            try
            {
                var appClass = new AppClass();
                appClass.Start();
            }
            catch (Exception)
            {// Log the exception and exit.
            }
            finally
            {
                Log.CloseAndFlush();
                appMutex.ReleaseMutex();
                appMutex.Close();
            }
        }
    }

    /// <summary>
    /// Application Class.
    /// </summary>
    public partial class AppClass : System.Windows.Application
    {
        public void Start()
        {
            this.InitializeComponent();
            var mainWindow = App.Resolve<MainWindow>();
            this.Run(mainWindow);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Log.Information("App exit.");

            // Exit2 (After the window closes).
            var data = new AppData(App.Settings, App.Options);
            data.Save();
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Log.Information("Session ending.");
            App.SessionEnding = true;
        }

        private void Application_Activated(object sender, System.EventArgs e)
        { // application activated.
            if (!App.Initialized)
            {
                App.Initialized = true;
            }
        }
    }

    [TinyhandObject]
    public partial class AppData
    {
        [Key(0)]
        public AppSettings Settings = default!;

        [Key(1)]
        public AppOptions Options = default!;

        public AppData()
        {
        }

        public AppData(AppSettings settings, AppOptions options)
        {
            this.Settings = settings;
            this.Options = options;
        }

        public static AppData Load()
        { // Load
            AppData? appData = null;
            bool loadError = false;

            try
            {
                using (var fs = File.OpenRead(Path.Combine(App.LocalDataFolder, AppConst.AppDataFile)))
                {
                    appData = TinyhandSerializer.Deserialize<AppData>(fs);
                }
            }
            catch
            {
                loadError = true;
            }

            if (appData == null)
            {
                appData = TinyhandSerializer.Reconstruct<AppData>();
            }

            appData.Settings.LoadError = loadError;

            return appData;
        }

        public void Save()
        {
            try
            {
                var bytes = TinyhandSerializer.Serialize(this);
                using (var fs = File.Create(Path.Combine(App.LocalDataFolder, AppConst.AppDataFile)))
                {
                    fs.Write(bytes.AsSpan());
                }
            }
            catch
            {
            }
        }
    }
}
