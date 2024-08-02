// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace StandardWinUI;

public partial class AppClass : Application
{
    public AppClass()
    {
        this.InitializeComponent();

        /*var builder = new AppUnit.Builder()
            .Configure(context =>
            {
                // Add Command
            });

        var args = SimpleParserHelper.GetCommandLineArguments();
        var unit = builder.Build();*/
        // await unit.RunAsync(new(args));

        // ThreadCore.Root.Terminate();
        // await ThreadCore.Root.WaitForTerminationAsync(-1); // Wait for the termination infinitely.
        // unit.Context.ServiceProvider.GetService<UnitLogger>()?.FlushAndTerminate();
        // ThreadCore.Root.TerminationEvent.Set(); // The termination process is complete (#1).
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        this.window = new MainWindow();
        this.window.Activate();
    }

    private Window window = default!;
}
