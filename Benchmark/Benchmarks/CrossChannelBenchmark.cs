// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.CrossChannel;
using Arc.WeakDelegate;
using BenchmarkDotNet.Attributes;
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
        public CrossChannelClass CCC { get; } = new();

        public CrossChannelBenchmark()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
        }

        /*[Benchmark]
        public void Send()
        {
            CrossChannel.Send<int>(3);
            return;
        }

        [Benchmark]
        public void OpenSend()
        {
            using (var c = CrossChannel.Open<uint>(null, x => { }))
            {
                CrossChannel.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8()
        {
            using (var c = CrossChannel.Open<uint>(null, x => { }))
            {
                CrossChannel.Send<uint>(1);
                CrossChannel.Send<uint>(2);
                CrossChannel.Send<uint>(3);
                CrossChannel.Send<uint>(4);
                CrossChannel.Send<uint>(5);
                CrossChannel.Send<uint>(6);
                CrossChannel.Send<uint>(7);
                CrossChannel.Send<uint>(8);
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
            using (var c = CrossChannel.Open<uint>(new object(), WeakActionTest))
            {
                CrossChannel.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_Weak()
        {
            using (var c = CrossChannel.Open<uint>(new object(), WeakActionTest))
            {
                CrossChannel.Send<uint>(1);
                CrossChannel.Send<uint>(2);
                CrossChannel.Send<uint>(3);
                CrossChannel.Send<uint>(4);
                CrossChannel.Send<uint>(5);
                CrossChannel.Send<uint>(6);
                CrossChannel.Send<uint>(7);
                CrossChannel.Send<uint>(8);
            }

            return;
        }*/

        [Benchmark]
        public void SendKey()
        {
            CrossChannel.SendKey<int, int>(3, 3);
            return;
        }

        [Benchmark]
        public void OpenSend_Key()
        {
            using (var d = CrossChannel.OpenKey<int, uint>(null, 3, x => { }))
            {
                CrossChannel.SendKey<int, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_Key()
        {
            using (var c = CrossChannel.OpenKey<int, uint>(null, 3, x => { }))
            {
                CrossChannel.SendKey<int, uint>(3, 1);
                CrossChannel.SendKey<int, uint>(3, 2);
                CrossChannel.SendKey<int, uint>(3, 3);
                CrossChannel.SendKey<int, uint>(3, 4);
                CrossChannel.SendKey<int, uint>(3, 5);
                CrossChannel.SendKey<int, uint>(3, 6);
                CrossChannel.SendKey<int, uint>(3, 7);
                CrossChannel.SendKey<int, uint>(3, 8);
            }

            return;
        }

        /*[Benchmark]
        public void OpenSend_TwoWay()
        {
            using (var c = CrossChannel.OpenTwoWay<int, int>(null, x => x))
            {
                CrossChannel.SendTwoWay<int, int>(1);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_TwoWay()
        {
            using (var c = CrossChannel.OpenTwoWay<int, int>(null, x => x))
            {
                CrossChannel.SendTwoWay<int, int>(1);
                CrossChannel.SendTwoWay<int, int>(2);
                CrossChannel.SendTwoWay<int, int>(3);
                CrossChannel.SendTwoWay<int, int>(4);
                CrossChannel.SendTwoWay<int, int>(5);
                CrossChannel.SendTwoWay<int, int>(6);
                CrossChannel.SendTwoWay<int, int>(7);
                CrossChannel.SendTwoWay<int, int>(8);
            }

            return;
        }

        [Benchmark]
        public void OpenSend_TwoWayKey()
        {
            using (var d = CrossChannel.OpenTwoWayKey<int, uint, uint>(null, 3, x => x))
            {
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_TwoWayKey()
        {
            using (var c = CrossChannel.OpenTwoWayKey<int, uint, uint>(null, 3, x => x))
            {
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 1);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 2);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 3);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 4);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 5);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 6);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 7);
                CrossChannel.SendTwoWayKey<int, uint, uint>(3, 8);
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
        }*/

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
        public void Class_OpenSend_Key2()
        {
            using (var c = this.CCC.OpenKey2<int, uint>(null, 1, x => { }))
            {
                this.CCC.SendKey2<int, uint>(1, 3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8_Key2()
        {
            using (var c = this.CCC.OpenKey2<int, uint>(null, 1, x => { }))
            {
                this.CCC.SendKey2<int, uint>(1, 1);
                this.CCC.SendKey2<int, uint>(1, 2);
                this.CCC.SendKey2<int, uint>(1, 3);
                this.CCC.SendKey2<int, uint>(1, 4);
                this.CCC.SendKey2<int, uint>(1, 5);
                this.CCC.SendKey2<int, uint>(1, 6);
                this.CCC.SendKey2<int, uint>(1, 7);
                this.CCC.SendKey2<int, uint>(1, 8);
            }

            return;
        }

        /*[Benchmark]
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
        }*/
    }
}
