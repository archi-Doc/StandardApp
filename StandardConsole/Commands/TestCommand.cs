// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Arc.Threading;
using Arc.Unit;
using SimpleCommandLine;

namespace StandardConsole;

public class TestOptions
{
    [SimpleOption("number", ShortName = "n")]
    public int Number { get; set; } = 2000;
}

[SimpleCommand("test")]
public class TestCommand : ISimpleCommandAsync<TestOptions>
{
    private readonly IConsoleService consoleService;

    public TestCommand(ILogger<TestCommand> logger, IConsoleService consoleService)
    {
        this.logger = logger;
        this.consoleService = consoleService;
    }

    public async Task RunAsync(TestOptions option, string[] args)
    {
        this.consoleService.WriteLine("Test command:", ConsoleColor.Red);
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
                this.logger.TryGet()?.Log("canceled");
            }
        });

        await c.WaitForTerminationAsync(-1);
    }

    private readonly ILogger logger;
}
