// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Arc.Threading;
using Serilog;
using SimpleCommandLine;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace StandardConsole;

public class TestOptions
{
    [SimpleOption("number", ShortName = "n")]
    public int Number { get; set; } = 2000;
}

[SimpleCommand("test")]
public class TestCommand : ISimpleCommandAsync<TestOptions>
{
    public TestCommand(IAppService appService)
    {
        this.AppService = appService;
    }

    public async Task RunAsync(TestOptions option, string[] args)
    {
        this.AppService.EnterCommand(string.Empty);

        Console.WriteLine("Test command:");
        Console.WriteLine($"Number is {option.Number}");

        var c = new ThreadCore(ThreadCore.Root, parameter =>
        {
            var core = (ThreadCore)parameter!;
            try
            {
                Task.Delay(option.Number, ThreadCore.Root.CancellationToken).Wait();
            }
            catch
            {
                Log.Information("canceled");
            }
        });

        await c.WaitForTerminationAsync(-1);
        this.AppService.ExitCommand();
    }

    public IAppService AppService { get; }
}
