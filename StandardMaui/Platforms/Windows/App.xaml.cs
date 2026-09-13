// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StandardMaui.WinUI;

/// <summary>
/// Creates and hosts the MAUI application on Windows.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
