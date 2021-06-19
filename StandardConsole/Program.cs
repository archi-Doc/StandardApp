// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using ImTools;
using Serilog;
using SimpleCommandLine;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace StandardConsole
{
    public class TestOptions
    {
        [SimpleOption("number", "n")]
        public int Number { get; set; } = 3000;
    }

    [SimpleCommand("test")]
    public class TestCommand : ISimpleCommandAsync<TestOptions>
    {
        public TestCommand(IAppService appService)
        {
            this.AppService = appService;
        }

        public async Task Run(TestOptions option, string[] args)
        {
            this.AppService.EnterCommand(string.Empty);

            Console.WriteLine("Test command:");
            Console.WriteLine($"Number is {option.Number}");
            try
            {
                await Task.Delay(option.Number, this.AppService.CancellationToken);
            }
            catch
            {
            }

            this.AppService.ExitCommand();
        }

        public IAppService AppService { get; }
    }

    [SimpleCommand("test2")]
    public class TestCommand2 : ISimpleCommand
    {
        public void Run(string[] args)
        {
            Console.WriteLine("Test command2:");
        }
    }

    public class Program
    {
        public static Container Container { get; } = new();

        public static async Task Main(string[] args)
        {
            // Simple Commands
            var commandTypes = new Type[]
            {
                typeof(TestCommand),
                typeof(TestCommand2),
            };

            // DI Container
            Container.Register<IAppService, AppService>(Reuse.Singleton);
            foreach (var x in commandTypes)
            {
                Container.Register(x, Reuse.Singleton);
            }

            Container.ValidateAndThrow();

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Log.Information("exit (ProcessExit)");
                Container.Resolve<IAppService>().Terminate();
            };

            Console.CancelKeyPress += (s, e) =>
            {
                Log.Information("exit (Ctrl+C)");
                Container.Resolve<IAppService>().Terminate();
            };

            var parserOptions = SimpleParserOptions.Standard with
            {
                ServiceProvider = Container,
                RequireStrictCommandName = false,
                RequireStrictOptionName = true
            };

            await SimpleParser.ParseAndRunAsync(commandTypes, args, parserOptions);
        }
    }
}
