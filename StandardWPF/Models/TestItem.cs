// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinyhand;
using ValueLink;

namespace StandardWPF;

[TinyhandObject]
[ValueLinkObject]
public partial class TestItem
{
    [MemberNameAsKey]
    [Link(AutoNotify = true, GenerateValue = true, Accessibility = ValueLinkAccessibility.Public)]
    private DateTime dateTime;

    [MemberNameAsKey]
    [Link(Type = ChainType.Ordered, AutoNotify = true, GenerateValue = true, Accessibility = ValueLinkAccessibility.Public)]
    private int id;

    [IgnoreMember]
    public int SelectionState { get; set; } // 0: Not selected, 1: Selected, 2: Selected and focused

    [Link(Type = ChainType.Observable, Name = "Observable", Primary = true)]
    [Link(Type = ChainType.QueueList, Name = "Queue")]
    public TestItem(int id, DateTime dateTime)
    {
        this.id = id;
        this.dateTime = dateTime;
    }

    public TestItem()
    {
    }
}
