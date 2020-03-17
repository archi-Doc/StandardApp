// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arc.Mvvm;
using Arc.Visceral;
using Xunit;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1214 // Readonly fields should appear before non-readonly fields
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1503 // Braces should not be omitted
#pragma warning disable SA1649 // File name should match first type name

namespace Test
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

    public class BrushOption : BindableBase
    { // Constructor -> (OnAfterDeserialize()) -> Prepare() -> ... -> OnBeforeSerialize()
        private System.Windows.Media.SolidColorBrush? brush;

        public BrushOption()
        {
        }

        public System.Windows.Media.SolidColorBrush? Brush
        {
            get { return this.brush; }
            private set { this.SetProperty(ref this.brush, value); }
        }
    }

    public class ReconstructTest
    {
        [Fact]
        public void Test2()
        {
            var tc = new TestClass2();
            Reconstruct.Do(ref tc);

            var brush = new BrushOption();
            Reconstruct.Do(ref brush);
        }

        [Fact]
        public void Test1()
        {
            var tc = new TestClass();
            Reconstruct.Do(ref tc);

            tc.cs.a = 5;
            tc.cs.b = 6;
            Reconstruct.Do(ref tc.cs);

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

            public bool Get<T>(out ReconstructAction<T>? action)
            {
                action = default;

                var type = typeof(T);
                if (type == typeof(ChildClass))
                {
                    ReconstructAction<ChildClass> ac = (ref ChildClass cc) =>
                    {
                        cc.a = 100;
                        cc.b = 200;
                    };
                    action = (ReconstructAction<T>)(object)ac;
                    return true; // supported.
                }

                return false; // not supported.
            }
        }

        [Fact]
        public void TestInitialResolvers()
        {
            Reconstruct.InitialResolvers = new IReconstructResolver[] { CustomReconstructResolver.Instance, DefaultReconstructResolver.Instance };

            var tc = new TestClass();
            Reconstruct.Do(ref tc);

            Assert.Equal(100, tc.ca!.a);
            Assert.Equal(200, tc.ca!.b);
            Assert.Equal(100, tc.cb.a);
            Assert.Equal(200, tc.cb.b);
        }
    }
}
