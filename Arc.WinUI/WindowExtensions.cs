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
    private const string OkText = "OK";
    private const string CancelText = "Cancel";

    /// <summary>
    /// Activates the specified window.
    /// </summary>
    /// <param name="window">The window to activate.</param>
    /// <param name="force">If set to <c>true</c>, forces the window to activate.</param>
    public static void ActivateWindow(this Window window, bool force = false)
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
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="window">The window to show the dialog in.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryCommand">The primary(default) command hash.</param>
    /// <param name="cancelCommand">The cancel command hash (0: No cancel button, 1: 'Cancel').</param>
    /// <param name="secondaryCommand">The secondary command hash.</param>
    /// <param name="cancellationToken">The cancellation hash.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    public static Task<RadioResult<ContentDialogResult>> ShowMessageDialogAsync(this Window window, ulong title, ulong content, ulong primaryCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0, CancellationToken cancellationToken = default)
    {
        var titleText = title == 0 ? string.Empty : HashedString.Get(title);
        var contentText = content == 0 ? string.Empty : HashedString.Get(content);
        var primaryText = primaryCommand == 0 ? OkText : primaryCommand == 1 ? OkText : HashedString.GetOrAlternative(primaryCommand, OkText);
        var cancelText = cancelCommand == 0 ? default : cancelCommand == 1 ? CancelText : HashedString.GetOrAlternative(cancelCommand, CancelText);
        var secondaryText = secondaryCommand == 0 ? default : HashedString.Get(secondaryCommand);

        return ShowMessageDialogAsync(window, titleText, contentText, primaryText, cancelText, secondaryText, cancellationToken);
    }

    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="window">The window to show the dialog in.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryCommand">The primary(default) command text.</param>
    /// <param name="cancelCommand">The cancel command text (<see langword="null" />: No cancel button, "": 'Cancel').</param>
    /// <param name="secondaryCommand">The secondary command text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    public static async Task<RadioResult<ContentDialogResult>> ShowMessageDialogAsync(this Window window, string title, string content, string primaryCommand, string? cancelCommand = default, string? secondaryCommand = default, CancellationToken cancellationToken = default)
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

        if (!string.IsNullOrEmpty(primaryCommand))
        {
            dialog.PrimaryButtonText = primaryCommand;
        }
        else
        {
            dialog.PrimaryButtonText = OkText;
        }

        if (cancelCommand == string.Empty)
        {
            dialog.CloseButtonText = CancelText;
        }
        else if (cancelCommand is not null)
        {
            dialog.CloseButtonText = cancelCommand;
        }

        if (!string.IsNullOrEmpty(secondaryCommand))
        {
            dialog.SecondaryButtonText = secondaryCommand;
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
    /// Loads the window placement.
    /// </summary>
    /// <param name="window">The window to load the placement for.</param>
    /// <param name="windowPlacement">The window placement.</param>
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

    /// <summary>
    /// Saves the window placement.
    /// </summary>
    /// <param name="window">The window to save the placement for.</param>
    /// <returns>The saved window placement.</returns>
    public static DipWindowPlacement SaveWindowPlacement(this Window window)
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
