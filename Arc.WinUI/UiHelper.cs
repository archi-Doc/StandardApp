// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public static class UiHelper
{
    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="service">The presentation service to show the dialog.</param>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryCommand">The primary(default) command hash.</param>
    /// <param name="cancelCommand">The cancel command hash (0: No cancel button, 1: 'Cancel').</param>
    /// <param name="secondaryCommand">The secondary command hash.</param>
    /// <param name="cancellationToken">The cancellation hash.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    public static Task<RadioResult<ContentDialogResult>> ShowMessageDialogAsync(this IMessageDialogService service, ulong title, ulong content, ulong primaryCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0, CancellationToken cancellationToken = default)
    {
        var titleText = title == 0 ? string.Empty : HashedString.Get(title);
        var contentText = content == 0 ? string.Empty : HashedString.Get(content);
        var primaryText = primaryCommand == 0 ? WindowExtensions.OkText : primaryCommand == 1 ? WindowExtensions.OkText : HashedString.GetOrAlternative(primaryCommand, WindowExtensions.OkText);
        var cancelText = cancelCommand == 0 ? default : cancelCommand == 1 ? WindowExtensions.CancelText : HashedString.GetOrAlternative(cancelCommand, WindowExtensions.CancelText);
        var secondaryText = secondaryCommand == 0 ? default : HashedString.Get(secondaryCommand);

        return service.Show(titleText, contentText, primaryText, cancelText, secondaryText, cancellationToken);
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

    public static bool PreventMultipleInstances(Mutex mutex)
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
