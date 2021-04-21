// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossLink;
using Tinyhand;

namespace StandardApp
{
    [TinyhandObject]
    [CrossLinkObject]
    public partial class TestItem
    {
        [KeyAsName]
        [Link(AutoNotify = true)]
        private DateTime dateTime;

        [KeyAsName]
        [Link(Type = ChainType.Ordered, AutoNotify = true)]
        private int id;

        [IgnoreMember]
        public int Selection { get; set; }

        [Link(Type = ChainType.List, Name = "Observable", Primary = true)]
        public TestItem(int id, DateTime dt)
        {
            this.id = id;
            this.DateTime = dt;
        }

        public TestItem()
        {
        }
    }
}
