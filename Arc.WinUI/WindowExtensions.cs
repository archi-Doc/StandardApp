// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arc.Internal;
using CrossChannel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public static class WindowExtensions
{
    public const string OkText = "OK";
    public const string CancelText = "Cancel";

    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="window">The window to show the dialog in.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryButtonText">The primary (default) button text.</param>
    /// <param name="cancelButtonText">The cancel button text (<see langword="null" />: No cancel button, "": 'Cancel').</param>
    /// <param name="secondaryButtonText">The secondary button text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    public static async Task<RadioResult<ContentDialogResult>> ShowMessageDialogAsync(this Window window, string title, string content, string primaryButtonText, string? cancelButtonText = default, string? secondaryButtonText = default, CancellationToken cancellationToken = default)
    {
        var dialog = new ContentDialog() { XamlRoot = window.Content.XamlRoot };
        if (window.Content is FrameworkElement element)
        {
            dialog.RequestedTheme = element.RequestedTheme;
        }

        var textBlock = new TextBlock() { Text = content, TextWrapping = TextWrapping.Wrap, };
        textBlock.FontSize *= Scaler.ViewScale;
        dialog.Content = textBlock;

        dialog.PrimaryButtonStyle = Scaler.DialogButtonStyle;
        dialog.SecondaryButtonStyle = Scaler.DialogButtonStyle;
        dialog.CloseButtonStyle = Scaler.DialogButtonStyle;

        dialog.Title = title;

        if (!string.IsNullOrEmpty(primaryButtonText))
        {
            dialog.PrimaryButtonText = primaryButtonText;
        }
        else
        {
            dialog.PrimaryButtonText = OkText;
        }

        if (cancelButtonText == string.Empty)
        {
            dialog.CloseButtonText = CancelText;
        }
        else if (cancelButtonText is not null)
        {
            dialog.CloseButtonText = cancelButtonText;
        }

        if (!string.IsNullOrEmpty(secondaryButtonText))
        {
            dialog.SecondaryButtonText = secondaryButtonText;
        }

        var dialogTask = dialog.ShowAsync(ContentDialogPlacement.InPlace);
        WinAPI.SetForegroundWindow(WinRT.Interop.WindowNative.GetWindowHandle(window));

        ContentDialogResult result;
        try
        {
            result = await dialogTask.AsTask().WaitAsync(cancellationToken);
        }
        catch
        {
            dialogTask.Cancel();
            result = ContentDialogResult.None;
        }

        return new(result);
    }

    /// <summary>
    /// Brings the specified window into the foreground and activates it.
    /// </summary>
    /// <param name="window">The window to activate.</param>
    /// <param name="force">If set to <c>true</c>, forces the window to activate.</param>
    public static void BringToForeground(this Window window, bool force = false)
    {
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        if (force)
        {
            WinAPI.ActivateWindowForce(handle);
        }
        else
        {
            WinAPI.ActivateWindow(handle);
        }
    }

    /// <summary>
    /// Applies the window placement to the window.
    /// </summary>
    /// <param name="window">The window to apply the placement to.</param>
    /// <param name="windowPlacement">The window placement.</param>
    public static void ApplyWindowPlacement(this Window window, DipWindowPlacement windowPlacement)
    {
        if (windowPlacement.IsValid)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            Arc.Internal.WinAPI.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            var wp = windowPlacement.ToWindowPlacementWithPhysicalPosition(dpiX, dpiY);
            wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Arc.WinUI.NativeWindowPlacement));
            wp.flags = 0;
            wp.showCmd = wp.showCmd == Arc.WinUI.ShowCommand.SHOWMAXIMIZED ? Arc.WinUI.ShowCommand.SHOWMAXIMIZED : Arc.WinUI.ShowCommand.SHOWNORMAL;
            Arc.Internal.WinAPI.SetWindowPlacement(hwnd, ref wp);
        }
    }

    /// <summary>
    /// Gets the current window placement of the window.
    /// </summary>
    /// <param name="window">The window to get the placement for.</param>
    /// <returns>The current window placement.</returns>
    public static DipWindowPlacement GetWindowPlacement(this Window window)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        Arc.Internal.WinAPI.GetWindowPlacement(hwnd, out var wp);
        Arc.Internal.WinAPI.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
        return new(wp, dpiX, dpiY);
    }

    /// <summary>
    /// Sets the application icon for the window.
    /// </summary>
    /// <param name="window">The window to set the icon for.</param>
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

    /// <summary>
    /// Removes the icon from the window.
    /// </summary>
    /// <param name="window">The window to remove the icon from.</param>
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
