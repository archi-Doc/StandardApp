// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

#pragma warning disable SA1208
#pragma warning disable SA1210

global using System;
global using StandardWinUI;
global using Arc.Threading;
global using Arc.Unit;
global using CrystalData;
global using Microsoft.Extensions.DependencyInjection;
global using Tinyhand;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Globalization;

namespace StandardWinUI;

#pragma warning disable SA1202

#if DISABLE_XAML_GENERATED_MAIN

public static partial class App
{
    public const string MutexName = "Arc.StandardWinUI";
    public const string AppDataFolder = "Arc\\StandardWinUI";
    public const string AppDataFile = "App.tinyhand";
    public const string DefaultCulture = "en";
    public const double DefaultFontSize = 14;

    private static void LoadStrings()
    {
        try
        {
            HashedString.SetDefaultCulture(DefaultCulture); // default culture

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            HashedString.LoadAssembly("ja", asm, "Resources.Strings.License.tinyhand"); // license
            HashedString.LoadAssembly("ja", asm, "Resources.Strings.String-ja.tinyhand");
            HashedString.LoadAssembly("en", asm, "Resources.Strings.String-en.tinyhand");
        }
        catch
        {
        }
    }

    #region FieldAndProperty

    public static string Version { get; private set; } = string.Empty;

    public static string Title { get; private set; } = string.Empty;

    public static string DataFolder { get; private set; } = string.Empty;

    public static AppSettings Settings { get; private set; } = default!;

    public static AppOptions Options { get; private set; } = default!;

    private static Mutex appMutex = new(false, MutexName);
    private static DispatcherQueue uiDispatcherQueue = default!;
    private static IServiceProvider serviceProvider = default!;
    private static Crystalizer? crystalizer;
    private static AppClass? appClass;

    #endregion

    [STAThread]
    private static void Main(string[] args)
    {
        LoadStrings();
        PrepareDataFolder();
        PrepareVersionAndTitle();
        if (PreventMultipleInstances())
        {
            return;
        }

        AppUnit.Unit? unit = default;
        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            XamlCheckProcessRequirements();
            Application.Start(_ =>
            {
                uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();
                var context = new DispatcherQueueSynchronizationContext(uiDispatcherQueue);
                SynchronizationContext.SetSynchronizationContext(context);

                var builder = new AppUnit.Builder();
                unit = builder.Build();
                serviceProvider = unit.Context.ServiceProvider;

                PrepareCrystalizer();
                PrepareCulture();
                appClass = GetService<AppClass>();
            });

            Task.Run(async () =>
            {// 'await task' does not work property.
                if (crystalizer is not null)
                {
                    await crystalizer.SaveAllAndTerminate();
                }

                ThreadCore.Root.Terminate();
                await ThreadCore.Root.WaitForTerminationAsync(-1);
                if (unit?.Context.ServiceProvider.GetService<UnitLogger>() is { } unitLogger)
                {
                    await unitLogger.FlushAndTerminate();
                }
            }).Wait();
        }
        finally
        {
            appMutex.ReleaseMutex();
            appMutex.Close();
        }
    }

    public static T GetService<T>()
        where T : class
    {
        if (serviceProvider.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in Configure within AppUnit.cs.");
        }

        return service;
    }

    public static void Exit()
    {
        appClass?.Exit();
    }

    // <summary>
    // Executes an action on the UI thread.
    // </summary>
    // <param name="action">The action that will be executed on the UI thread.</param>
    public static void TryEnqueueOnUI(DispatcherQueueHandler callback)
        => uiDispatcherQueue.TryEnqueue(callback);

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

    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();

    private static void PrepareDataFolder()
    {
        // Data Folder
        try
        {
            // UWP
            DataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }
        catch
        {
            // not UWP
            DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolder);
        }

        try
        {
            Directory.CreateDirectory(DataFolder);
        }
        catch
        {
        }
    }

    private static void PrepareVersionAndTitle()
    {
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
        Title = HashedString.Get(Hashed.App.Name) + " " + Version;
    }

    private static bool PreventMultipleInstances()
    {
        if (appMutex.WaitOne(0, false))
        {
            return false;
        }

        appMutex.Close(); // Release mutex.

        var prevProcess = Arc.Internal.Methods.GetPreviousProcess();
        if (prevProcess != null)
        {
            var handle = prevProcess.MainWindowHandle; // The window handle that associated with the previous process.
            if (handle == IntPtr.Zero)
            {
                handle = Arc.Internal.Methods.GetWindowHandle(prevProcess.Id, Title); // Get handle.
            }

            if (handle != IntPtr.Zero)
            {
                Arc.Internal.Methods.ActivateWindow(handle);
            }
        }

        return true;
    }

    private static void PrepareCrystalizer()
    {
        crystalizer = GetService<Crystalizer>();
        crystalizer.PrepareAndLoadAll(false).Wait();

        // Load settings and options.
        Settings = crystalizer.GetCrystal<AppSettings>().Data;
        Options = crystalizer.GetCrystal<AppOptions>().Data;
    }

    private static void PrepareCulture()
    {
        try
        {
            if (Settings.Culture == string.Empty)
            {
                if (CultureInfo.CurrentUICulture.Name != "ja-JP")
                {
                    Settings.Culture = "en"; // English
                }
            }

            HashedString.ChangeCulture(App.Settings.Culture);
        }
        catch
        {
            Settings.Culture = App.DefaultCulture;
            HashedString.ChangeCulture(Settings.Culture);
        }
    }
}
#endif
