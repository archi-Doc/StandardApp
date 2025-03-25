// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace StandardWinUI;

public partial class StandardApp : Application
{
    public StandardApp()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        this.window = App.GetMainWindow();
        this.window.Activate();
    }

    private Window? window;
}
