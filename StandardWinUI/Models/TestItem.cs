// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using ValueLink;

namespace StandardWinUI;

[TinyhandObject]
[ValueLinkObject]
public partial class TestItem
{
    [KeyAsName]
    [Link(AutoNotify = true, AddValue = true, Accessibility = ValueLinkAccessibility.Public)]
    private DateTime dateTime;

    [KeyAsName]
    [Link(Type = ChainType.Ordered, AutoNotify = true, AddValue = true, Accessibility = ValueLinkAccessibility.Public)]
    private int id;

    [IgnoreMember]
    public int Selection { get; set; }

    [Link(Type = ChainType.Observable, Name = "Observable", Primary = true)]
    [Link(Type = ChainType.QueueList, Name = "Queue")]
    public TestItem(int id, DateTime dt)
    {
        this.id = id;
        this.dateTime = dt;
    }

    public TestItem()
    {
    }
}
