// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Arc.WinAPI;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace Arc.WPF
{
    public class DpiManager
    { // dpi manager. per monitor dpiを管理する。
        public static bool IsSupported
        {
            get
            {
                var version = Environment.OSVersion.Version;
                return (version.Major > 6) || (version.Major == 6 && version.Minor >= 3);
            }
        }

        public Window Window { get; private set; }

        public HwndSource Source { get; private set; }

        public Dpi SystemDpi { get; private set; }

        public Dpi CurrentDpi { get; private set; }

        public bool PerMonitorDPI { get; set; } // per-monitor dpi

        public double OriginalFontSize { get; set; }

        public double FontSize
        {
            get { return this.OriginalFontSize * this.CurrentSystemRatio.Y; }
        }

        public DpiRatio CurrentPreviousRatio { get; private set; } // 現在のdpiと以前のdpiの比率

        public DpiRatio CurrentSystemRatio { get; private set; } // 現在のdpiとシステム（デフォルト）dpiの比率

        public DpiManager(Window window)
        {
            this.Window = window;
            if (this.Window == null)
            {
                throw new ArgumentNullException();
            }

            var hwnd = PresentationSource.FromVisual(window) as HwndSource;
            if (hwnd == null)
            {
                throw new ArgumentNullException();
            }

            this.Source = hwnd;

            this.SystemDpi = this.GetSystemDpi();
            this.CurrentDpi = this.GetDpi();
            this.CurrentPreviousRatio = DpiRatio.Default;
            this.CurrentSystemRatio = new DpiRatio(this.CurrentDpi.X / this.SystemDpi.X, this.CurrentDpi.Y / this.SystemDpi.Y);
        }

        public void ChangeDpi(Dpi dpi)
        { // change dpi
            if (dpi.X < 1.0d || dpi.Y < 1.0d)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.CurrentPreviousRatio = new DpiRatio((double)dpi.X / this.CurrentDpi.X, (double)dpi.Y / this.CurrentDpi.Y);
            this.CurrentSystemRatio = new DpiRatio((double)dpi.X / this.SystemDpi.X, (double)dpi.Y / this.SystemDpi.Y);

            this.CurrentDpi = dpi;
        }

        public void AdjustWindowPlacement(ref WINDOWPLACEMENT wp, Dpi current, Dpi target)
        { // adjust window placement rev
            double rx, ry;
            if (current.X < Dpi.MinimunValue)
            {
                rx = 1.0d;
            }
            else
            {
                rx = (double)target.X / current.X;
            }

            if (current.Y < Dpi.MinimunValue)
            {
                ry = 1.0d;
            }
            else
            {
                ry = (double)target.Y / current.Y;
            }

            wp.normalPosition.Bottom = wp.normalPosition.Top + (int)((wp.normalPosition.Bottom - wp.normalPosition.Top) * ry);
            wp.normalPosition.Right = wp.normalPosition.Left + (int)((wp.normalPosition.Right - wp.normalPosition.Left) * rx);
        }

        /*public void AdjustWindowPlacement(ref WINDOWPLACEMENT wp)
        {//adjust window placement rev
            wp.normalPosition.Bottom = wp.normalPosition.Top + (int)((wp.normalPosition.Bottom - wp.normalPosition.Top) * CurrentSystemRatio.Y);
            wp.normalPosition.Right = wp.normalPosition.Left + (int)((wp.normalPosition.Right - wp.normalPosition.Left) * CurrentSystemRatio.X);
        }
        public void AdjustWindowPlacement2(ref WINDOWPLACEMENT wp)
        {//adjust window placement rev
            wp.normalPosition.Bottom = wp.normalPosition.Top + (int)((wp.normalPosition.Bottom - wp.normalPosition.Top) / CurrentSystemRatio.Y);
            wp.normalPosition.Right = wp.normalPosition.Left + (int)((wp.normalPosition.Right - wp.normalPosition.Left) / CurrentSystemRatio.X);
        }*/

        public Dpi GetSystemDpi()
        { // get system dpi
            if (this.Source.CompositionTarget != null)
            {
                return new Dpi(
                    (uint)(Dpi.Default.X * this.Source.CompositionTarget.TransformToDevice.M11),
                    (uint)(Dpi.Default.Y * this.Source.CompositionTarget.TransformToDevice.M22));
            }

            return Dpi.Default;
        }

        public Dpi GetDpi(MonitorDpiType dpiType = MonitorDpiType.Default)
        { // get dpi
            if (!IsSupported)
            {
                return Dpi.Default;
            }

            var hmonitor = Arc.WinAPI.Methods.MonitorFromWindow(this.Source.Handle, MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            uint dpiX = 1, dpiY = 1;
            Arc.WinAPI.Methods.GetDpiForMonitor(hmonitor, dpiType, ref dpiX, ref dpiY);
            return new Dpi(dpiX, dpiY);
        }

        public delegate int SetProcessDpiAwarenessDelegate(ProcessDpiAwareness dpi);

        public static void SetProcessDpiAwareness(ProcessDpiAwareness awareness)
        { // AssemblyInfo.cs [assembly: System.Windows.Media.DisableDpiAwareness], manifestでもDpiAwarenessを削除する。
            IntPtr ptrLib = Arc.WinAPI.Methods.LoadLibrary("Shcore.dll");
            try
            {
                IntPtr setPtr = Arc.WinAPI.Methods.GetProcAddress(ptrLib, "SetProcessDpiAwareness");
                var setFunc = (SetProcessDpiAwarenessDelegate)Marshal.GetDelegateForFunctionPointer(setPtr, typeof(SetProcessDpiAwarenessDelegate));
                if (setFunc == null)
                {
                    return;
                }

                var setRes = setFunc(awareness);
            }
            catch
            {
            }
            finally
            {
                Arc.WinAPI.Methods.FreeLibrary(ptrLib);
            }
        }

        public delegate IntPtr SetThreadDpiAwarenessContextDelegate(IntPtr dpi);

        // delegate IntPtr SetThreadDpiAwarenessContextDelegate(int dpi);
        public static void SetThreadDpiAwarenessContext()
        { // Enables automatic scaling of the non-client area portions of the specified top-level window.
            IntPtr ptrLib = Arc.WinAPI.Methods.LoadLibrary("User32.dll");
            try
            {
                IntPtr setPtr = Arc.WinAPI.Methods.GetProcAddress(ptrLib, "SetThreadDpiAwarenessContext");
                var setFunc = (SetThreadDpiAwarenessContextDelegate)Marshal.GetDelegateForFunctionPointer(setPtr, typeof(SetThreadDpiAwarenessContextDelegate));
                if (setFunc == null)
                {
                    return;
                }

                var setRes = setFunc(new IntPtr(-3));

                // var setRes2 = setFunc(new IntPtr(18));
                // var setRes = setFunc(-3);

                // IntPtr enablePtr = Arc.WinAPI.Methods.GetProcAddress(ptrLib, "EnableNonClientDpiScaling");
                // var enableFunc = (EnableNonClientDpiScalingDelegate)Marshal.GetDelegateForFunctionPointer(enablePtr, typeof(EnableNonClientDpiScalingDelegate));
                // if (enableFunc == null) return;
                // var res = enableFunc(hwnd);
            }
            catch
            {
            }
            finally
            {
                Arc.WinAPI.Methods.FreeLibrary(ptrLib);
            }
        }
    }

    /*delegate IntPtr SetThreadDpiAwarenessContextDelegate(IntPtr dpi);
    delegate bool EnableNonClientDpiScalingDelegate(IntPtr hwnd);
    public static void EnableNonClientDpiScaling()
    {//Enables automatic scaling of the non-client area portions of the specified top-level window.
        IntPtr ptrLib = Arc.WinAPI.Methods.LoadLibrary("User32.dll");
        try
        {
            IntPtr setPtr = Arc.WinAPI.Methods.GetProcAddress(ptrLib, "SetThreadDpiAwarenessContext");
            var setFunc = (SetThreadDpiAwarenessContextDelegate)Marshal.GetDelegateForFunctionPointer(setPtr, typeof(SetThreadDpiAwarenessContextDelegate));
            if (setFunc == null) return;
            var setRes = setFunc(new IntPtr(-3));
            //setRes = setFunc(new IntPtr(18));

            //IntPtr enablePtr = Arc.WinAPI.Methods.GetProcAddress(ptrLib, "EnableNonClientDpiScaling");
            //var enableFunc = (EnableNonClientDpiScalingDelegate)Marshal.GetDelegateForFunctionPointer(enablePtr, typeof(EnableNonClientDpiScalingDelegate));
            //if (enableFunc == null) return;
            //var res = enableFunc(hwnd);
        }
        catch { }
        finally { Arc.WinAPI.Methods.FreeLibrary(ptrLib); }
    }*/

    public struct DpiRatio
    { // dpi ratio
        public static readonly DpiRatio Default = new DpiRatio(1.0d, 1.0d);

        public double X { get; set; }

        public double Y { get; set; }

        public DpiRatio(double x, double y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(DpiRatio dpi1, DpiRatio dpi2)
        {
            return dpi1.X == dpi2.X && dpi1.Y == dpi2.Y;
        }

        public static bool operator !=(DpiRatio dpi1, DpiRatio dpi2)
        {
            return !(dpi1 == dpi2);
        }

        public bool Equals(DpiRatio other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is DpiRatio && this.Equals((DpiRatio)obj);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
    }

    public struct Dpi
    { // dpi
        public static readonly uint MinimunValue = 32;
        public static readonly uint DefaultValue = 96;
        public static readonly Dpi Default = new Dpi(96, 96);

        public uint X { get; set; }

        public uint Y { get; set; }

        public Dpi(uint x, uint y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(Dpi dpi1, Dpi dpi2)
        {
            return dpi1.X == dpi2.X && dpi1.Y == dpi2.Y;
        }

        public static bool operator !=(Dpi dpi1, Dpi dpi2)
        {
            return !(dpi1 == dpi2);
        }

        public bool Equals(Dpi other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Dpi && this.Equals((Dpi)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)this.X * 397) ^ (int)this.Y;
            }
        }
    }
}
