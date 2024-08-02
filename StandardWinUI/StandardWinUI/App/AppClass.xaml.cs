// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.Unit;
using Microsoft.UI.Xaml;

namespace StandardWinUI;

public partial class AppClass : Application
{
    public AppClass(ILogger<AppClass> logger)
    {
        this.InitializeComponent();

        this.logger = logger;
        this.logger.TryGet()?.Log("Start");
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        this.window = App.GetService<MainWindow>();
        this.window.Activate();
    }

    private ILogger logger;
    private Window window = default!;
}
