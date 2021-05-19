// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WeakDelegate;
using BenchmarkDotNet.Attributes;
using CrossChannel;
using DryIoc;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using PubSub;

#pragma warning disable SA1649 // File name should match first type name

namespace Benchmark
{
    [Config(typeof(BenchmarkConfig))]
    public class CrossChannelBenchmark
    {
        public RadioClass CCC { get; } = new();

        public CrossChannelBenchmark()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public void Send()
        {
            Radio.Send<int>(3);
            return;
        }

        [Benchmark]
        public void OpenSend()
        {
            using (var c = Radio.Open<uint>(null, x => { }))
            {
                Radio.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8()
        {
            using (var c = Radio.Open<uint>(null, x => { }))
            {
                Radio.Send<uint>(1);
                Radio.Send<uint>(2);
                Radio.Send<uint>(3);
                Radio.Send<uint>(4);
                Radio.Send<uint>(5);
                Radio.Send<uint>(6);
                Radio.Send<uint>(7);
                Radio.Send<uint>(8);
            }

            return;
        }

        public void WeakActionTest(uint x)
        {
        }

        // [Benchmark]
        // public WeakAction<int> CreateWeakAction() => new WeakAction<int>(this, x => { });

        [Benchmark]
        public void OpenSend_Weak()
        {
            using (var c = Radio.Open<uint>(new object(), WeakActionTest))
            {
                Radio.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_Weak()
        {
            using (var c = Radio.Open<uint>(new object(), WeakActionTest))
            {
                Radio.Send<uint>(1);
                Radio.Send<uint>(2);
                Radio.Send<uint>(3);
                Radio.Send<uint>(4);
                Radio.Send<uint>(5);
                Radio.Send<uint>(6);
                Radio.Send<uint>(7);
                Radio.Send<uint>(8);
            }

            return;
        }

        [Benchmark]
        public void SendKey()
        {
            Radio.SendKey<int, int>(3, 3);
            return;
        }

        [Benchmark]
        public void OpenSend_Key()
        {
            using (var d = Radio.OpenKey<int, uint>(null, 3, x => { }))
            {
                Radio.SendKey<int, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_Key()
        {
            using (var c = Radio.OpenKey<int, uint>(null, 3, x => { }))
            {
                Radio.SendKey<int, uint>(3, 1);
                Radio.SendKey<int, uint>(3, 2);
                Radio.SendKey<int, uint>(3, 3);
                Radio.SendKey<int, uint>(3, 4);
                Radio.SendKey<int, uint>(3, 5);
                Radio.SendKey<int, uint>(3, 6);
                Radio.SendKey<int, uint>(3, 7);
                Radio.SendKey<int, uint>(3, 8);
            }

            return;
        }

        [Benchmark]
        public void OpenSend_TwoWay()
        {
            using (var c = Radio.OpenTwoWay<int, int>(null, x => x))
            {
                Radio.SendTwoWay<int, int>(1);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_TwoWay()
        {
            using (var c = Radio.OpenTwoWay<int, int>(null, x => x))
            {
                Radio.SendTwoWay<int, int>(1);
                Radio.SendTwoWay<int, int>(2);
                Radio.SendTwoWay<int, int>(3);
                Radio.SendTwoWay<int, int>(4);
                Radio.SendTwoWay<int, int>(5);
                Radio.SendTwoWay<int, int>(6);
                Radio.SendTwoWay<int, int>(7);
                Radio.SendTwoWay<int, int>(8);
            }

            return;
        }

        [Benchmark]
        public void OpenSend_TwoWayKey()
        {
            using (var d = Radio.OpenTwoWayKey<int, uint, uint>(null, 3, x => x))
            {
                Radio.SendTwoWayKey<int, uint, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_TwoWayKey()
        {
            using (var c = Radio.OpenTwoWayKey<int, uint, uint>(null, 3, x => x))
            {
                Radio.SendTwoWayKey<int, uint, uint>(3, 1);
                Radio.SendTwoWayKey<int, uint, uint>(3, 2);
                Radio.SendTwoWayKey<int, uint, uint>(3, 3);
                Radio.SendTwoWayKey<int, uint, uint>(3, 4);
                Radio.SendTwoWayKey<int, uint, uint>(3, 5);
                Radio.SendTwoWayKey<int, uint, uint>(3, 6);
                Radio.SendTwoWayKey<int, uint, uint>(3, 7);
                Radio.SendTwoWayKey<int, uint, uint>(3, 8);
            }

            return;
        }

        [Benchmark]
        public void Class_Send()
        {
            this.CCC.Send<int>(3);
            return;
        }

        [Benchmark]
        public void Class_OpenSend()
        {
            using (var c = this.CCC.Open<uint>(null, x => { }))
            {
                this.CCC.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8()
        {
            using (var c = this.CCC.Open<uint>(null, x => { }))
            {
                this.CCC.Send<uint>(1);
                this.CCC.Send<uint>(2);
                this.CCC.Send<uint>(3);
                this.CCC.Send<uint>(4);
                this.CCC.Send<uint>(5);
                this.CCC.Send<uint>(6);
                this.CCC.Send<uint>(7);
                this.CCC.Send<uint>(8);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend_Key()
        {
            using (var c = this.CCC.OpenKey<int, uint>(null, 1, x => { }))
            {
                this.CCC.SendKey<int, uint>(1, 3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8_Key()
        {
            using (var c = this.CCC.OpenKey<int, uint>(null, 1, x => { }))
            {
                this.CCC.SendKey<int, uint>(1, 1);
                this.CCC.SendKey<int, uint>(1, 2);
                this.CCC.SendKey<int, uint>(1, 3);
                this.CCC.SendKey<int, uint>(1, 4);
                this.CCC.SendKey<int, uint>(1, 5);
                this.CCC.SendKey<int, uint>(1, 6);
                this.CCC.SendKey<int, uint>(1, 7);
                this.CCC.SendKey<int, uint>(1, 8);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend_TwoWay()
        {
            using (var c = this.CCC.OpenTwoWay<int, int>(null, x => x))
            {
                this.CCC.SendTwoWay<int, int>(3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8_TwoWay()
        {
            using (var c = this.CCC.OpenTwoWay<int, int>(null, x => x))
            {
                this.CCC.SendTwoWay<int, int>(1);
                this.CCC.SendTwoWay<int, int>(2);
                this.CCC.SendTwoWay<int, int>(3);
                this.CCC.SendTwoWay<int, int>(4);
                this.CCC.SendTwoWay<int, int>(5);
                this.CCC.SendTwoWay<int, int>(6);
                this.CCC.SendTwoWay<int, int>(7);
                this.CCC.SendTwoWay<int, int>(8);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend_TwoWayKey()
        {
            using (var c = this.CCC.OpenTwoWayKey<int, int, int>(null, 1, x => x))
            {
                this.CCC.SendTwoWayKey<int, int, int>(1, 3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8_TwoWayKey()
        {
            using (var c = this.CCC.OpenTwoWayKey<int, int, int>(null, 1, x => x))
            {
                this.CCC.SendTwoWayKey<int, int, int>(1, 1);
                this.CCC.SendTwoWayKey<int, int, int>(1, 2);
                this.CCC.SendTwoWayKey<int, int, int>(1, 3);
                this.CCC.SendTwoWayKey<int, int, int>(1, 4);
                this.CCC.SendTwoWayKey<int, int, int>(1, 5);
                this.CCC.SendTwoWayKey<int, int, int>(1, 6);
                this.CCC.SendTwoWayKey<int, int, int>(1, 7);
                this.CCC.SendTwoWayKey<int, int, int>(1, 8);
            }

            return;
        }
    }
}
