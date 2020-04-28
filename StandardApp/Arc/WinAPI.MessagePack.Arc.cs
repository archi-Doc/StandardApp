// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using MessagePack;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1602 // Enumeration items should be documented

namespace Arc.WinAPI
{
    /// <summary>
    /// Arc.WinAPI Methods.
    /// </summary>
    public partial class Methods
    {
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
    }

    [MessagePackObject]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
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

    [MessagePackObject]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
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

    [MessagePackObject]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
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
}
