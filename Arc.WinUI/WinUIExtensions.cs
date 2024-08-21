// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Arc.Internal;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public static class WinUIExtensions
{
    private const string OkString = "OK";
    private const string CancelString = "Cancel";

    public static async Task<ulong> ShowMessageDialogAsync(this Window window, ulong title, ulong content, ulong defaultCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0)
    {
        var dialog = new ContentDialog() { XamlRoot = window.Content.XamlRoot };
        if (window.Content is FrameworkElement element)
        {
            dialog.RequestedTheme = element.RequestedTheme;
        }

        var textBlock = new TextBlock() { Text = HashedString.Get(content), TextWrapping = TextWrapping.Wrap, };
        textBlock.FontSize *= Scaler.ViewScale;
        dialog.Content = textBlock;
        if (title != 0)
        {
            dialog.Title = HashedString.Get(title);
        }

        if (defaultCommand != 0)
        {
            dialog.PrimaryButtonText = HashedString.GetOrAlternative(defaultCommand, OkString);
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
            dialog.CloseButtonText = HashedString.GetOrAlternative(cancelCommand, CancelString);
        }

        var dialogTask = dialog.ShowAsync(ContentDialogPlacement.InPlace);
        WinAPI.SetForegroundWindow(WinRT.Interop.WindowNative.GetWindowHandle(window));
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
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            Arc.Internal.WinAPI.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            var wp = windowPlacement.ToWINDOWPLACEMENT2(dpiX, dpiY);
            wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Arc.WinUI.WINDOWPLACEMENT));
            wp.flags = 0;
            wp.showCmd = wp.showCmd == Arc.WinUI.ShowCommand.SHOWMAXIMIZED ? Arc.WinUI.ShowCommand.SHOWMAXIMIZED : Arc.WinUI.ShowCommand.SHOWNORMAL;
            Arc.Internal.WinAPI.SetWindowPlacement(hwnd, ref wp);
        }
    }

    public static DipWindowPlacement SaveWindowPlacement(this Window window)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        Arc.Internal.WinAPI.GetWindowPlacement(hwnd, out var wp);
        Arc.Internal.WinAPI.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
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

            var moduleHandle = Arc.Internal.WinAPI.GetModuleHandle(new IntPtr(0));
            var iconHandle = Arc.Internal.WinAPI.LoadImage(moduleHandle, "#32512", WinAPI.ImageType.Icon, 16, 16, 0); // ApplicationIcon
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
        var extendedStyle = WinAPI.GetWindowLong(hwnd, WinAPI.GWL_EXSTYLE);
        WinAPI.SetWindowLong(hwnd, WinAPI.GWL_EXSTYLE, extendedStyle | WinAPI.WS_EX_DLGMODALFRAME);

        // Update the window's non-client area to reflect the changes
        WinAPI.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_NOZORDER | WinAPI.SWP_FRAMECHANGED);

        WinAPI.SendMessage(hwnd, WinAPI.WM_SETICON, new IntPtr(1), IntPtr.Zero);
        WinAPI.SendMessage(hwnd, WinAPI.WM_SETICON, IntPtr.Zero, IntPtr.Zero);
    }
}
