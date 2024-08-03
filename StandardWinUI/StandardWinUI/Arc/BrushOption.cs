// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

#pragma warning disable SA1649 // File name should match first type name

namespace Arc.Views;

[TinyhandObject]
public partial class BrushOption : ObservableObject
{ // Constructor -> (OnAfterDeserialize()) -> Prepare() -> ... -> OnBeforeSerialize()
    public BrushOption()
        : this(Colors.Black)
    {
    }

    public BrushOption(Color color)
    {
        this.initialColorInt = ColorToInt(color);
        this.ColorInt = this.initialColorInt;
    }

    private int initialColorInt;
    private SolidColorBrush? brush; // [ObservableProperty]

    [IgnoreMember]
    public SolidColorBrush Brush
    {
        get => this.brush ??= this.ColorChanged ? new(IntToColor(this.ColorInt)) : new(IntToColor(this.initialColorInt));
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<SolidColorBrush>.Default.Equals(this.brush, value))
            {
                this.brush = value;
                this.OnPropertyChanged(nameof(BrushOption.Brush));
            }
        }
    }

    [Key(0)]
    public bool ColorChanged { get; set; } // true:changed, false:default

    [Key(1)]
    public int ColorInt { get; set; }

    public void Change(Color color)
    {
        this.ColorChanged = true;
        this.ColorInt = ColorToInt(color);
        this.Brush = new SolidColorBrush(color);
    }

    public void Reset()
    {
        if (this.ColorChanged)
        {
            this.ColorChanged = false;
            this.ColorInt = this.initialColorInt;
            this.Brush = new SolidColorBrush(IntToColor(this.ColorInt));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ColorToInt(Color color)
        => (int)color.A << 24 | (int)color.R << 16 | (int)color.G << 8 | (int)color.B;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color IntToColor(int colorInt)
        => Color.FromArgb((byte)(colorInt >> 24), (byte)(colorInt >> 16), (byte)(colorInt >> 8), (byte)colorInt);
}
