// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Arc.WinUI;

/// <summary>
/// Stores a default or customized brush color for serialization and data binding.
/// </summary>
[TinyhandObject]
public partial class BrushOption : ObservableObject
{ // Constructor -> (OnDeserialized()) -> Prepare() -> ... -> OnSerializing()
    public BrushOption()
        : this(Colors.Black)
    {
    }

    public BrushOption(Color color)
    {
        this.initialColorArgb = ColorToArgb(color);
        this.ColorArgb = this.initialColorArgb;
    }

    private int initialColorArgb;
    private SolidColorBrush? brush; // [ObservableProperty]

    [IgnoreMember]
    public SolidColorBrush Brush
    {
        get => this.brush ??= this.IsColorChanged ? new(ArgbToColor(this.ColorArgb)) : new(ArgbToColor(this.initialColorArgb));
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<SolidColorBrush>.Default.Equals(this.brush, value))
            {
                this.brush = value;
                this.OnPropertyChanged(nameof(BrushOption.Brush));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the color has been changed from the initial color.
    /// </summary>
    [Key(0)]
    public bool IsColorChanged { get; set; } // true:changed, false:default

    /// <summary>
    /// Gets or sets the color as a 32-bit ARGB value.
    /// </summary>
    [Key(1)]
    public int ColorArgb { get; set; }

    public void SetColor(Color color)
    {
        this.IsColorChanged = true;
        this.ColorArgb = ColorToArgb(color);
        this.Brush = new SolidColorBrush(color);
    }

    public void Reset()
    {
        if (this.IsColorChanged)
        {
            this.IsColorChanged = false;
            this.ColorArgb = this.initialColorArgb;
            this.Brush = new SolidColorBrush(ArgbToColor(this.ColorArgb));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ColorToArgb(Color color)
        => (int)color.A << 24 | (int)color.R << 16 | (int)color.G << 8 | (int)color.B;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color ArgbToColor(int argb)
        => Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
}
