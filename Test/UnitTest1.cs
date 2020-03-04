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

    public struct ChildStruct : IReconstructable
    {
        public int a;
        public int b;
        public ChildClass2 cc2;

        public void Reconstruct()
        {
            this.a = 10;
            this.b = 20;
        }
    }

    public class TestClass : IReconstructable
    {
        public int x;
        public string? y;
        public ChildClass? ca;
        public ChildClass cb = default!;
        public ChildClass2 cc = default!;
        public ChildStruct cs;

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

            tc.cs.a = 5;
            tc.cs.b = 6;
            var st = tc.cs;
            st.Reconstruct();
            tc.cs.Reconstruct();
            tc.cs.a = 5;
            tc.cs.b = 6;
            Reconstruct.Do(tc);


            Assert.Equal(0, tc.x);
            Assert.Equal("", tc.y);
            Assert.NotNull(tc.ca);
            Assert.NotNull(tc.cb);
            Assert.NotNull(tc.cc);
            Assert.Equal(2, tc.cc.a2);
            Assert.Equal(3, tc.cc.b2);
        }
    }
}
