using System;
using Xunit;
using Arc.Visceral;

namespace Test
{
    public class ChildClass
    {
        int a;
        int b;
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

    public class ReconstructTest
    {
        [Fact]
        public void Test1()
        {
            var tc = new TestClass();
            Reconstruct.Do(tc);
        }
    }
}
