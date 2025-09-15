﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arc.Threading;
using Arc.Unit;
using SimpleCommandLine;
using static SimpleCommandLine.SimpleParser;

namespace StandardConsole;

public class ConsoleUnit : UnitBase, IUnitPreparable, IUnitExecutable
{
    public class Builder : UnitBuilder<Unit>
    {// Builder class for customizing dependencies.
        public Builder()
            : base()
        {
            // Configuration for Unit.
            this.Configure(context =>
            {
                context.AddSingleton<ConsoleUnit>();
                context.RegisterInstanceCreation<ConsoleUnit>();

                // Command
                context.AddCommand(typeof(TestCommand));
                context.AddCommand(typeof(TestCommand2));

                // Log filter
                context.AddSingleton<ExampleLogFilter>();

                // Logger
                context.ClearLoggerResolver();
                context.AddLoggerResolver(x =>
                {// Log source/level -> Resolver() -> Output/filter
                    if (x.LogLevel <= LogLevel.Debug)
                    {
                        x.SetOutput<ConsoleLogger>();
                        return;
                    }

                    x.SetOutput<ConsoleAndFileLogger>();

                    if (x.LogSourceType == typeof(TestCommand))
                    {
                        x.SetFilter<ExampleLogFilter>();
                    }
                });
            });

            this.PostConfigure(context =>
            {
                context.SetOptions(context.GetOptions<UnitOptions>() with
                {
                    DataDirectory = "test",
                });

                var logfile = "Logs/Log.txt";
                context.SetOptions(context.GetOptions<FileLoggerOptions>() with
                {
                    Path = Path.Combine(context.DataDirectory, logfile),
                    MaxLogCapacity = 2,
                    ClearLogsAtStartup = false,
                });
            });
        }
    }

    public class Unit : BuiltUnit
    {// Unit class for customizing behaviors.
        public record Param(string Args);

        public Unit(UnitContext context)
            : base(context)
        {
        }

        public async Task RunAsync(Param param)
        {
            // Create optional instances
            this.Context.CreateInstances();

            this.Context.SendPrepare(new());
            await this.Context.SendStartAsync(new(ThreadCore.Root));

            var parserOptions = SimpleParserOptions.Standard with
            {
                ServiceProvider = this.Context.ServiceProvider,
                RequireStrictCommandName = false,
                RequireStrictOptionName = true,
            };

            // Main
            // await SimpleParser.ParseAndRunAsync(this.Context.Commands, "example -string test", parserOptions);
            await SimpleParser.ParseAndRunAsync(this.Context.Commands, param.Args, parserOptions);

            this.Context.SendStop(new());
            await this.Context.SendTerminateAsync(new());
        }
    }

    private class ExampleLogFilter : ILogFilter
    {
        public ExampleLogFilter(ConsoleUnit consoleUnit)
        {
            this.consoleUnit = consoleUnit;
        }

        public ILogWriter? Filter(LogFilterParameter param)
        {// Log source/Event id/LogLevel -> Filter() -> ILog
            if (param.LogSourceType == typeof(TestCommand))
            {
                // return null; // No log
                if (param.LogLevel == LogLevel.Error)
                {
                    return param.Context.TryGet<ConsoleAndFileLogger>(LogLevel.Fatal); // Error -> Fatal
                }
                else if (param.LogLevel == LogLevel.Fatal)
                {
                    return param.Context.TryGet<ConsoleAndFileLogger>(LogLevel.Error); // Fatal -> Error
                }
            }

            return param.OriginalLogger;
        }

        private ConsoleUnit consoleUnit;
    }

    public ConsoleUnit(UnitContext context, ILogger<ConsoleUnit> logger, UnitOptions options)
        : base(context)
    {
        this.logger = logger;
        this.options = options;
    }

    void IUnitPreparable.Prepare(UnitMessage.Prepare message)
    {
        this.logger.TryGet()?.Log("Unit prepared.");
        this.logger.TryGet()?.Log($"Program: {this.options.ProgramDirectory}");
        this.logger.TryGet()?.Log($"Data: {this.options.DataDirectory}");
    }

    async Task IUnitExecutable.StartAsync(UnitMessage.StartAsync message, CancellationToken cancellationToken)
    {
        this.logger.TryGet()?.Log("Unit started.");
    }

    void IUnitExecutable.Stop(UnitMessage.Stop message)
    {
        this.logger.TryGet()?.Log("Unit stopped.");
    }

    async Task IUnitExecutable.TerminateAsync(UnitMessage.TerminateAsync message, CancellationToken cancellationToken)
    {
        this.logger.TryGet()?.Log("Unit terminated.");
    }

    private ILogger<ConsoleUnit> logger;
    private UnitOptions options;
}
