// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arc.Threading;
using Arc.Unit;
using SimpleCommandLine;

namespace StandardConsole;

public class ConsoleUnit : UnitBase, IUnitPreparable, IUnitExecutable
{
    public class Builder : UnitBuilder<Product>
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
                context.AddCommand(typeof(Test2Command));

                // Log filter
                context.AddSingleton<ExampleLogFilter>();

                // Logger
                context.ClearLogOutputResolvers();
                context.AddLogOutputResolver(x =>
                {// Log source/level -> Resolver() -> Output/filter
                    if (x.LogLevel <= LogLevel.Debug)
                    {
                        x.SetOutput<ConsoleLogOutput>();
                        return;
                    }

                    x.SetOutput<ConsoleAndFileLogOutput>();

                    if (x.LogSourceType == typeof(TestCommand))
                    {
                        x.SetFilter<ExampleLogFilter>();
                    }
                });
            });

            this.PostConfigure(context =>
            {
                context.DataDirectory = "test";

                var logfile = "Logs/Log.txt";
                context.SetOptions(context.GetOrCreateOptions<FileLogOutputOptions>() with
                {
                    FilePath = Path.Combine(context.DataDirectory, logfile),
                    MaxLogCapacityInMegabytes = 2,
                    ClearLogsAtStartup = false,
                });
            });
        }
    }

    public class Product : UnitProduct
    {// Unit class for customizing behaviors.
        public record RunParameters(string Arguments);

        public Product(UnitContext context)
            : base(context)
        {
        }

        public async Task RunAsync(RunParameters parameters)
        {
            // Create optional instances
            this.Context.CreateInstances();

            await this.Context.SendPrepareAsync();
            await this.Context.SendStartAsync();

            var parserOptions = SimpleParserOptions.Standard with
            {
                ServiceProvider = this.Context.ServiceProvider,
                RequireCommandName = false,
                RejectUnknownOptionNames = true,
            };

            // Main
            // await SimpleParser.ParseAndRunAsync(this.Context.CommandTypes, "example -string test", parserOptions);
            await SimpleParser.ParseAndExecute(this.Context.CommandTypes, parameters.Arguments, parserOptions);

            await this.Context.SendStopAsync();
            await this.Context.SendTerminateAsync();
        }
    }

    private class ExampleLogFilter : ILogFilter
    {
        public ExampleLogFilter(ConsoleUnit consoleUnit)
        {
            this.consoleUnit = consoleUnit;
        }

        public LogWriter? Filter(LogFilterContext param)
        {// Log source/Event id/LogLevel -> Filter() -> ILog
            if (param.LogSourceType == typeof(TestCommand))
            {
                // return null; // No log
                if (param.LogLevel == LogLevel.Error)
                {
                    return param.LogService.GetWriter<ConsoleAndFileLogOutput>(LogLevel.Fatal); // Error -> Fatal
                }
                else if (param.LogLevel == LogLevel.Fatal)
                {
                    return param.LogService.GetWriter<ConsoleAndFileLogOutput>(LogLevel.Error); // Fatal -> Error
                }
            }

            return param.OriginalWriter;
        }

        private ConsoleUnit consoleUnit;
    }

    public ConsoleUnit(UnitContext context, ILogger<ConsoleUnit> logger, UnitOptions options)
        : base(context)
    {
        this.logger = logger;
        this.options = options;
    }

    async Task IUnitPreparable.PrepareAsync(UnitContext unitContext, CancellationToken cancellationToken)
    {
        this.logger.GetWriter()?.Write("Unit prepared.");
        this.logger.GetWriter()?.Write($"Program: {this.options.ProgramDirectory}");
        this.logger.GetWriter()?.Write($"Data: {this.options.DataDirectory}");
    }

    async Task IUnitExecutable.StartAsync(UnitContext unitContext, CancellationToken cancellationToken)
    {
        this.logger.GetWriter()?.Write("Unit started.");
    }

    async Task IUnitExecutable.StopAsync(UnitContext unitContext, CancellationToken cancellationToken)
    {
        this.logger.GetWriter()?.Write("Unit stopped.");
    }

    async Task IUnitExecutable.TerminateAsync(UnitContext unitContext, CancellationToken cancellationToken)
    {
        this.logger.GetWriter()?.Write("Unit terminated.");
    }

    private ILogger<ConsoleUnit> logger;
    private UnitOptions options;
}
