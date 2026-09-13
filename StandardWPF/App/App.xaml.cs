// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

#pragma warning disable SA1208 // System using directives should be placed before other using directives
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
global using System;
global using Tinyhand;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Arc.WPF;
using DryIoc; // alternative: SimpleInjector
using Serilog;
using StandardWPF;
using StandardWPF.Views;
using StandardWPF.ViewServices;

#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1600 // Elements should be documented

namespace Application;

/// <summary>
/// Provides WPF startup, shared services, localization, and application settings.
/// </summary>
public static partial class App
{
    private static System.Threading.Mutex appMutex = new System.Threading.Mutex(false, AppConst.MutexName);

    static App()
    {
    }

    public static bool IsInitialized { get; set; } = false; // Application initialized

    public static bool IsSessionEnding { get; set; } = false; // Session ending

    public static Dispatcher UIDispatcher { get; } = Dispatcher.CurrentDispatcher; // UI dispatcher

    public static Container Container { get; } = new DryIoc.Container(); // DI container

    public static AppSettings Settings { get; private set; } = new();

    public static AppOptions Options { get; private set; } = new();

    public static string Version { get; private set; } = string.Empty;

    public static string Title { get; private set; } = string.Empty;

    public static string DataDirectory { get; private set; } = string.Empty;

    public static TService Resolve<TService>() => Container.Resolve<TService>();

    /// <summary>
    /// Executes an action on the UI thread.
    /// If this method is called from the UI thread, the action is executed immediately.
    /// If the method is called from another thread, the action will be enqueued on the UI thread's dispatcher and executed asynchronously.
    /// </summary>
    /// <param name="action">The action that will be executed on the UI thread.</param>
    public static void ExecuteOrEnqueueOnUI(Action action)
    {
        if (UIDispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            UIDispatcher.InvokeAsync(action);
        }
    }

    /// <summary>
    /// Opens an absolute HTTP or HTTPS URL in the default browser.
    /// </summary>
    /// <param name="url">The absolute HTTP or HTTPS URL to open.</param>
    public static void OpenBrowser(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("An absolute HTTP or HTTPS URL is required.", nameof(url));
        }

        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
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
        // Data directory
        try
        {
            // UWP
            DataDirectory = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }
        catch
        {
            // not UWP
            DataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppConst.DataFolderName);
        }

        try
        {
            Directory.CreateDirectory(DataDirectory);
        }
        catch
        {
        }

        // Stringer
        try
        {
            HashedString.SetDefaultCulture(AppConst.DefaultCulture); // default culture

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            HashedString.LoadAssembly("ja", asm, "Resources.license.tinyhand"); // license
            HashedString.LoadAssembly("ja", asm, "Resources.strings-ja.tinyhand");
            HashedString.LoadAssembly("en", asm, "Resources.strings-en.tinyhand");
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
        Title = HashedString.Get(Hashed.App.Name) + " " + App.Version;

        // Prevents multiple instances.
        bool ownsMutex;
        try
        {
            ownsMutex = appMutex.WaitOne(0, false);
        }
        catch (System.Threading.AbandonedMutexException)
        {
            ownsMutex = true;
        }

        if (!ownsMutex)
        {
            appMutex.Close(); // Release mutex.

            using var prevProcess = Arc.WinAPI.NativeMethods.GetPreviousProcess();
            if (prevProcess != null)
            {
                var handle = prevProcess.MainWindowHandle; // The window handle that associated with the previous process.
                if (handle == IntPtr.Zero)
                {
                    handle = Arc.WinAPI.NativeMethods.GetWindowHandle(prevProcess.Id, Title); // Get handle.
                }

                if (handle != IntPtr.Zero)
                {
                    Arc.WinAPI.NativeMethods.ActivateWindow(handle);
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
            Path.Combine(DataDirectory, "log.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 31,
            buffered: true,
            flushToDiskInterval: TimeSpan.FromMilliseconds(1000))
        /*.WriteTo.File(
            new Serilog.Formatting.Json.JsonFormatter(renderMessage: true),
            Path.Combine(DataDirectory, "log.json"),
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
                App.Settings.Culture = CultureInfo.CurrentUICulture.Name == "ja-JP" ? "ja" : "en";
            }

            HashedString.TrySetCurrentCulture(App.Settings.Culture);
        }
        catch
        {
            App.Settings.Culture = AppConst.DefaultCulture;
            HashedString.TrySetCurrentCulture(App.Settings.Culture);
        }

        Bootstrap();
        RunApplication();
    }

    private static void RunApplication()
    {
        try
        {
            var application = new WpfApplication();
            application.Start();
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
/// Runs the WPF main window and persists data at application shutdown.
/// </summary>
public partial class WpfApplication : System.Windows.Application
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
        App.IsSessionEnding = true;
    }

    private void Application_Activated(object sender, System.EventArgs e)
    { // application activated.
        if (!App.IsInitialized)
        {
            App.IsInitialized = true;
        }
    }
}

/// <summary>
/// Loads and saves the WPF application's settings and options.
/// </summary>
[TinyhandObject]
public partial class AppData
{
    [Key(0)]
    public AppSettings Settings = new();

    [Key(1)]
    public AppOptions Options = new();

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
            using (var fs = File.OpenRead(Path.Combine(App.DataDirectory, AppConst.DataFileName)))
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

        appData.Settings.HasLoadError = loadError;

        return appData;
    }

    public void Save()
    {
        try
        {
            var bytes = TinyhandSerializer.Serialize(this);
            using (var fs = File.Create(Path.Combine(App.DataDirectory, AppConst.DataFileName)))
            {
                fs.Write(bytes.AsSpan());
            }
        }
        catch
        {
        }
    }
}
