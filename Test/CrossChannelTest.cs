// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using CrossChannel;
using Xunit;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1214 // Readonly fields should appear before non-readonly fields
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1503 // Braces should not be omitted
#pragma warning disable SA1649 // File name should match first type name

namespace Test.CrossChannelTest
{
    public class CrossChannelTest
    {
        [Fact]
        public void Test_CrossChannel()
        {
            Radio.Send<int>(2);

            this.Test_CrossChannel_Create();
            this.Test_CrossChannel_Create();
            var i = Radio.SendTwoWay<int, int>(3);
            Assert.Equal(2, i.Length);
            Assert.Equal(6, i[0]);
            Assert.Equal(6, i[1]);

            GC.Collect();

            i = Radio.SendTwoWay<int, int>(43);
            Assert.Empty(i);

            this.Test_CrossChannel_Create();

            i = Radio.SendTwoWay<int, int>(4);
            Assert.Single(i);
            Assert.Equal(8, i[0]);
        }

        [Fact]
        public void Test_CrossChannel_Create()
        {
            new TestClass_CrossChannel();
            return;
        }

        [Fact]
        public void Test_Dispose()
        {
            this.Test_Dispose1();
            GC.Collect();

            Radio.Open<int>(x => { }, new object());

            this.Test_Dispose2();
            GC.Collect();

            Radio.OpenKey<int, int>(0, x => { }, new object());
            Radio.OpenKey<int, int>(0, x => { }, new object());
        }

        private void Test_Dispose1()
        {
            for (var n = 0; n < 31; n++)
            {
                Radio.Open<int>(x => { }, new object());
            }
        }

        private void Test_Dispose2()
        {
            for (var n = 0; n < 31; n++)
            {
                Radio.OpenKey<int, int>(0, x => { }, new object());
            }
        }
    }

    public class TestClass_CrossChannel
    {
        public TestClass_CrossChannel()
        {
            Radio.OpenTwoWay<int, int>(this.Function, this);
        }

        public int Function(int x)
        {
            return x * 2;
        }
    }
}
