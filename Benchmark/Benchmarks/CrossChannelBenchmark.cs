// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.CrossChannel;
using BenchmarkDotNet.Attributes;
using DryIoc;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using PubSub;

#pragma warning disable SA1649 // File name should match first type name

namespace Benchmark
{
    public class SimpleReceiver
    {
        public SimpleReceiver()
        {
            CrossChannel.Open<int, int>(null, x => x * 5);
            CrossChannel.Open<int, int>(null, x => x * 5);
        }
    }

    public class SimpleReceiver2
    {
        public SimpleReceiver2()
        {
            CrossChannel.Open<uint, uint>(null, x => x * 3);
            CrossChannel.Open<uint, uint>(null, x => x * 3);
        }
    }

    public class H2HReceiver
    {
        public H2HReceiver()
        {
            CrossChannel.Open<int>(null, x => { });
        }
    }

    public class PubSubReceiver
    {
        public PubSubReceiver()
        {
            Hub.Default.Subscribe<int>(x => { });
        }
    }

    public class SingletonClass
    {
        public int Id { get; set; }
    }

    public class TransientClass
    {
        public int Id { get; set; }
    }

    [Config(typeof(BenchmarkConfig))]
    public class CrossChannelBenchmark
    {
        public ServiceProvider Provider { get; }

        public Container Container { get; } = new();

        public CrossChannelClass CCC { get; } = new();

        public CrossChannelBenchmark()
        {
            var sc = new ServiceCollection();
            sc.AddMessagePipe();
            this.Provider = sc.BuildServiceProvider();
            this.Container.Register<SingletonClass>(Reuse.Singleton);
            this.Container.Register<TransientClass>(Reuse.Transient);
        }

        [GlobalSetup]
        public void Setup()
        {
            var simpleReceiver = new SimpleReceiver();
            var simpleReceiver2 = new SimpleReceiver2();
            var h2hReceiver = new H2HReceiver();
            var pubSubReceiver = new PubSubReceiver();
        }

        [Benchmark]
        public void Send()
        {
            CrossChannel.Send<int>(3);
            return;
        }

        [Benchmark]
        public void OpenAndSend()
        {
            using (var c = CrossChannel.Open<uint>(null, x => { }))
            {
                CrossChannel.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void OpenAndSend8()
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

        [Benchmark]
        public void SendKey()
        {
            CrossChannel.SendKey<int, int>(3, 3);
            return;
        }

        [Benchmark]
        public void OpenAndSendKey()
        {
            using (var c = CrossChannel.OpenKey<int, uint>(3, null, x => { }))
            {
                CrossChannel.SendKey<int, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenAndSendKey8()
        {
            using (var c = CrossChannel.OpenKey<int, uint>(3, null, x => { }))
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

        [Benchmark]
        public void Send_CC2()
        {
            this.CCC.Send<int>(3);
            return;
        }

        [Benchmark]
        public void OpenAndSend_CC2()
        {
            using (var c = this.CCC.Open<uint>(null, x => { }))
            {
                this.CCC.Send<uint>(3);
            }

            return;
        }

        [Benchmark]
        public void OpenAndSend8_CC2()
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
        public void SendKey_CC2()
        {
            this.CCC.SendKey<int, int>(3, 3);
            return;
        }

        [Benchmark]
        public void OpenAndSendKey_CC2()
        {
            using (var c = this.CCC.OpenKey<int, uint>(3, null, x => { }))
            {
                this.CCC.SendKey<int, uint>(3, 3);
            }

            return;
        }

        [Benchmark]
        public void OpenAndSendKey8_CC2()
        {
            using (var c = this.CCC.OpenKey<int, uint>(3, null, x => { }))
            {
                this.CCC.SendKey<int, uint>(3, 1);
                this.CCC.SendKey<int, uint>(3, 2);
                this.CCC.SendKey<int, uint>(3, 3);
                this.CCC.SendKey<int, uint>(3, 4);
                this.CCC.SendKey<int, uint>(3, 5);
                this.CCC.SendKey<int, uint>(3, 6);
                this.CCC.SendKey<int, uint>(3, 7);
                this.CCC.SendKey<int, uint>(3, 8);
            }

            return;
        }

        /*[Benchmark]
        public void Send_Pub()
        {
            Hub.Default.Publish(3);
            return;
        }

        [Benchmark]
        public void OpenAndSend_Pub()
        {
            Hub.Default.Subscribe<uint>(x => { });
            Hub.Default.Publish<uint>(3);
            Hub.Default.Unsubscribe<uint>();

            return;
        }

        [Benchmark]
        public void OpenAndSend8_Pub()
        {
            Hub.Default.Subscribe<uint>(x => { });
            Hub.Default.Publish<uint>(1);
            Hub.Default.Publish<uint>(2);
            Hub.Default.Publish<uint>(3);
            Hub.Default.Publish<uint>(4);
            Hub.Default.Publish<uint>(5);
            Hub.Default.Publish<uint>(6);
            Hub.Default.Publish<uint>(7);
            Hub.Default.Publish<uint>(8);
            Hub.Default.Unsubscribe<uint>();

            return;
        }

        [Benchmark]
        public void OpenAndSend_MP()
        {
            var sub = this.Provider.GetService<ISubscriber<uint>>()!;
            var pub = this.Provider.GetService<IPublisher<uint>>()!;
            using (var i = sub.Subscribe(x => { }))
            {
                pub.Publish(3);
            }

            return;
        }

        [Benchmark]
        public void OpenAndSend8_MP()
        {
            var sub = this.Provider.GetService<ISubscriber<uint>>()!;
            var pub = this.Provider.GetService<IPublisher<uint>>()!;
            using (var i = sub.Subscribe(x => { }))
            {
                pub.Publish(1);
                pub.Publish(2);
                pub.Publish(3);
                pub.Publish(4);
                pub.Publish(5);
                pub.Publish(6);
                pub.Publish(7);
                pub.Publish(8);
            }

            return;
        }*/
    }
}
