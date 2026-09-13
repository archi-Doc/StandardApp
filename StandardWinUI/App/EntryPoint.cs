// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace StandardWinUI;

#if DISABLE_XAML_GENERATED_MAIN

/// <summary>
/// Initializes WinUI services and coordinates single-instance startup and shutdown.
/// </summary>
public static partial class EntryPoint
{
    public static DispatcherQueue UIDispatcherQueue { get; private set; } = default!;

    public static string DataDirectory { get; private set; } = string.Empty;

    private static Mutex? appMutex = string.IsNullOrEmpty(App.MutexName) ? default : new(false, App.MutexName);

    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    [STAThread]
    private static void Main(string[] args)
    {
        PrepareDataDirectory();
        if (appMutex is not null &&
            UIHelper.TryActivateRunningInstance(appMutex))
        {
            return;
        }

        AppUnit.Product? unit = default;
        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            XamlCheckProcessRequirements(); // If an exception occurs here, run the Package project or set WindowsAppSDKSelfContained to true.
            Application.Start(_ =>
            {
                UIDispatcherQueue = DispatcherQueue.GetForCurrentThread();
                var context = new DispatcherQueueSynchronizationContext(UIDispatcherQueue);
                SynchronizationContext.SetSynchronizationContext(context);

                var builder = new AppUnit.Builder();
                unit = builder.Build();
                var serviceProvider = unit.Context.ServiceProvider;
                var app = serviceProvider.GetRequiredService<IApp>();
                var application = app.GetApplication(); // Create an application instance.
            });

            Task.Run(async () =>
            {// 'await task' does not work properly.
                if (unit is null)
                {
                    return;
                }

                if (unit.Context.ServiceProvider.GetService<CrystalControl>() is { } crystalControl)
                {
                    await crystalControl.StoreAndRip();
                }

                unit.Context.ExecutionRoot.RequestTermination();
                await unit.Context.ExecutionRoot.WaitForTerminationAsync();
                if (unit.Context.ServiceProvider.GetService<LogUnit>() is { } unitLogger)
                {
                    await unitLogger.FlushAndTerminateAsync();
                }
            }).Wait();
        }
        finally
        {
            if (appMutex is not null)
            {
                appMutex.ReleaseMutex();
                appMutex.Close();
            }
        }
    }

    private static void PrepareDataDirectory()
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
            DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.DataFolderName);
        }

        try
        {
            Directory.CreateDirectory(DataDirectory);
        }
        catch
        {
        }
    }

    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();
}

#endif
