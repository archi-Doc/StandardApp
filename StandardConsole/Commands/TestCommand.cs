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
    public TestCommand(ILogger<TestCommand> logger)
    {
        this.logger = logger;
    }

    public async Task RunAsync(TestOptions option, string[] args)
    {
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
                this.logger.TryGet()?.Log("canceled");
            }
        });

        await c.WaitForTerminationAsync(-1);
    }

    private readonly ILogger logger;
}
