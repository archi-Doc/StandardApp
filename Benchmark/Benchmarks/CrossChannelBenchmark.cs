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

        [Benchmark]
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
        }

        [Benchmark]
        public void SendKey()
        {
            CrossChannel.Send_Key<int, int>(3, 3);
            return;
        }

        [Benchmark]
        public void OpenSend_Key()
        {
            using (var d = CrossChannel.Open_Key<int, uint>(null, 3, x => { }))
            {
                CrossChannel.Send_Key<int, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_Key()
        {
            using (var c = CrossChannel.Open_Key<int, uint>(null, 3, x => { }))
            {
                CrossChannel.Send_Key<int, uint>(3, 1);
                CrossChannel.Send_Key<int, uint>(3, 2);
                CrossChannel.Send_Key<int, uint>(3, 3);
                CrossChannel.Send_Key<int, uint>(3, 4);
                CrossChannel.Send_Key<int, uint>(3, 5);
                CrossChannel.Send_Key<int, uint>(3, 6);
                CrossChannel.Send_Key<int, uint>(3, 7);
                CrossChannel.Send_Key<int, uint>(3, 8);
            }

            return;
        }

        [Benchmark]
        public void OpenSend_Result()
        {
            using (var c = CrossChannel.OpenAndRespond<int, int>(null, x => x))
            {
                CrossChannel.SendAndReceive<int, int>(1);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_Result()
        {
            using (var c = CrossChannel.OpenAndRespond<int, int>(null, x => x))
            {
                CrossChannel.SendAndReceive<int, int>(1);
                CrossChannel.SendAndReceive<int, int>(2);
                CrossChannel.SendAndReceive<int, int>(3);
                CrossChannel.SendAndReceive<int, int>(4);
                CrossChannel.SendAndReceive<int, int>(5);
                CrossChannel.SendAndReceive<int, int>(6);
                CrossChannel.SendAndReceive<int, int>(7);
                CrossChannel.SendAndReceive<int, int>(8);
            }

            return;
        }

        [Benchmark]
        public void OpenSend_KeyResult()
        {
            using (var d = CrossChannel.OpenAndRespond_Key<int, uint, uint>(null, 3, x => x))
            {
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenSend8_KeyResult()
        {
            using (var c = CrossChannel.OpenAndRespond_Key<int, uint, uint>(null, 3, x => x))
            {
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 1);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 2);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 3);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 4);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 5);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 6);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 7);
                CrossChannel.SendAndReceive_Key<int, uint, uint>(3, 8);
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
        public void Class_OpenSend_Result()
        {
            using (var c = this.CCC.Open<int, int>(null, x => x))
            {
                this.CCC.Send<int, int>(3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8_Result()
        {
            using (var c = this.CCC.Open<int, int>(null, x => x))
            {
                this.CCC.Send<int, int>(1);
                this.CCC.Send<int, int>(2);
                this.CCC.Send<int, int>(3);
                this.CCC.Send<int, int>(4);
                this.CCC.Send<int, int>(5);
                this.CCC.Send<int, int>(6);
                this.CCC.Send<int, int>(7);
                this.CCC.Send<int, int>(8);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend_KeyResult()
        {
            using (var c = this.CCC.OpenKey<int, int, int>(null, 1, x => x))
            {
                this.CCC.SendKey<int, int, int>(1, 3);
            }

            return;
        }

        [Benchmark]
        public void Class_OpenSend8_KeyResult()
        {
            using (var c = this.CCC.OpenKey<int, int, int>(null, 1, x => x))
            {
                this.CCC.SendKey<int, int, int>(1, 1);
                this.CCC.SendKey<int, int, int>(1, 2);
                this.CCC.SendKey<int, int, int>(1, 3);
                this.CCC.SendKey<int, int, int>(1, 4);
                this.CCC.SendKey<int, int, int>(1, 5);
                this.CCC.SendKey<int, int, int>(1, 6);
                this.CCC.SendKey<int, int, int>(1, 7);
                this.CCC.SendKey<int, int, int>(1, 8);
            }

            return;
        }
    }
}
