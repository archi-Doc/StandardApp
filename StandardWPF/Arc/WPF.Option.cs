// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Arc.Mvvm;
using Tinyhand;

#pragma warning disable SA1649 // File name should match first type name

namespace Arc.WPF;

/// <summary>
/// Stores a default or customized brush color for serialization and data binding.
/// </summary>
[TinyhandObject]
public partial class BrushOption : BindableBase
{ // Constructor -> (OnDeserialized()) -> Prepare() -> ... -> OnSerializing()
    private SolidColorBrush? brush;

    public BrushOption()
        : this(Colors.Black)
    {
    }

    public BrushOption(Color initialColor)
    {
        if (this.Brush == null)
        {
            this.Brush = new SolidColorBrush(initialColor);
        }
    }

    [IgnoreMember]
    public SolidColorBrush? Brush
    {
        get { return this.brush; }
        private set { this.SetProperty(ref this.brush, value); }
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
        this.Brush = new SolidColorBrush(color);
        this.IsColorChanged = true;
    }

    [TinyhandOnDeserialized]
    public void OnDeserialized()
    { // After data has loaded.
        if (this.IsColorChanged)
        {
            this.Brush = new SolidColorBrush(Color.FromArgb((byte)(this.ColorArgb >> 24), (byte)(this.ColorArgb >> 16), (byte)(this.ColorArgb >> 8), (byte)this.ColorArgb));
        }
    }

    [TinyhandOnSerializing]
    public void OnSerializing()
    { // Before data is saved.
        if (this.Brush != null)
        {
            this.ColorArgb = (this.Brush.Color.A << 24) | (this.Brush.Color.R << 16) | (this.Brush.Color.G << 8) | this.Brush.Color.B;
        }
        else
        {
            this.ColorArgb = 0;
        }
    }
}
