﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinAPI;
using Tinyhand;

namespace StandardWinUI;

[TinyhandObject]
public partial class AppSettings : ITinyhandSerializationCallback
{// Application Settings
    public const string Filename = "AppSettings.tinyhand";

    [Key(0)]
    public bool LoadError { get; set; } // True if a load error occured.

    [Key(1)]
    public DipWindowPlacement WindowPlacement { get; set; } = default!;

    [Key(2)]
    public string Culture { get; set; } = App.DefaultCulture; // Default culture

    [Key(3)]
    public double DisplayScaling { get; set; } = 1.0d; // Display Scaling

    [Key(4)]
    public TestItem.GoshujinClass TestItems { get; set; } = default!;

    public void OnAfterDeserialize()
    {
        // Transformer.Instance.ScaleX = this.DisplayScaling;
        // Transformer.Instance.ScaleY = this.DisplayScaling;
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterReconstruct()
    {
    }
}