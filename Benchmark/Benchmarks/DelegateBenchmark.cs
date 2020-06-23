// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using Arc.WeakDelegate;
using BenchmarkDotNet.Attributes;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1649 // File name should match first type name

namespace Benchmark
{
    public class DelegateTestClass
    {
        public int TestFunction(int x)
        {
            return x * 2;
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class DelegateBenchmark
    {
        private DelegateTestClass testClass = null!;
        private WeakFunc<int, int> weakFunc = null!;
        private Func<int, int> func = null!;

        public DelegateBenchmark()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            this.testClass = new DelegateTestClass();
            this.func = this.testClass.TestFunction;
            this.weakFunc = new WeakFunc<int, int>(this.testClass.TestFunction);
        }

        [Benchmark]
        public WeakFunc<int, int> Prepare_WeakFunc()
        {
            return new WeakFunc<int, int>(this.testClass.TestFunction);
        }

        /*[Benchmark]
        public int Execute_Direct()
        {
            return this.func(4);
        }

        [Benchmark]
        public int Execute_WeakFunc()
        {
            return this.weakFunc.Execute(4);
        }*/
    }
}
