﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using StandardWinUI;
using Tinyhand;
using Windows.UI;

#pragma warning disable SA1649 // File name should match first type name

namespace Arc.Views;

[TinyhandObject]
public partial class BrushOption : ObservableObject, ITinyhandSerializationCallback
{ // Constructor -> (OnAfterDeserialize()) -> Prepare() -> ... -> OnBeforeSerialize()
    private Color initialColor;

    [ObservableProperty]
    private SolidColorBrush? brush;

    public BrushOption()
        : this(Colors.Black)
    {
    }

    public BrushOption(Color initialColor)
    {
        this.initialColor = initialColor;
        App.TryEnqueueOnUI(() =>
        {
            if (this.Brush == null)
            {
                this.Brush = new SolidColorBrush(initialColor);
            }
        });
    }

    [Key(0)]
    public bool ChangedFlag { get; set; } // true:changed, false:default

    [Key(1)]
    public int BrushColor { get; set; }

    public void Change(Color color)
    {
        this.Brush = new SolidColorBrush(color);
        this.ChangedFlag = true;
    }

    public void OnAfterDeserialize()
    { // After data has loaded.
        if (this.ChangedFlag)
        {
            this.Brush = new SolidColorBrush(Color.FromArgb((byte)(this.BrushColor >> 24), (byte)(this.BrushColor >> 16), (byte)(this.BrushColor >> 8), (byte)this.BrushColor));
        }
    }

    public void OnAfterReconstruct()
    {
    }

    public void OnBeforeSerialize()
    { // Before data is saved.
        if (this.Brush != null)
        {
            this.BrushColor = (this.Brush.Color.A << 24) | (this.Brush.Color.R << 16) | (this.Brush.Color.G << 8) | this.Brush.Color.B;
        }
        else
        {
            this.BrushColor = 0;
        }
    }
}
