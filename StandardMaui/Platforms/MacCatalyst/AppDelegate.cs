// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Foundation;

namespace StandardMaui;

/// <summary>
/// Creates the MAUI application for Apple platforms.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
