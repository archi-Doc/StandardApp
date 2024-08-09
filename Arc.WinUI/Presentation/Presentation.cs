// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml;

namespace Arc.WinUI;

public static class Presentation
{
    private static object syncWindows = new();
    private static List<WeakReference<Window>> windows = new();

    /// <summary>
    /// Tries to get the <see cref="Window"/> associated with the specified <see cref="UIElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="UIElement"/> to get the associated <see cref="Window"/> for.</param>
    /// <param name="window">When this method returns, contains the associated <see cref="Window"/>, if found; otherwise, the default value.</param>
    /// <returns><c>true</c> if the associated <see cref="Window"/> is found; otherwise, <c>false</c>.</returns>
    public static bool TryGetWindow(this UIElement element, [MaybeNullWhen(false)] out Window window)
    {
        var content = element.XamlRoot.Content;
        lock (syncWindows)
        {
            for (var i = 0; i < windows.Count; i++)
            {
                var item = windows[i];
                if (item.TryGetTarget(out var target))
                {
                    if (target.Content == content)
                    {
                        window = target;
                        return true;
                    }
                }
                else
                {
                    windows.RemoveAt(i);
                }
            }
        }

        window = default;
        return false;
    }

    /// <summary>
    /// Initializes the presentation for the specified window.
    /// </summary>
    /// <param name="window">The window to initialize the presentation for.</param>
    public static void InitializeWindow(this Window window)
    {
        lock (syncWindows)
        {
            for (var i = 0; i < windows.Count; i++)
            {
                var item = windows[i];
                if (item.TryGetTarget(out var target))
                {
                    if (target == window)
                    {// Found
                        return;
                    }
                }
                else
                {
                    windows.RemoveAt(i);
                }
            }

            // Not found
            windows.Add(new WeakReference<Window>(window));
        }

        Transformer.Register(window);
    }

    private static int FindWindowInternal(Window window)
    {
        for (var i = 0; i < windows.Count; i++)
        {
            var item = windows[i];
            if (item.TryGetTarget(out var target))
            {
                if (target == window)
                {
                    return i;
                }
            }
            else
            {
                windows.RemoveAt(i);
            }
        }

        return -1;
    }
}
