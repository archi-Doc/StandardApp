// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI;

namespace StandardWinUI;

/// <summary>
/// AppSettings manages the application's settings.
/// </summary>
[TinyhandObject(ImplicitMemberNameAsKey = true)]
public partial class AppSettings
{
    public const string FileName = "AppSettings.tinyhand";

    #region FieldAndProperty

    public DipWindowPlacement WindowPlacement { get; set; } = new();

    public string Culture { get; set; } = string.Empty;

    public double ViewScale { get; set; } = 1.0d;

    [Key("Baibai")] // Keeps the previous key to preserve compatibility with existing settings files.
    public int BaibainNumber { get; set; }

    // public TestItem.GoshujinClass TestItems { get; set; } = new();

    [Key("BrushTest")] // Keeps the previous key to preserve compatibility with existing settings files.
    public BrushOption TestBrush { get; set; } = new(Colors.Red);

    public BrushCollection BrushCollection { get; set; } = new(); // Brush Collection

    #endregion

    [TinyhandOnDeserialized]
    public void OnDeserialized()
    {
        Scaler.ViewScale = this.ViewScale;
    }

    [TinyhandOnSerializing]
    public void OnSerializing()
    {
        this.ViewScale = Scaler.ViewScale;
    }
}

[TinyhandObject]
public partial class BrushCollection
{
    [Key(0)]
    public BrushOption Brush1 { get; set; } = new(Colors.BurlyWood);

    public BrushOption this[string name]
    {
        get
        {
            return this.Brush1;
        }
    }
}
