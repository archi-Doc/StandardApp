// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.CrossChannel;
using BenchmarkDotNet.Attributes;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1649 // File name should match first type name

namespace Benchmark
{
    public class SimpleReceiver
    {
        public SimpleReceiver()
        {
            CrossChannel.Open<int, int>(x => x * 5);
        }
    }

    public class SimpleReceiver2
    {
        public SimpleReceiver2()
        {
            CrossChannel.Open<uint, uint>(x => x * 3);
            CrossChannel.Open<uint, uint>(x => x * 3);
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class CrossChannelBenchmark
    {
        public CrossChannelBenchmark()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            var simpleReceiver = new SimpleReceiver();
            var simpleReceiver2 = new SimpleReceiver2();
        }

        [Benchmark]
        public int[] Send()
        {
            return CrossChannel.Send<int, int>(3);
        }

        [Benchmark]
        public uint[] Send2()
        {
            return CrossChannel.SendTarget<uint, uint>(3, null);
        }
    }
}
