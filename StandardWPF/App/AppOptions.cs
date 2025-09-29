// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Windows.Media;
using Arc.WPF;

namespace StandardWPF;

[TinyhandObject]
public partial class AppOptions
{ // Application Options
    public AppOptions()
    {
    }

    [Key(0)]
    public BrushOption BrushTest { get; set; } = new(Colors.Red);

    [Key(1)]
    public BrushCollection BrushCollection { get; set; } = new(); // Brush Collection
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
