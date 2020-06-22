// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using Arc.CrossChannel;
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
            CrossChannel.Send<int>(2);

            this.Test_CrossChannel_Create();
            this.Test_CrossChannel_Create();
            var i = CrossChannel.Send<int, int>(3);

            GC.Collect();

            i = CrossChannel.SendTarget<int, int>(43, "ee");

            this.Test_CrossChannel_Create();
        }

        [Fact]
        public void Test_CrossChannel_Create()
        {
            new TestClass_CrossChannel();
            return;
        }
    }

    public class TestClass_CrossChannel
    {
        public TestClass_CrossChannel()
        {
            CrossChannel.Open<int, int>(this.Function);
        }

        public int Function(int x)
        {
            return x * 2;
        }
    }
}
