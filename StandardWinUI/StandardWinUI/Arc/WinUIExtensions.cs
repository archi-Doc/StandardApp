// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using Arc.WinAPI;
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
            Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            var wp = windowPlacement.ToWINDOWPLACEMENT2(dpiX, dpiY);
            wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Arc.WinAPI.WINDOWPLACEMENT));
            wp.flags = 0;
            wp.showCmd = wp.showCmd == Arc.WinAPI.SW.SHOWMAXIMIZED ? Arc.WinAPI.SW.SHOWMAXIMIZED : Arc.WinAPI.SW.SHOWNORMAL;
            Arc.WinAPI.Methods.SetWindowPlacement(hwnd, ref wp);
        }
    }

    public static DipWindowPlacement SaveWindowPlacement(this Window window)
    {
        var hwnd = window.GetWindowHandle();
        Arc.WinAPI.Methods.GetWindowPlacement(hwnd, out var wp);
        Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
        return new(wp, dpiX, dpiY);
    }
}
