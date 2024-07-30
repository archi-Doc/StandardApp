// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using SimpleCommandLine;

namespace StandardConsole;

[SimpleCommand("test2")]
public class TestCommand2 : ISimpleCommand
{
    public void Run(string[] args)
    {
        Console.WriteLine("Test command2:");
    }
}
