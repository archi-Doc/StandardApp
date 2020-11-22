// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace Benchmark
{
    public partial class ChildClass
    {
        public static ChildClass Reconstruct(ChildClass? t)
        {
            if (t == null) t = new Benchmark.ChildClass();

            return t;
        }
    }

    public partial class ChildClass2
    {
        public static ChildClass2 Reconstruct(ChildClass2? t)
        {
            if (t == null) t = new Benchmark.ChildClass2();
            t.OnAfterReconstruct();

            return t;
        }
    }

    public partial class TestClass
    {
        public static TestClass Reconstruct(TestClass? t)
        {
            if (t == null) t = new Benchmark.TestClass();
            if (t.y == null) t.y = string.Empty;
            if (t.ca == null) t.ca = new Benchmark.ChildClass();
            if (t.cb == null) t.cb = new Benchmark.ChildClass();
            if (t.cc == null) t.cc = new Benchmark.ChildClass2();
            t.cc.OnAfterReconstruct();
            t.OnAfterReconstruct();

            return t;
        }
    }
}
