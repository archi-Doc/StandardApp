// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleCommandLine;

namespace StandardConsole;

[SimpleCommand("test2")]
public class TestCommand2 : ISimpleCommand
{
    public async Task Execute(string[] args, CancellationToken cancellationToken)
    {
        Console.WriteLine("Test command2:");
    }
}
