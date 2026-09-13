// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace StandardMaui;

/// <summary>
/// Initializes the application and creates its main window.
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    /// <inheritdoc/>
    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(new AppShell());
}
