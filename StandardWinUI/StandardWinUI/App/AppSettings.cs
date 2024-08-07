﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;

namespace StandardWinUI;

[TinyhandObject(ImplicitKeyAsName = true)]
public partial class AppSettings : ITinyhandSerializationCallback
{// Application Settings
    public const string Filename = "AppSettings.tinyhand";

    public DipWindowPlacement WindowPlacement { get; set; } = default!;

    public string Culture { get; set; } = string.Empty;

    public double DisplayScaling { get; set; } = 1.0d;

    // public TestItem.GoshujinClass TestItems { get; set; } = default!;

    public void OnAfterDeserialize()
    {
        Transformer.DisplayScaling = this.DisplayScaling;
    }

    public void OnBeforeSerialize()
    {
        this.DisplayScaling = Transformer.DisplayScaling;
    }

    public void OnAfterReconstruct()
    {
    }
}
