// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.Visceral.Obsolete;
using Xunit;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1214 // Readonly fields should appear before non-readonly fields
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1503 // Braces should not be omitted
#pragma warning disable SA1649 // File name should match first type name

namespace Test.Reconstruct
{
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

        [Reconstructable]
        public string? y;
        public ChildClass? ca;
        public ChildClass cb = default!;
        public ChildClass2 cc = default!;
        public ChildStruct cs;

        public void Reconstruct()
        {
        }
    }

    public class TestChild2
    {
        public string? Name { get; set; }

        public TestChild2()
        {
        }
    }

    public class TestClass2 : IReconstructable
    {
        public TestChild2? Tc1;

        public readonly TestChild2? Tc2;

        public TestChild2? Tc3 { get; private set; }

        public TestChild2? Tc4 { get; }

        public TestChild2? tc;

        public TestChild2 Tc5
        {
            get
            {
                return this.tc ??= new TestChild2();
            }

            set
            {
                this.tc = value;
            }
        }

        public TestClass2()
        {
            this.Tc5.Name = "Tc5";
        }

        public void Reconstruct()
        {
        }
    }

    public class EmptyClass
    {
    }

    [Reconstructable]
    public class EmptyClass2
    {
    }

    public class ReconstructTest
    {
        [Fact]
        public void Test2()
        {
            var ec = new EmptyClass();
            Arc.Visceral.Obsolete.Reconstruct.Do(ec);

            var ec2 = new EmptyClass2();
            Arc.Visceral.Obsolete.Reconstruct.Do(ec2);

            var tc = new TestClass2();
            Arc.Visceral.Obsolete.Reconstruct.Do(tc);
        }

        [Fact]
        public void Test1()
        {
            var tc = new TestClass();
            var tc2 = Arc.Visceral.Obsolete.Reconstruct.Do(tc);

            tc.cs.a = 5;
            tc.cs.b = 6;
            tc.cs = Arc.Visceral.Obsolete.Reconstruct.Do(tc.cs);

            Assert.Equal(0, tc.x);
            Assert.Equal(string.Empty, tc.y);

            Assert.NotNull(tc.ca);
            Assert.NotNull(tc.cb);
            Assert.NotNull(tc.cc);
            Assert.Equal(2, tc.cc.a2);
            Assert.Equal(3, tc.cc.b2);

            Assert.Equal(10, tc.cs.a);
            Assert.Equal(20, tc.cs.b);
            Assert.NotNull(tc.cs.cc2);
        }

        public class CustomReconstructResolver : IReconstructResolver
        {
            /// <summary>
            /// Default instance.
            /// </summary>
            public static readonly CustomReconstructResolver Instance = new CustomReconstructResolver();

            public bool Get<T>(out ReconstructFunc<T>? action)
            {
                action = default;

                var type = typeof(T);
                if (type == typeof(ChildClass))
                {
                    ReconstructFunc<ChildClass> ac = (ChildClass cc) =>
                    {
                        cc.a = 100;
                        cc.b = 200;
                        return cc;
                    };
                    action = (ReconstructFunc<T>)(object)ac;
                    return true; // supported.
                }

                return false; // not supported.
            }
        }

        [Fact]
        public void TestInitialResolvers()
        {
            Arc.Visceral.Obsolete.Reconstruct.Resolvers = new IReconstructResolver[] { CustomReconstructResolver.Instance, DefaultReconstructResolver.Instance };
            Arc.Visceral.Obsolete.Reconstruct.RebuildCache<TestClass>(); // Rebuild static cache.

            var tc = new TestClass();
            Arc.Visceral.Obsolete.Reconstruct.Do(tc);

            Assert.Equal(100, tc.ca!.a);
            Assert.Equal(200, tc.ca!.b);
            Assert.Equal(100, tc.cb.a);
            Assert.Equal(200, tc.cb.b);
        }

        [Reconstructable]
        public class CircularClass1
        {
            public CircularClass2? Class2 { get; set; }
        }

        [Reconstructable]
        public class CircularClass2
        {
            public CircularClass1? Class1 { get; set; }

            public CircularClass3? Class3 { get; set; }
        }

        [Reconstructable]
        public class CircularClass3
        {
            public CircularClass1? Class1 { get; set; }

            public CircularClass3? Class3 { get; set; }
        }

        [Fact]
        public void TestCircular()
        {
            var cc1 = new CircularClass1();
            Arc.Visceral.Obsolete.Reconstruct.Do(cc1);

            Assert.NotNull(cc1.Class2);
            Assert.Null(cc1.Class2!.Class1);
            Assert.NotNull(cc1.Class2!.Class3);
            Assert.Null(cc1.Class2!.Class3!.Class1);
            Assert.Null(cc1.Class2!.Class3!.Class3);

            var cc2 = new CircularClass2();
            Arc.Visceral.Obsolete.Reconstruct.Do(cc2);

            Assert.Null(cc2.Class1);
            Assert.NotNull(cc2.Class3);
            Assert.Null(cc2.Class3!.Class1);
            Assert.Null(cc2.Class3!.Class3);
        }
    }
}
