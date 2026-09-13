// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using Tinyhand;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Arc.WinAPI;

[TinyhandObject]
public partial class DipWindowPlacement
{ // Device Independent, 1/96 inch
    [Key(0)]
    public int Length { get; set; }

    [Key(1)]
    public int Flags { get; set; }

    [Key(2)]
    public SW ShowCommand { get; set; }

    [Key(3)]
    public DipPoint MinPosition { get; set; } = new DipPoint();

    [Key(4)]
    public DipPoint MaxPosition { get; set; } = new DipPoint();

    [Key(5)]
    public DipRect NormalPosition { get; set; } = new DipRect();

    public DipWindowPlacement()
    {
    }

    /// <summary>
    /// Converts all positions and sizes from physical pixels to DIPs.
    /// </summary>
    /// <param name="windowPlacement">The native window placement.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    public void FromWindowPlacement(WINDOWPLACEMENT windowPlacement, double dpiX, double dpiY)
    {
        this.Length = windowPlacement.length;
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
    public WINDOWPLACEMENT ToWindowPlacement(double dpiX, double dpiY)
    {
        return new WINDOWPLACEMENT
        {
            length = this.Length,
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
    public void FromWindowPlacementWithPhysicalPosition(WINDOWPLACEMENT windowPlacement, double dpiX, double dpiY)
    {
        this.Length = windowPlacement.length;
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
    public WINDOWPLACEMENT ToWindowPlacementWithPhysicalPosition(double dpiX, double dpiY)
    {
        return new WINDOWPLACEMENT
        {
            length = this.Length,
            flags = this.Flags,
            showCmd = this.ShowCommand,
            minPosition = this.MinPosition.ToPointUnscaled(),
            maxPosition = this.MaxPosition.ToPointUnscaled(),
            normalPosition = this.NormalPosition.ToRectWithPhysicalPosition(dpiX, dpiY),
        };
    }
}

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

    public DipPoint(POINT point, double dpiX, double dpiY)
    {
        this.FromPoint(point, dpiX, dpiY);
    }

    public DipPoint()
    {
    }

    public void FromPoint(POINT point, double dpiX, double dpiY)
    {
        this.X = point.X * 96 / dpiX;
        this.Y = point.Y * 96 / dpiY;
    }

    public POINT ToPoint(double dpiX, double dpiY)
    {
        return new POINT((int)(this.X * dpiX / 96), (int)(this.Y * dpiY / 96));
    }

    /// <summary>
    /// Copies the point without DPI conversion.
    /// </summary>
    /// <param name="point">The native point.</param>
    public void FromPointUnscaled(POINT point)
    {
        this.X = point.X;
        this.Y = point.Y;
    }

    /// <summary>
    /// Returns the point without DPI conversion.
    /// </summary>
    /// <returns>The native point.</returns>
    public POINT ToPointUnscaled()
    {
        return new POINT((int)this.X, (int)this.Y);
    }
}

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

    public DipRect(RECT rect, double dpiX, double dpiY)
    {
        this.FromRect(rect, dpiX, dpiY);
    }

    public DipRect()
    {
    }

    public void FromRect(RECT rect, double dpiX, double dpiY)
    {
        this.Left = rect.Left * 96 / dpiX;
        this.Top = rect.Top * 96 / dpiY;
        this.Right = rect.Right * 96 / dpiX;
        this.Bottom = rect.Bottom * 96 / dpiY;
    }

    public RECT ToRect(double dpiX, double dpiY)
    {
        return new RECT((int)(this.Left * dpiX / 96), (int)(this.Top * dpiY / 96), (int)(this.Right * dpiX / 96), (int)(this.Bottom * dpiY / 96));
    }

    /// <summary>
    /// Keeps the top-left position in physical pixels and converts only the size from physical pixels to DIPs.
    /// </summary>
    /// <param name="rect">The native rectangle.</param>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    public void FromRectWithPhysicalPosition(RECT rect, double dpiX, double dpiY)
    {
        this.Left = rect.Left;
        this.Top = rect.Top;
        this.Right = rect.Left + ((rect.Right - rect.Left) * 96 / dpiX);
        this.Bottom = rect.Top + ((rect.Bottom - rect.Top) * 96 / dpiY);
    }

    /// <summary>
    /// Keeps the top-left position in physical pixels and converts only the size from DIPs to physical pixels.
    /// </summary>
    /// <param name="dpiX">The horizontal DPI.</param>
    /// <param name="dpiY">The vertical DPI.</param>
    /// <returns>The native rectangle.</returns>
    public RECT ToRectWithPhysicalPosition(double dpiX, double dpiY)
    {
        return new RECT((int)this.Left, (int)this.Top, (int)(this.Left + (this.Width * dpiX / 96)), (int)(this.Top + (this.Height * dpiY / 96)));
    }
}

/// <summary>
/// Native (Win32) methods.
/// </summary>
public partial class NativeMethods
{
    [DllImport("user32.dll")]
    internal static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
}

[TinyhandObject]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct WINDOWPLACEMENT
{
    [Key(0)]
    public int length;
    [Key(1)]
    public int flags;
    [Key(2)]
    public SW showCmd;
    [Key(3)]
    public POINT minPosition;
    [Key(4)]
    public POINT maxPosition;
    [Key(5)]
    public RECT normalPosition;
}

[TinyhandObject]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct POINT
{
    [Key(0)]
    public int X;
    [Key(1)]
    public int Y;

    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

[TinyhandObject]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct RECT
{
    [Key(0)]
    public int Left;
    [Key(1)]
    public int Top;
    [Key(2)]
    public int Right;
    [Key(3)]
    public int Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }
}

public enum SW
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
}
