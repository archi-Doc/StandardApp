// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace StandardWinUI;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Tinyhand;

#if DISABLE_XAML_GENERATED_MAIN

public static partial class Program
{
    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();

    #region FieldAndProperty

    public static string Version { get; private set; } = string.Empty;

    public static string Title { get; private set; } = string.Empty;

    private static Mutex appMutex = new(false, AppConstants.MutexName);

    #endregion

    [STAThread]
    private static async Task Main(string[] args)
    {
        // C4
        try
        {
            HashedString.SetDefaultCulture(AppConstants.DefaultCulture); // default culture

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            HashedString.LoadAssembly("ja", asm, "Resources.Strings.License.tinyhand"); // license
            HashedString.LoadAssembly("ja", asm, "Resources.Strings.String-ja.tinyhand");
            HashedString.LoadAssembly("en", asm, "Resources.Strings.String-en.tinyhand");
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
        Title = HashedString.Get(Hashed.App.Name) + " " + Version;

        /*bool isRedirect = await DecideRedirection();
        if (isRedirect)
        {
            return;
        }*/

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

        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            XamlCheckProcessRequirements();
            Application.Start(_ =>
            {
                try
                {
                    var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    var app = new App();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in application start callback: {ex.Message}.");
                }
            });
        }
        finally
        {
            appMutex.ReleaseMutex();
            appMutex.Close();
        }
    }

    /*private static async Task<bool> DecideRedirection()
    {
        var isRedirect = false;
        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
        var kind = args.Kind;
        var keyInstance = AppInstance.FindOrRegisterForKey("randomKey");

        if (keyInstance.IsCurrent)
        {
            keyInstance.Activated += OnActivated;
        }
        else
        {
            isRedirect = true;
            await keyInstance.RedirectActivationToAsync(args);
        }

        return isRedirect;
    }

    private static void OnActivated(object sender, AppActivationArguments args)
    {
        ExtendedActivationKind kind = args.Kind;
    }*/
}
#endif
