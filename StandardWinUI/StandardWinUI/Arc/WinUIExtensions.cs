// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Arc.Internal;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace Arc.WinUI;

public static class WinUIExtensions
{
    private const string OkString = "OK";

    public static async Task<ulong> ShowMessageDialogAsync(this Window window, ulong title, ulong content, ulong defaultCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0)
    {
        var dialog = new ContentDialog() { XamlRoot = window.Content.XamlRoot };
        if (window.Content is FrameworkElement element)
        {
            dialog.RequestedTheme = element.RequestedTheme;
        }

        var textBlock = new TextBlock() { Text = HashedString.Get(content), TextWrapping = TextWrapping.Wrap, };
        textBlock.FontSize *= App.Settings.DisplayScaling;
        dialog.Content = textBlock;
        if (title != 0)
        {
            dialog.Title = HashedString.Get(title);
        }

        if (defaultCommand != 0)
        {
            dialog.PrimaryButtonText = HashedString.Get(defaultCommand);
        }
        else
        {
            dialog.PrimaryButtonText = OkString;
        }

        if (secondaryCommand != 0)
        {
            dialog.SecondaryButtonText = HashedString.Get(secondaryCommand);
        }

        if (cancelCommand != 0)
        {
            dialog.CloseButtonText = HashedString.Get(cancelCommand);
        }

        var dialogTask = dialog.ShowAsync(ContentDialogPlacement.InPlace);
        HwndExtensions.SetForegroundWindow(window.GetWindowHandle());
        var result = await dialogTask;
        return result switch
        {
            ContentDialogResult.Primary => defaultCommand,
            ContentDialogResult.Secondary => secondaryCommand,
            ContentDialogResult.None => cancelCommand,
            _ => 0,
        };
    }

    public static void LoadWindowPlacement(this Window window, DipWindowPlacement windowPlacement)
    {
        if (windowPlacement.IsValid)
        {
            var hwnd = window.GetWindowHandle();
            Arc.Internal.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            var wp = windowPlacement.ToWINDOWPLACEMENT2(dpiX, dpiY);
            wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Arc.WinUI.WINDOWPLACEMENT));
            wp.flags = 0;
            wp.showCmd = wp.showCmd == Arc.WinUI.SW.SHOWMAXIMIZED ? Arc.WinUI.SW.SHOWMAXIMIZED : Arc.WinUI.SW.SHOWNORMAL;
            Arc.Internal.Methods.SetWindowPlacement(hwnd, ref wp);
        }
    }

    public static DipWindowPlacement SaveWindowPlacement(this Window window)
    {
        var hwnd = window.GetWindowHandle();
        Arc.Internal.Methods.GetWindowPlacement(hwnd, out var wp);
        Arc.Internal.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
        return new(wp, dpiX, dpiY);
    }

    /*public static void SetIconFromEmbeddedResource(this Window window, string resourceName, Assembly? assembly = default)
    {
        try
        {
            assembly ??= Assembly.GetEntryAssembly();
            if (assembly is null)
            {
                return;
            }

            var name = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase));
            if (name is null)
            {
                return;
            }

            var moduleHandle = Arc.Internal.Methods.GetModuleHandle(new IntPtr(0));
            var iconHandle = Arc.Internal.Methods.LoadImage(moduleHandle, "#32512", ImageType.Icon, 16, 16, 0); // ApplicationIcon
            var iconId = Microsoft.UI.Win32Interop.GetIconIdFromIcon(iconHandle);

            // var icon = new System.Drawing.Icon(assembly.GetManifestResourceStream(rName));
            window.AppWindow.SetIcon(iconId);
            //window.SetTaskBarIcon()
        }
        catch
        {
        }
    }*/

    public static void SetApplicationIcon(this Window window)
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly is null)
            {
                return;
            }

            var moduleHandle = Arc.Internal.Methods.GetModuleHandle(new IntPtr(0));
            var iconHandle = Arc.Internal.Methods.LoadImage(moduleHandle, "#32512", ImageType.Icon, 16, 16, 0); // ApplicationIcon
            var iconId = Microsoft.UI.Win32Interop.GetIconIdFromIcon(iconHandle);

            window.AppWindow.SetIcon(iconId);
        }
        catch
        {
        }
    }

    public static void RemoveIcon(this Window window)
    {
        // Get this window's handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Change the extended window style to not show a window icon
        var extendedStyle = Methods.GetWindowLong(hwnd, Methods.GWL_EXSTYLE);
        Methods.SetWindowLong(hwnd, Methods.GWL_EXSTYLE, extendedStyle | Methods.WS_EX_DLGMODALFRAME);

        // Update the window's non-client area to reflect the changes
        Methods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, Methods.SWP_NOMOVE | Methods.SWP_NOSIZE | Methods.SWP_NOZORDER | Methods.SWP_FRAMECHANGED);

        Methods.SendMessage(hwnd, Methods.WM_SETICON, new IntPtr(1), IntPtr.Zero);
        Methods.SendMessage(hwnd, Methods.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
    }
}
