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
    private static ExecutionRoot? root;

    public static async Task Main()
    {
        AppCloseHandler.Set(() =>
        {// Closing the console window or terminating the process.
            root?.RequestTermination(); // Send a termination signal to the root.
            root?.WaitForTermination(TimeSpan.FromSeconds(2)).Wait();
        });

        Console.CancelKeyPress += (s, e) =>
        {// Ctrl+C pressed.
            e.Cancel = true;
            root?.RequestTermination(); // Send a termination signal to the root.
        };

        var builder = new ConsoleUnit.Builder()
            .Configure(context =>
            {
                // Add Command
            });

        var args = SimpleParserHelper.GetCommandLineArguments();
        var unit = builder.Build();
        root = unit.Context.ExecutionRoot;

        await unit.RunAsync(new(args));

        root.RequestTermination();
        await root.WaitForTermination(); // Wait for the termination infinitely.
        if (unit.Context.ServiceProvider.GetService<LogUnit>() is { } unitLogger)
        {
            await unitLogger.FlushAndTerminate();
        }
    }
}
