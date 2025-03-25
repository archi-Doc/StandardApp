// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;

namespace StandardWinUI;

/// <summary>
/// AppSettings manages the application's settings.
/// </summary>
[TinyhandObject(ImplicitKeyAsName = true)]
public partial class AppSettings
{
    public const string Filename = "AppSettings.tinyhand";

    public DipWindowPlacement WindowPlacement { get; set; } = default!;

    public string Culture { get; set; } = string.Empty;

    public double ViewScale { get; set; } = 1.0d;

    public int Baibai { get; set; }

    // public TestItem.GoshujinClass TestItems { get; set; } = default!;

    [TinyhandOnDeserialized]
    public void OnAfterDeserialize()
    {
        Scaler.ViewScale = this.ViewScale;
    }

    [TinyhandOnSerializing]
    public void OnBeforeSerialize()
    {
        this.ViewScale = Scaler.ViewScale;
    }
}
