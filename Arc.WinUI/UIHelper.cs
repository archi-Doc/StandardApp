// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public static class UIHelper
{
    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="service">The message dialog service to show the dialog.</param>
    /// <param name="titleHash">The title hash (0: No title).</param>
    /// <param name="contentHash">The content hash (0: No content).</param>
    /// <param name="primaryButtonHash">The primary (default) button hash (0 or 1: 'OK').</param>
    /// <param name="cancelButtonHash">The cancel button hash (0: No cancel button, 1: 'Cancel').</param>
    /// <param name="secondaryButtonHash">The secondary button hash (0: No secondary button).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    public static Task<RadioResult<ContentDialogResult>> ShowMessageDialogAsync(this IMessageDialogService service, ulong titleHash, ulong contentHash, ulong primaryButtonHash = 0, ulong cancelButtonHash = 0, ulong secondaryButtonHash = 0, CancellationToken cancellationToken = default)
    {
        var titleText = titleHash == 0 ? string.Empty : HashedString.Get(titleHash);
        var contentText = contentHash == 0 ? string.Empty : HashedString.Get(contentHash);
        var primaryText = primaryButtonHash == 0 ? WindowExtensions.OkText : primaryButtonHash == 1 ? WindowExtensions.OkText : HashedString.GetOrAlternative(primaryButtonHash, WindowExtensions.OkText);
        var cancelText = cancelButtonHash == 0 ? default : cancelButtonHash == 1 ? WindowExtensions.CancelText : HashedString.GetOrAlternative(cancelButtonHash, WindowExtensions.CancelText);
        var secondaryText = secondaryButtonHash == 0 ? default : HashedString.Get(secondaryButtonHash);

        return service.ShowAsync(titleText, contentText, primaryText, cancelText, secondaryText, cancellationToken);
    }

    /// <summary>
    /// Open url with default browser.
    /// </summary>
    /// <param name="url">URL.</param>
    public static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
            }
        }
    }

    /// <summary>
    /// Checks whether another instance of the application is already running, and if so, activates its main window.
    /// </summary>
    /// <param name="mutex">The mutex used to detect another instance. It is closed if another instance is running.</param>
    /// <returns><see langword="true"/> if another instance is running (the caller should exit); otherwise, <see langword="false"/> (the mutex is acquired).</returns>
    public static bool TryActivateRunningInstance(Mutex mutex)
    {
        if (mutex.WaitOne(0, false))
        {
            return false;
        }

        mutex.Close(); // Release mutex.

        var prevProcess = Arc.Internal.WinAPI.GetPreviousProcess();
        if (prevProcess != null)
        {
            var handle = prevProcess.MainWindowHandle; // The window handle that associated with the previous process.
            // if (handle == IntPtr.Zero)
            // {
            //    handle = Arc.Internal.WinAPI.GetWindowHandle(prevProcess.Id, title); // Get handle.
            // }

            if (handle != IntPtr.Zero)
            {
                Arc.Internal.WinAPI.ActivateWindow(handle);
            }
        }

        return true;
    }
}
