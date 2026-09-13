// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using SimpleCommandLine;
using StandardWinUI.PresentationState;

namespace StandardWinUI;

/// <summary>
/// AppUnit is a class that manages the dependencies of the DI container, logs, and CrystalData (data persistence).
/// </summary>
public class AppUnit : UnitBase, IUnitPreparable, IUnitExecutable
{
    public class Builder : UnitBuilder<Product>
    {// Builder class for customizing dependencies.
        public Builder()
            : base()
        {
            // Configuration for Unit.
            this.PreConfigure(context =>
            {
                context.ProgramDirectory = Entrypoint.DataFolder;
                context.DataDirectory = Entrypoint.DataFolder;
            });

            this.Configure(context =>
            {
                // context.AddSingleton<AppUnit>();
                context.AddSingleton<StandardApp>();
                context.AddSingleton<IApp, App>();
                // context.Services.AddSingleton(x => (App)x.GetRequiredService<IApp>()); // If you want to use the App instance, please uncomment it.

                // Presentation-State
                context.AddSingleton<NaviWindow>();
                context.AddSingleton<HelloPage>();
                context.AddSingleton<BaibainPage>();
                context.AddSingleton<StatePage>();
                context.AddSingleton<StatePageState>();
                context.AddSingleton<MessagePage>();
                context.AddSingleton<MessagePageState>();
                context.AddSingleton<AdvancedPage>();
                context.AddSingleton<AdvancedPageState>();
                context.AddSingleton<SettingsPage>();
                context.AddSingleton<SettingsState>();
                context.AddSingleton<InformationPage>();
                context.AddSingleton<InformationState>();

                // Command
                // context.AddCommand(typeof(TestCommand));
                // context.AddCommand(typeof(TestCommand2));

                // Log filter
                context.AddSingleton<ExampleLogFilter>();

                // Logger
                context.ClearLogOutputResolvers();
                context.AddLogOutputResolver(x =>
                {// Log source/level -> Resolver() -> Output/filter
                    x.SetOutput<FileLogOutput<FileLogOutputOptions>>();

                    // if (x.LogLevel <= LogLevel.Debug)
                    // {
                    //    x.SetOutput<ConsoleLogOutput>();
                    //    return;
                    // }

                    // x.SetOutput<ConsoleAndFileLogOutput>();

                    // if (x.LogSourceType == typeof(TestCommand))
                    // {
                    //    x.SetFilter<ExampleLogFilter>();
                    // }
                });
            });

            this.PostConfigure(context =>
            {
                var logfile = "Logs/Log.txt";
                context.SetOptions(context.GetOrCreateOptions<FileLogOutputOptions>() with
                {
                    FilePath = Path.Combine(context.DataDirectory, logfile),
                    MaxLogCapacityInMegabytes = 2,
                    ClearLogsAtStartup = false,
                });
            });

            this.AddBuilder(CrystalBuilder());
        }

        private static CrystalUnit.Builder CrystalBuilder()
        {
            return new CrystalUnit.Builder()
                .ConfigureCrystal(context =>
                {
                    context.AddCrystal<AppSettings>(new()
                    {
                        NumberOfHistoryFiles = 0,
                        FileConfiguration = new GlobalFileConfiguration(AppSettings.Filename),
                        SaveFormat = SaveFormat.Utf8,
                    });
                });
        }
    }

    public class Product : UnitProduct
    {// Unit class for customizing behaviors.
        public record Param(string Args);

        public Product(UnitContext context)
            : base(context)
        {
        }

        public async Task RunAsync(Param param)
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
            await SimpleParser.ParseAndExecute(this.Context.CommandTypes, param.Args, parserOptions);

            await this.Context.SendStopAsync();
            await this.Context.SendTerminateAsync();
        }
    }

    private class ExampleLogFilter : ILogFilter
    {
        public ExampleLogFilter(AppUnit consoleUnit)
        {
            this.consoleUnit = consoleUnit;
        }

        public LogWriter? Filter(LogFilterContext param)
        {// Log source/Event id/LogLevel -> Filter() -> ILog
            if (param.LogSourceType == typeof(StandardApp))
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

        private AppUnit consoleUnit;
    }

    public AppUnit(UnitContext context, ILogger<AppUnit> logger, UnitOptions options)
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

    private readonly ILogger logger;
    private readonly UnitOptions options;
}
