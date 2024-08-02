// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace StandardWinUI;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Arc.Threading;
using Arc.Unit;
using CrystalData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using SimpleCommandLine;
using Tinyhand;

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

    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();

    #region FieldAndProperty

    public static string Version { get; private set; } = string.Empty;

    public static string Title { get; private set; } = string.Empty;

    public static string DataFolder { get; private set; } = string.Empty;

    private static Mutex appMutex = new(false, MutexName);
    private static DispatcherQueue uiDispatcherQueue = default!;
    private static IServiceProvider serviceProvider = default!;

    #endregion

    [STAThread]
    private static async Task Main(string[] args)
    {
        PrepareDataFolder();
        LoadStrings();

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
                serviceProvider.GetService<AppClass>();
            });
        }
        finally
        {
            ThreadCore.Root.Terminate();
            await ThreadCore.Root.WaitForTerminationAsync(-1); // Wait for the termination infinitely.
            unit?.Context.ServiceProvider.GetService<UnitLogger>()?.FlushAndTerminate();

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
}
#endif
