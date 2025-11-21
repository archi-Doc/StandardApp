// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Arc;
using Arc.Threading;
using Arc.Unit;
using Microsoft.Extensions.DependencyInjection;
using SimpleCommandLine;

namespace StandardConsole;

public class Program
{
    public static async Task Main()
    {
        AppCloseHandler.Set(() =>
        {// Console window closing or process terminated.
            ThreadCore.Root.Terminate(); // Send a termination signal to the root.
            ThreadCore.Root.TerminationEvent.WaitOne(2000); // Wait until the termination process is complete (#1).
        });

        Console.CancelKeyPress += (s, e) =>
        {// Ctrl+C pressed
            e.Cancel = true;
            ThreadCore.Root.Terminate(); // Send a termination signal to the root.
        };

        var builder = new ConsoleUnit.Builder()
            .Configure(context =>
            {
                // Add Command
            });

        var args = SimpleParserHelper.GetCommandLineArguments();
        var unit = builder.Build();
        await unit.RunAsync(new(args));

        ThreadCore.Root.Terminate();
        await ThreadCore.Root.WaitForTerminationAsync(-1); // Wait for the termination infinitely.
        unit.Context.ServiceProvider.GetService<UnitLogger>()?.FlushAndTerminate();
        ThreadCore.Root.TerminationEvent.Set(); // The termination process is complete (#1).
    }
}
