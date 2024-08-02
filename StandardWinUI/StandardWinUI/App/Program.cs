// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace StandardWinUI;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

#if DISABLE_XAML_GENERATED_MAIN

public static partial class Program
{
    [LibraryImport("Microsoft.ui.xaml.dll")]
    private static partial void XamlCheckProcessRequirements();

    #region FieldAndProperty

    private static Mutex appMutex = new(false, AppConstants.MutexName);

    #endregion

    [STAThread]
    private static void Main(string[] args)
    {
        XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();
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
}
#endif
