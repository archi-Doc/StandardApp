// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Arc.WinUI;

public static class Helper
{
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

    public static bool PreventMultipleInstances(Mutex mutex, string title)
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
            if (handle == IntPtr.Zero)
            {
                handle = Arc.Internal.WinAPI.GetWindowHandle(prevProcess.Id, title); // Get handle.
            }

            if (handle != IntPtr.Zero)
            {
                Arc.Internal.WinAPI.ActivateWindow(handle);
            }
        }

        return true;
    }
}
