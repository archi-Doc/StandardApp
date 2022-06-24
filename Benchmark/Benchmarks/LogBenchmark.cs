// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using Arc.CrossChannel;
using BenchmarkDotNet.Attributes;
using DryIoc;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PubSub;
using Serilog;
using ZLogger;

#pragma warning disable SA1649 // File name should match first type name

namespace Benchmark;

public class EmptyClass
{
    public EmptyClass()
    {
    }
}

public class SerilogClass
{
    public SerilogClass(int id)
    {
        Log.Information($"test {id}");
    }
}

public class ZLoggerEmptyClass
{
    public ZLoggerEmptyClass(ILogger<ZLoggerEmptyClass> logger)
    {
    }
}

public class ZLoggerClass
{
    public ZLoggerClass(ILogger<ZLoggerClass> logger)
    {
        logger.ZLogInformation("test {0}", 123);
    }
}

public class ZLoggerProcessor : IAsyncLogProcessor
{
    readonly StringWriter stringWriter;
    readonly ZLoggerOptions options;

    public ZLoggerProcessor(StringWriter stream, ZLoggerOptions options)
    {
        this.stringWriter = stream;
        this.options = options;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async ValueTask DisposeAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        this.stringWriter.Dispose();
    }

    public void Post(IZLoggerEntry log)
    {
        var msg = log.FormatToString(options, null);
        this.stringWriter.Write(msg);
        log.Return();
    }
}

[Config(typeof(BenchmarkConfig))]
public class LogBenchmark
{
    public ServiceProvider Provider { get; private set; }

    public StringWriter StringWriter { get; } = new();

    public StringWriter StringWriter2 { get; } = new();

    public MemoryStream MemoryStream { get; } = new();

    public LogBenchmark()
    {
    }

    [GlobalSetup]
    public void Setup()
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.TextWriter(this.StringWriter)
        .CreateLogger();

        var sc = new ServiceCollection();
        // sc.AddLogging(x => x.AddZLoggerStream(this.MemoryStream).AddZLoggerConsole().SetMinimumLevel(LogLevel.Information));
        var logProcessor = new ZLoggerProcessor(this.StringWriter2, new ZLoggerOptions());
        sc.AddLogging(x => x.AddZLoggerLogProcessor(logProcessor).SetMinimumLevel(LogLevel.Information));
        sc.AddTransient<ZLoggerClass>();
        sc.AddTransient<ZLoggerEmptyClass>();
        this.Provider = sc.BuildServiceProvider();
    }

    [Benchmark]
    public EmptyClass LogClass()
    {
        return new EmptyClass();
    }

    [Benchmark]
    public SerilogClass SerilogClass()
    {
        var c = new SerilogClass(123);
        var sb = this.StringWriter.GetStringBuilder();
        sb.Length = 0;
        return c;
    }

    [Benchmark]
    public ZLoggerClass ZLoggerClass()
    {
        var c = this.Provider.GetRequiredService<ZLoggerClass>();
        var sb = this.StringWriter2.GetStringBuilder();
        sb.Length = 0;
        return c;
    }

    [Benchmark]
    public ZLoggerEmptyClass ZLoggerEmptyClass()
    {
        return this.Provider.GetRequiredService<ZLoggerEmptyClass>();
    }
}
