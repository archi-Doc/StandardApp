// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Runtime.InteropServices;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Arc.WinUI;

/// <summary>
/// Win32 WINDOWPLACEMENT structure.
/// </summary>
[TinyhandObject]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct NativeWindowPlacement
{
    [Key(0)]
    public int length;
    [Key(1)]
    public int flags;
    [Key(2)]
    public ShowCommand showCmd;
    [Key(3)]
    public NativePoint minPosition;
    [Key(4)]
    public NativePoint maxPosition;
    [Key(5)]
    public NativeRect normalPosition;
}

/// <summary>
/// Win32 POINT structure.
/// </summary>
[TinyhandObject]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct NativePoint
{
    [Key(0)]
    public int X;
    [Key(1)]
    public int Y;

    public NativePoint(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

/// <summary>
/// Win32 RECT structure.
/// </summary>
[TinyhandObject]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct NativeRect
{
    [Key(0)]
    public int Left;
    [Key(1)]
    public int Top;
    [Key(2)]
    public int Right;
    [Key(3)]
    public int Bottom;

    public NativeRect(int left, int top, int right, int bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }
}

public enum ShowCommand
{
    HIDE = 0,
    SHOWNORMAL = 1,
    SHOWMINIMIZED = 2,
    SHOWMAXIMIZED = 3,
    SHOWNOACTIVATE = 4,
    SHOW = 5,
    MINIMIZE = 6,
    SHOWMINNOACTIVE = 7,
    SHOWNA = 8,
    RESTORE = 9,
    SHOWDEFAULT = 10,
    FORCEMINIMIZE = 11,
}

/// <summary>
/// Stores window placement with DPI conversion and optional physical screen positions.
/// </summary>
[TinyhandObject]
public partial class DipWindowPlacement
{ // Device Independent, 1/96 inch
    private const int WindowPlacementLength = 44;

    [Key(0)]
    public int Flags { get; set; }

    [Key(1)]
    public ShowCommand ShowCommand { get; set; }

    [Key(2)]
    public DipPoint MinPosition { get; set; } = new DipPoint();

    [Key(3)]
    public DipPoint MaxPosition { get; set; } = new DipPoint();

    [Key(4)]
    public DipRect NormalPosition { get; set; } = new DipRect();

    public DipWindowPlacement()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DipWindowPlacement"/> class.<br/>
    /// Same as <see cref="FromWindowPlacementWithPhysicalPosition(NativeWindowPlacement, double, double)"/>.
    /// </summary>
    /// <param name="windowPlacement">The native window placement.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    public DipWindowPlacement(NativeWindowPlacement windowPlacement, double dpiX, double dpiY)
    {
        this.FromWindowPlacementWithPhysicalPosition(windowPlacement, dpiX, dpiY);
    }

    public bool IsValid => this.NormalPosition.Width > 0 && this.NormalPosition.Height > 0;

    /// <summary>
    /// Converts all positions and sizes from physical pixels to DIPs.
    /// </summary>
    /// <param name="windowPlacement">The native window placement.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    public void FromWindowPlacement(NativeWindowPlacement windowPlacement, double dpiX, double dpiY)
    {
        this.Flags = windowPlacement.flags;
        this.ShowCommand = windowPlacement.showCmd;
        this.MinPosition.FromPoint(windowPlacement.minPosition, dpiX, dpiY);
        this.MaxPosition.FromPoint(windowPlacement.maxPosition, dpiX, dpiY);
        this.NormalPosition.FromRect(windowPlacement.normalPosition, dpiX, dpiY);
    }

    /// <summary>
    /// Converts all positions and sizes from DIPs to physical pixels.
    /// </summary>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    /// <returns>The native window placement.</returns>
    public NativeWindowPlacement ToWindowPlacement(double dpiX, double dpiY)
    {
        return new NativeWindowPlacement
        {
            length = WindowPlacementLength,
            flags = this.Flags,
            showCmd = this.ShowCommand,
            minPosition = this.MinPosition.ToPoint(dpiX, dpiY),
            maxPosition = this.MaxPosition.ToPoint(dpiX, dpiY),
            normalPosition = this.NormalPosition.ToRect(dpiX, dpiY),
        };
    }

    /// <summary>
    /// Keeps positions in physical pixels and converts only the window size from physical pixels to DIPs.
    /// </summary>
    /// <param name="windowPlacement">The native window placement.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    public void FromWindowPlacementWithPhysicalPosition(NativeWindowPlacement windowPlacement, double dpiX, double dpiY)
    {
        this.Flags = windowPlacement.flags;
        this.ShowCommand = windowPlacement.showCmd;
        this.MinPosition.FromPointUnscaled(windowPlacement.minPosition);
        this.MaxPosition.FromPointUnscaled(windowPlacement.maxPosition);
        this.NormalPosition.FromRectWithPhysicalPosition(windowPlacement.normalPosition, dpiX, dpiY);
    }

    /// <summary>
    /// Keeps positions in physical pixels and converts only the window size from DIPs to physical pixels.
    /// </summary>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    /// <returns>The native window placement.</returns>
    public NativeWindowPlacement ToWindowPlacementWithPhysicalPosition(double dpiX, double dpiY)
    {
        return new NativeWindowPlacement
        {
            length = WindowPlacementLength,
            flags = this.Flags,
            showCmd = this.ShowCommand,
            minPosition = this.MinPosition.ToPointUnscaled(),
            maxPosition = this.MaxPosition.ToPointUnscaled(),
            normalPosition = this.NormalPosition.ToRectWithPhysicalPosition(dpiX, dpiY),
        };
    }
}

/// <summary>
/// Stores a point with conversion between physical pixels and device-independent units.
/// </summary>
[TinyhandObject]
public partial class DipPoint
{ // Device Independent, 1/96 inch
    [Key(0)]
    public double X { get; set; }

    [Key(1)]
    public double Y { get; set; }

    public DipPoint(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    public DipPoint(NativePoint point, double dpiX, double dpiY)
    {
        this.FromPoint(point, dpiX, dpiY);
    }

    public DipPoint()
    {
    }

    public void FromPoint(NativePoint point, double dpiX, double dpiY)
    {
        this.X = point.X * 96d / dpiX;
        this.Y = point.Y * 96d / dpiY;
    }

    public NativePoint ToPoint(double dpiX, double dpiY)
    {
        return new NativePoint((int)(this.X * dpiX / 96), (int)(this.Y * dpiY / 96));
    }

    /// <summary>
    /// Copies the point without DPI conversion.
    /// </summary>
    /// <param name="point">The native point.</param>
    public void FromPointUnscaled(NativePoint point)
    {
        this.X = point.X;
        this.Y = point.Y;
    }

    /// <summary>
    /// Returns the point without DPI conversion.
    /// </summary>
    /// <returns>The native point.</returns>
    public NativePoint ToPointUnscaled()
    {
        return new NativePoint((int)this.X, (int)this.Y);
    }
}

/// <summary>
/// Stores a rectangle with DPI conversion and optional physical top-left coordinates.
/// </summary>
[TinyhandObject]
public partial class DipRect
{
    [Key(0)]
    public double Left { get; set; }

    [Key(1)]
    public double Top { get; set; }

    [Key(2)]
    public double Right { get; set; }

    [Key(3)]
    public double Bottom { get; set; }

    [IgnoreMember]
    public double Width => this.Right - this.Left;

    [IgnoreMember]
    public double Height => this.Bottom - this.Top;

    public DipRect(double left, double top, double right, double bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    public DipRect(NativeRect rect, double dpiX, double dpiY)
    {
        this.FromRect(rect, dpiX, dpiY);
    }

    public DipRect()
    {
    }

    public void FromRect(NativeRect rect, double dpiX, double dpiY)
    {
        this.Left = rect.Left * 96d / dpiX;
        this.Top = rect.Top * 96d / dpiY;
        this.Right = rect.Right * 96d / dpiX;
        this.Bottom = rect.Bottom * 96d / dpiY;
    }

    public NativeRect ToRect(double dpiX, double dpiY)
    {
        return new NativeRect((int)(this.Left * dpiX / 96), (int)(this.Top * dpiY / 96), (int)(this.Right * dpiX / 96), (int)(this.Bottom * dpiY / 96));
    }

    /// <summary>
    /// Keeps the top-left position in physical pixels and converts only the size from physical pixels to DIPs.
    /// </summary>
    /// <param name="rect">The native rectangle.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    public void FromRectWithPhysicalPosition(NativeRect rect, double dpiX, double dpiY)
    {
        this.Left = rect.Left;
        this.Top = rect.Top;
        this.Right = rect.Left + (((double)rect.Right - rect.Left) * 96d / dpiX);
        this.Bottom = rect.Top + (((double)rect.Bottom - rect.Top) * 96d / dpiY);
    }

    /// <summary>
    /// Keeps the top-left position in physical pixels and converts only the size from DIPs to physical pixels.
    /// </summary>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    /// <returns>The native rectangle.</returns>
    public NativeRect ToRectWithPhysicalPosition(double dpiX, double dpiY)
    {
        return new NativeRect((int)this.Left, (int)this.Top, (int)(this.Left + (this.Width * dpiX / 96)), (int)(this.Top + (this.Height * dpiY / 96)));
    }
}
