// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.Visceral;
using BenchmarkDotNet.Attributes;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1503 // Braces should not be omitted

namespace Benchmark
{
    [Reconstructable]
    [Arc.Visceral.Obsolete.Reconstructable]
    public partial class ChildClass
    {
        public int a;
        public int b;
    }

    public partial class ChildClass2 : IReconstructable, Arc.Visceral.Obsolete.IReconstructable
    {
        public int a2;
        public int b2;

        public void OnAfterReconstruct()
        {
            this.a2 = 2;
            this.b2 = 3;
        }

        public void Reconstruct()
        {
            this.a2 = 2;
            this.b2 = 3;
        }
    }

    public partial class TestClass : IReconstructable, Arc.Visceral.Obsolete.IReconstructable
    {
        public int x;
        public string? y;
        public ChildClass? ca;
        public ChildClass cb = default!;
        public ChildClass2 cc = default!;

        public void OnAfterReconstruct()
        {
        }

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
            ReconstructLoader.Load();
        }

        [GlobalSetup]
        public void Setup()
        {
            this.tc = new TestClass();
        }

        [Benchmark]
        public Arc.Visceral.Obsolete.ReconstructFunc<TestClass>? BuildReconstruct()
        {
            var builder = Arc.Visceral.Obsolete.Reconstruct.BuildCode<TestClass>();
            return builder;
        }

        [Benchmark]
        public TestClass TestReconstruct_Obsolete()
        {
            Arc.Visceral.Obsolete.Reconstruct.Do(this.tc);
            this.tc.cc.a2 = -1;
            Arc.Visceral.Obsolete.Reconstruct.Do(this.tc);
            return this.tc;
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
        public TestClass TestRaw()
        {
            if (this.tc.y == null) this.tc.y = string.Empty;
            if (this.tc.ca == null) this.tc.ca = new ChildClass();
            if (this.tc.cb == null) this.tc.cb = new ChildClass();
            if (this.tc.cc == null) this.tc.cc = new ChildClass2();
            this.tc.cc.OnAfterReconstruct();
            this.tc.OnAfterReconstruct();

            this.tc.cc.a2 = -1;

            if (this.tc.y == null) this.tc.y = string.Empty;
            if (this.tc.ca == null) this.tc.ca = new ChildClass();
            if (this.tc.cb == null) this.tc.cb = new ChildClass();
            if (this.tc.cc == null) this.tc.cc = new ChildClass2();
            this.tc.cc.OnAfterReconstruct();
            this.tc.OnAfterReconstruct();

            return this.tc;
        }
    }
}
