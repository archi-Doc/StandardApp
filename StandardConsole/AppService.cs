﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.IO;
using System.Threading;
using Serilog;

namespace StandardConsole;

public interface IAppService
{
    void EnterCommand(string directory);

    void ExitCommand();
}

public class AppService : IAppService
{
    public void EnterCommand(string directory)
    {
        // Logger: Debug, Information, Warning, Error, Fatal
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(directory, "logs", "log.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 31,
            buffered: true,
            flushToDiskInterval: TimeSpan.FromMilliseconds(1000))
        .CreateLogger();

        Log.Information("started");
    }

    public void ExitCommand()
    {
        Log.Information("terminated");
        Log.CloseAndFlush();
    }
}
