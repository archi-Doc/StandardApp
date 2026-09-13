// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Arc.Threading;
using Arc.Unit;
using SimpleCommandLine;

namespace StandardConsole;

/// <summary>
/// Contains the delay option for the sample command.
/// </summary>
public class TestOptions
{
    [SimpleOption("number", ShortName = "n")]
    public int Number { get; set; } = 2000;
}

/// <summary>
/// Runs the sample command and waits for its worker to terminate.
/// </summary>
[SimpleCommand("test")]
public class TestCommand : ISimpleCommand<TestOptions>
{
    private readonly ExecutionRoot root;
    private readonly IConsoleService consoleService;

    public TestCommand(ExecutionRoot root, ILogger<TestCommand> logger, IConsoleService consoleService)
    {
        this.root = root;
        this.logger = logger;
        this.consoleService = consoleService;
    }

    public async Task Execute(TestOptions options, string[] args, CancellationToken cancellationToken)
    {
        this.consoleService.WriteLine("Test command:", ConsoleColor.Red);
        Console.WriteLine($"Number is {options.Number}");

        var c = new ThreadCore(this.root, parameter =>
        {
            var core = (ThreadCore)parameter!;
            try
            {
                Task.Delay(options.Number, core.CancellationToken).Wait();
            }
            catch
            {
                this.logger.GetWriter()?.Write("canceled");
            }
        });

        await c.WaitForTerminationAsync();
    }

    private readonly ILogger logger;
}
