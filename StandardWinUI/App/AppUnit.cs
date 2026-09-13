// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using SimpleCommandLine;
using StandardWinUI.PresentationState;

namespace StandardWinUI;

/// <summary>
/// Configures WinUI services, logging, and CrystalData persistence.
/// </summary>
public class AppUnit : UnitBase, IUnitPreparable, IUnitExecutable
{
    /// <summary>
    /// Builds the application's service container and logging configuration.
    /// </summary>
    public class Builder : UnitBuilder<Product>
    {// Builder class for customizing dependencies.
        public Builder()
            : base()
        {
            // Configuration for Unit.
            this.PreConfigure(context =>
            {
                context.ProgramDirectory = EntryPoint.DataDirectory;
                context.DataDirectory = EntryPoint.DataDirectory;
            });

            this.Configure(context =>
            {
                // context.AddSingleton<AppUnit>();
                context.AddSingleton<StandardApp>();
                context.AddSingleton<IApp, App>();
                // context.Services.AddSingleton(x => (App)x.GetRequiredService<IApp>()); // If you want to use the App instance, please uncomment it.

                // Presentation-State
                context.AddSingleton<MainWindow>();
                context.AddSingleton<HelloPage>();
                context.AddSingleton<BaibainPage>();
                context.AddSingleton<StatePage>();
                context.AddSingleton<StatePageState>();
                context.AddSingleton<MessagePage>();
                context.AddSingleton<MessagePageState>();
                context.AddSingleton<AdvancedPage>();
                context.AddSingleton<AdvancedPageState>();
                context.AddSingleton<SettingsPage>();
                context.AddSingleton<SettingsPageState>();
                context.AddSingleton<InformationPage>();
                context.AddSingleton<InformationPageState>();

                // Command
                // context.AddCommand(typeof(TestCommand));
                // context.AddCommand(typeof(Test2Command));

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
                        FileConfiguration = new GlobalFileConfiguration(AppSettings.FileName),
                        SaveFormat = SaveFormat.Utf8,
                    });
                });
        }
    }

    /// <summary>
    /// Runs configured commands and coordinates the application lifecycle.
    /// </summary>
    public class Product : UnitProduct
    {// Unit class for customizing behaviors.
        /// <summary>
        /// Contains the command-line arguments for an application run.
        /// </summary>
        /// <param name="Arguments">The command-line arguments to parse.</param>
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
            await SimpleParser.ParseAndExecute(this.Context.CommandTypes, parameters.Arguments, parserOptions);

            await this.Context.SendStopAsync();
            await this.Context.SendTerminateAsync();
        }
    }

    private class ExampleLogFilter : ILogFilter
    {
        public ExampleLogFilter(AppUnit appUnit)
        {
            this.appUnit = appUnit;
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

        private AppUnit appUnit;
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
