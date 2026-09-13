// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Android.App;
using Android.Runtime;

namespace StandardMaui;

/// <summary>
/// Creates the MAUI application for the Android process.
/// </summary>
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
