// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

/*  BenchmarkDotNet, small template code
 *  PM> Install-Package BenchmarkDotNet
 */

using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography;
using Arc.Visceral;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1503 // Braces should not be omitted

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<ByteCopyTest>(); // SwapTest, MemoryAllocationTest, ByteCopyTest
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(ReconstructTest),
            });
            switcher.Run(args);
        }
    }

    public class BenchmarkConfig : BenchmarkDotNet.Configs.ManualConfig
    {
        public BenchmarkConfig()
        {
            this.Add(BenchmarkDotNet.Exporters.MarkdownExporter.GitHub);
            this.Add(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);

            // this.Add(Job.ShortRun.With(BenchmarkDotNet.Environments.Platform.X64).WithWarmupCount(1).WithIterationCount(1));
            // this.Add(BenchmarkDotNet.Jobs.Job.MediumRun.WithGcForce(true).WithId("GcForce medium"));
            // this.Add(BenchmarkDotNet.Jobs.Job.ShortRun);
            this.Add(BenchmarkDotNet.Jobs.Job.MediumRun);
        }
    }

    [Reconstructable]
    public class ChildClass
    {
        public int a;
        public int b;
    }

    public class ChildClass2 : IReconstructable
    {
        public int a2;
        public int b2;

        public void Reconstruct()
        {
            this.a2 = 2;
            this.b2 = 3;
        }
    }

    public class TestClass : IReconstructable
    {
        public int x;
        public string? y;
        public ChildClass? ca;
        public ChildClass cb = default!;
        public ChildClass2 cc = default!;

        public void Reconstruct()
        {
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class ReconstructTest
    {
        private TestClass tc = default!;

        public ReconstructTest()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            this.tc = new TestClass();
        }

        [Benchmark]
        public TestClass TestReconstruct()
        {
            Reconstruct.Do(this.tc);
            this.tc.cc.a2 = -1;
            Reconstruct.Do(this.tc);
            return this.tc;
        }

        [Benchmark]
        public TestClass TestExpressionTree()
        {
            Obsolete.Visceral.Reconstruct.Do(ref this.tc);
            this.tc.cc.a2 = -1;
            Obsolete.Visceral.Reconstruct.Do(ref this.tc);
            return this.tc;
        }

        [Benchmark]
        public TestClass TestRaw()
        {
            if (this.tc.y == null) this.tc.y = string.Empty;
            if (this.tc.ca == null) this.tc.ca = new ChildClass();
            if (this.tc.cb == null) this.tc.cb = new ChildClass();
            if (this.tc.cc == null) this.tc.cc = new ChildClass2();
            this.tc.cc.Reconstruct();
            this.tc.Reconstruct();

            this.tc.cc.a2 = -1;

            if (this.tc.y == null) this.tc.y = string.Empty;
            if (this.tc.ca == null) this.tc.ca = new ChildClass();
            if (this.tc.cb == null) this.tc.cb = new ChildClass();
            if (this.tc.cc == null) this.tc.cc = new ChildClass2();
            this.tc.cc.Reconstruct();
            this.tc.Reconstruct();

            return this.tc;
        }
    }
}
