// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1310 // Field names should not contain underscore
#pragma warning disable SA1601 // Partial elements should be documented
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace Arc.WinAPI
{
    public static class Extensions
    {
        public static ushort ToLoWord(this IntPtr dword)
        {
            return (ushort)((uint)dword & 0xffff);
        }

        public static ushort ToHiWord(this IntPtr dword)
        {
            return (ushort)((uint)dword >> 16);
        }
    }

    public static class Const
    {
        public const string SHELL_IDLIST_STRING = "Shell IDList Array";
    }

    public partial class Methods
    {
        [DllImport("shell32.dll")]
        public static extern IntPtr ILCombine(IntPtr pidl1, IntPtr pidl2);

        [DllImport("shell32.dll")]
        public static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll")]
        public static extern uint SHGetNameFromIDList(IntPtr pidl, SIGDN sigdnName, [Out, MarshalAs(UnmanagedType.LPWStr)] out string str);

        [DllImport("shell32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

        public enum SIGDN : uint
        {
            NORMALDISPLAY = 0x00000000,
            PARENTRELATIVEPARSING = 0x80018001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000,
            PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            PARENTRELATIVE = 0x80080001,
            PARENTRELATIVEFORUI = 0x80094001,
        }

        public static string[]? GetPathFromIDList(System.Windows.IDataObject dataObject)
        {
            string[]? result = null;
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
            MemoryStream data = (MemoryStream)dataObject.GetData(Arc.WinAPI.Const.SHELL_IDLIST_STRING);
            if (data == null)
            {
                return result;
            }

            var b = data.ToArray();
            IntPtr p = Marshal.AllocHGlobal(b.Length);
            Marshal.Copy(b, 0, p, b.Length);

            // Get number of items.
            var cidl = (uint)Marshal.ReadInt32(p);
            result = new string[cidl];

            // Get parent folder.
            int offset = sizeof(uint);
            IntPtr parentpidl = (IntPtr)((int)p + (uint)Marshal.ReadInt32(p, offset));
            SIGDN sigdn = SIGDN.DESKTOPABSOLUTEPARSING;

            // SHGetNameFromIDList(parentpidl, sigdn, out ts);

            // Get subitems.
            for (int n = 0; n < cidl; ++n)
            {
                offset += sizeof(uint);
                IntPtr relpidl = (IntPtr)((int)p + (uint)Marshal.ReadInt32(p, offset));
                IntPtr abspidl = ILCombine(parentpidl, relpidl);
                SHGetNameFromIDList(abspidl, sigdn, out result[n]);
                ILFree(abspidl);
            }

            return result;
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, ShowCommands nShowCmd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);

        [DllImport("SHCore.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY);

        /// <summary>
        /// Get the process with the same process name and the same module name.
        /// </summary>
        /// <returns>Process.</returns>
        public static Process? GetPreviousProcess()
        {
            Process curProcess = Process.GetCurrentProcess();
            Process[] allProcesses = Process.GetProcessesByName(curProcess.ProcessName);

            foreach (Process checkProcess in allProcesses)
            {
                // 自分自身のプロセスIDは無視する
                if (checkProcess.Id != curProcess.Id)
                {
                    // プロセスのフルパス名を比較して同じアプリケーションか検証
                    if (string.Compare(checkProcess.MainModule.FileName, curProcess.MainModule.FileName, true) == 0)
                    {
                        // 同じフルパス名のプロセスを取得
                        return checkProcess;
                    }
                }
            }

            // 同じアプリケーションのプロセスが見つからない！
            return null;
        }

        /// <summary>
        /// Brings the window into the foreground and activates the window.
        /// </summary>
        public static void WakeupWindow(IntPtr hWnd)
        {
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SW_RESTORE);
            }

            SetForegroundWindow(hWnd);
        }

        public static void SendKey(VirtualKeyCode keyCode)
        {
            INPUT[] input = new INPUT[2];

            input[0].Type = (uint)InputType.Keyboard;
            input[0].Data.Keyboard = new KEYBDINPUT
            {
                KeyCode = (ushort)keyCode,
                Scan = 0,
                Flags = IsExtendedKey(keyCode) ? (uint)KeyboardFlag.ExtendedKey : 0,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            };

            input[1].Type = (uint)InputType.Keyboard;
            input[1].Data.Keyboard = new KEYBDINPUT
            {
                KeyCode = (ushort)keyCode,
                Scan = 0,
                Flags = (IsExtendedKey(keyCode) ? (uint)KeyboardFlag.ExtendedKey : 0) | (uint)KeyboardFlag.KeyUp,
                Time = 0,
                ExtraInfo = IntPtr.Zero,
            };

            var result = SendInput(2, input, Marshal.SizeOf(typeof(INPUT)));
        }

        public static bool IsExtendedKey(VirtualKeyCode keyCode)
        {
            if (keyCode == VirtualKeyCode.MENU ||
                keyCode == VirtualKeyCode.LMENU ||
                keyCode == VirtualKeyCode.RMENU ||
                keyCode == VirtualKeyCode.CONTROL ||
                keyCode == VirtualKeyCode.RCONTROL ||
                keyCode == VirtualKeyCode.INSERT ||
                keyCode == VirtualKeyCode.DELETE ||
                keyCode == VirtualKeyCode.HOME ||
                keyCode == VirtualKeyCode.END ||
                keyCode == VirtualKeyCode.PRIOR ||
                keyCode == VirtualKeyCode.NEXT ||
                keyCode == VirtualKeyCode.RIGHT ||
                keyCode == VirtualKeyCode.UP ||
                keyCode == VirtualKeyCode.LEFT ||
                keyCode == VirtualKeyCode.DOWN ||
                keyCode == VirtualKeyCode.NUMLOCK ||
                keyCode == VirtualKeyCode.CANCEL ||
                keyCode == VirtualKeyCode.SNAPSHOT ||
                keyCode == VirtualKeyCode.DIVIDE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the cursor position (relative to control coordinate).
        /// </summary>
        /// <returns>Cursor position.</returns>
        public static Point GetNowPosition(Visual v)
        {
            POINT32 p;

            GetCursorPos(out p);
            return v.PointFromScreen(new Point(p.X, p.Y));
            /*var source = HwndSource.FromVisual(v) as HwndSource;
            var hwnd = source.Handle;

            ScreenToClient(hwnd, ref p);
            return new Point(p.X, p.Y);*/
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hLibModule);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        // 外部プロセスのメイン・ウィンドウを起動するためのWin32 API
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        // キー操作、マウス操作をシミュレート(擬似的に操作する)
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbsize);

        // 仮想キーコードをスキャンコードに変換
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        internal static extern int MapVirtualKey(int wCode, int wMapType);

        [DllImport("user32.dll")]
        internal static extern void GetCursorPos(out POINT32 pt);

        [DllImport("user32.dll")]
        internal static extern int ScreenToClient(IntPtr hwnd, ref POINT32 pt);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        // ShowWindowAsync関数のパラメータに渡す定義値
        public const int SW_RESTORE = 9;  // 画面を元の大きさに戻す

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const uint WM_SETICON = 0x0080;

        public static void RemoveIcon(Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            // Change the extended window style to not show a window icon
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
            SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public enum ShowCommands : int
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
        SW_MAX = 11,
    }

    public enum ProcessDpiAwareness
    {
        Unaware,
        Aware,
        PerMonitorAware,
    }

    public enum MonitorDefaultTo
    {
        MONITOR_DEFAULTTONULL = 0,
        MONITOR_DEFAULTTOPRIMARY = 1,
        MONITOR_DEFAULTTONEAREST = 2,
    }

    public enum MonitorDpiType
    {
        EffectiveDpi = 0,
        AngularDpi = 1,
        RawDpi = 2,
        Default = EffectiveDpi,
    }

    public enum FOFunc : uint
    {
        FO_MOVE = 0x0001,
        FO_COPY = 0x0002,
        FO_DELETE = 0x0003,
        FO_RENAME = 0x0004,
    }

    public enum FOFlags : ushort
    {
        FOF_MULTIDESTFILES = 0x0001,
        FOF_CONFIRMMOUSE = 0x0002,
        FOF_SILENT = 0x0004,  // don't create progress/report
        FOF_RENAMEONCOLLISION = 0x0008,
        FOF_NOCONFIRMATION = 0x0010,  // Don't prompt the user.
        FOF_WANTMAPPINGHANDLE = 0x0020,  // Fill in SHFILEOPSTRUCT.hNameMappings
        FOF_ALLOWUNDO = 0x0040, // Must be freed using SHFreeNameMappings
        FOF_FILESONLY = 0x0080,  // on *.*, do only files
        FOF_SIMPLEPROGRESS = 0x0100,  // means don't show names of files
        FOF_NOCONFIRMMKDIR = 0x0200,  // don't confirm making any needed dirs
        FOF_NOERRORUI = 0x0400,  // don't put up error UI
        FOF_NOCOPYSECURITYATTRIBS = 0x0800,  // dont copy NT file Security Attributes
        FOF_NORECURSION = 0x1000,  // don't recurse into directories.
        FOF_NO_CONNECTED_ELEMENTS = 0x2000,  // don't operate on connected elements.
        FOF_WANTNUKEWARNING = 0x4000,  // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
        FOF_NORECURSEREPARSE = 0x8000,  // treat reparse points as objects, not containers
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public FOFunc wFunc;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pFrom;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pTo;
        public FOFlags fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszProgressTitle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT32
    {
        public uint X;
        public uint Y;
    }

    internal struct INPUT
    {
        public uint Type; // 0:mouse, 1:keyboard, 2:hardware
        public MOUSEKEYBDHARDWAREINPUT Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;

        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;

        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
    }

#pragma warning disable CS0649

    internal struct HARDWAREINPUT
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }

    internal struct KEYBDINPUT
    {
        public ushort KeyCode;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    internal struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

#pragma warning restore CS0649

    public enum InputType : uint
    {
        Mouse = 0, // Mouse event
        Keyboard = 1, // Keyboard event
        Hardware = 2, // Hardware event
    }

    [Flags]
    public enum KeyboardFlag : uint
    {
        KeyDown = 0x00, // key down
        ExtendedKey = 0x01, // extended key
        KeyUp = 0x02, // key up
        Unicode = 0x04, // unicode
        ScanCode = 0x08, // scancode
    }

    public enum VirtualKeyCode
    { // UInt16
        LBUTTON = 0x01,
        RBUTTON = 0x02,
        CANCEL = 0x03,
        MBUTTON = 0x04,
        XBUTTON1 = 0x05,
        XBUTTON2 = 0x06,
        BACK = 0x08,
        TAB = 0x09,
        CLEAR = 0x0C,
        RETURN = 0x0D,
        SHIFT = 0x10,
        CONTROL = 0x11,
        MENU = 0x12,
        PAUSE = 0x13,
        CAPITAL = 0x14,
        KANA = 0x15,
        HANGEUL = 0x15,
        HANGUL = 0x15,
        JUNJA = 0x17,
        FINAL = 0x18,
        HANJA = 0x19,
        KANJI = 0x19,
        ESCAPE = 0x1B,
        CONVERT = 0x1C,
        NONCONVERT = 0x1D,
        ACCEPT = 0x1E,
        MODECHANGE = 0x1F,
        SPACE = 0x20,
        PRIOR = 0x21,
        NEXT = 0x22,
        END = 0x23,
        HOME = 0x24,
        LEFT = 0x25,
        UP = 0x26,
        RIGHT = 0x27,
        DOWN = 0x28,
        SELECT = 0x29,
        PRINT = 0x2A,
        EXECUTE = 0x2B,
        SNAPSHOT = 0x2C,
        INSERT = 0x2D,
        DELETE = 0x2E,
        HELP = 0x2F,
        VK_0 = 0x30,
        VK_1 = 0x31,
        VK_2 = 0x32,
        VK_3 = 0x33,
        VK_4 = 0x34,
        VK_5 = 0x35,
        VK_6 = 0x36,
        VK_7 = 0x37,
        VK_8 = 0x38,
        VK_9 = 0x39,
        VK_A = 0x41,
        VK_B = 0x42,
        VK_C = 0x43,
        VK_D = 0x44,
        VK_E = 0x45,
        VK_F = 0x46,
        VK_G = 0x47,
        VK_H = 0x48,
        VK_I = 0x49,
        VK_J = 0x4A,
        VK_K = 0x4B,
        VK_L = 0x4C,
        VK_M = 0x4D,
        VK_N = 0x4E,
        VK_O = 0x4F,
        VK_P = 0x50,
        VK_Q = 0x51,
        VK_R = 0x52,
        VK_S = 0x53,
        VK_T = 0x54,
        VK_U = 0x55,
        VK_V = 0x56,
        VK_W = 0x57,
        VK_X = 0x58,
        VK_Y = 0x59,
        VK_Z = 0x5A,
        LWIN = 0x5B,
        RWIN = 0x5C,
        APPS = 0x5D,
        SLEEP = 0x5F,
        NUMPAD0 = 0x60,
        NUMPAD1 = 0x61,
        NUMPAD2 = 0x62,
        NUMPAD3 = 0x63,
        NUMPAD4 = 0x64,
        NUMPAD5 = 0x65,
        NUMPAD6 = 0x66,
        NUMPAD7 = 0x67,
        NUMPAD8 = 0x68,
        NUMPAD9 = 0x69,
        MULTIPLY = 0x6A,
        ADD = 0x6B,
        SEPARATOR = 0x6C,
        SUBTRACT = 0x6D,
        DECIMAL = 0x6E,
        DIVIDE = 0x6F,
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,
        NUMLOCK = 0x90,
        SCROLL = 0x91,
        LSHIFT = 0xA0,
        RSHIFT = 0xA1,
        LCONTROL = 0xA2,
        RCONTROL = 0xA3,
        LMENU = 0xA4,
        RMENU = 0xA5,
        BROWSER_BACK = 0xA6,
        BROWSER_FORWARD = 0xA7,
        BROWSER_REFRESH = 0xA8,
        BROWSER_STOP = 0xA9,
        BROWSER_SEARCH = 0xAA,
        BROWSER_FAVORITES = 0xAB,
        BROWSER_HOME = 0xAC,
        VOLUME_MUTE = 0xAD,
        VOLUME_DOWN = 0xAE,
        VOLUME_UP = 0xAF,
        MEDIA_NEXT_TRACK = 0xB0,
        MEDIA_PREV_TRACK = 0xB1,
        MEDIA_STOP = 0xB2,
        MEDIA_PLAY_PAUSE = 0xB3,
        LAUNCH_MAIL = 0xB4,
        LAUNCH_MEDIA_SELECT = 0xB5,
        LAUNCH_APP1 = 0xB6,
        LAUNCH_APP2 = 0xB7,
        OEM_1 = 0xBA,
        OEM_PLUS = 0xBB,
        OEM_COMMA = 0xBC,
        OEM_MINUS = 0xBD,
        OEM_PERIOD = 0xBE,
        OEM_2 = 0xBF,
        OEM_3 = 0xC0,
        OEM_4 = 0xDB,
        OEM_5 = 0xDC,
        OEM_6 = 0xDD,
        OEM_7 = 0xDE,
        OEM_8 = 0xDF,
        OEM_102 = 0xE2,
        PROCESSKEY = 0xE5,
        PACKET = 0xE7,
        ATTN = 0xF6,
        CRSEL = 0xF7,
        EXSEL = 0xF8,
        EREOF = 0xF9,
        PLAY = 0xFA,
        ZOOM = 0xFB,
        NONAME = 0xFC,
        PA1 = 0xFD,
        OEM_CLEAR = 0xFE,
    }
}
