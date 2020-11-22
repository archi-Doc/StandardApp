﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Arc.Text;
using Arc.WinAPI;
using Arc.WPF;
using DryIoc; // alternative: SimpleInjector
using Serilog;
using StandardApp;
using StandardApp.Views;
using StandardApp.ViewServices;
using Tinyhand;

#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1402
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
        {
            // Register your types:

            // Register your windows and view models:
            App.Container.Register<MainWindow>(Reuse.Singleton);
            App.Container.RegisterMapping<IMainViewService, MainWindow>(); // App.Container.RegisterMany<MainWindow>(Reuse.Singleton);
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

            // Version, Title
            Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
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

            // Logger: Debug, Information, Warning, Error, Fatal
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                "log.txt",
                rollingInterval: RollingInterval.Day,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromMilliseconds(1000))
            .CreateLogger();

            Log.Information("App startup.");

            // Load
            var data = AppData.Load();
            Settings = data.Settings;
            Options = data.Options;

            // Set culture
            try
            {
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
                using (var fs = File.OpenRead(AppConst.AppDataFile))
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
                using (var fs = File.Create(AppConst.AppDataFile))
                {
                    fs.Write(bytes.AsSpan());
                }
            }
            catch
            {
            }
        }
    }

    [TinyhandObject]
    public partial class AppSettings : ITinyhandSerializationCallback
    {// Application Settings
        [Key(0)]
        public bool LoadError { get; set; } // True if a loading error has occured.

        [Key(1)]
        public DipWindowPlacement WindowPlacement { get; set; } = default!;

        [Key(2)]
        public string Culture { get; set; } = AppConst.DefaultCulture; // Default culture

        [Key(3)]
        public double DisplayScaling { get; set; } = 1.0d; // Display Scaling

        public void OnAfterDeserialize()
        {
            Transformer.Instance.ScaleX = this.DisplayScaling;
            Transformer.Instance.ScaleY = this.DisplayScaling;
        }

        public void OnBeforeSerialize()
        {
        }
    }

    [TinyhandObject]
    public partial class AppOptions
    { // Application Options
        public AppOptions()
        {
        }

        [Key(0)]
        public BrushOption BrushTest { get; set; } = new BrushOption();

        [Key(1)]
        public BrushCollection BrushCollection { get; set; } = new BrushCollection(); // Brush Collection

        public void Reconstruct()
        {
            this.BrushTest.Prepare(Colors.Red);
        }
    }

    [TinyhandObject]
    public partial class BrushCollection : ITinyhandSerializationCallback
    {
        [Key(0)]
        public BrushOption Brush1 { get; set; } = null!;

        public BrushOption this[string name]
        {
            get
            {
                return this.Brush1;
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            this.Brush1.Prepare(Colors.BurlyWood);
        }
    }
}
