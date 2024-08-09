// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

public static class Presentation
{
    private const string ViewScaleName = "viewscale";

    public static double ViewScale { get; set; } = 1.0d;

    private static object syncViewItems = new();
    private static Dictionary<IntPtr, Item> viewItems = new();

    private static object syncWindows = new();
    private static List<WeakReference<Window>> windows = new();

    private record Item(WeakReference<Viewbox> ViewboxReference)
    {
        private double previousScale = 1;

        public void LoadedEventHandler(object sender, RoutedEventArgs e)
        {
            if (sender is Viewbox viewbox &&
                this.previousScale != ViewScale)
            {
                var ratio = ViewScale / this.previousScale;

                viewbox.Stretch = Stretch.Uniform;
                viewbox.Width = viewbox.ActualWidth * ratio;
                viewbox.Height = viewbox.ActualHeight * ratio;

                this.previousScale = ViewScale;
            }
        }
    }

    public static void RefreshViewScale()
    {
        List<IntPtr>? toRemove = default;

        lock (syncViewItems)
        {
            foreach (var x in viewItems)
            {
                if (x.Value.ViewboxReference.TryGetTarget(out var viewbox))
                {
                    x.Value.LoadedEventHandler(viewbox, default!);
                    viewbox.UpdateLayout();
                }
                else
                {
                    toRemove ??= new();
                    toRemove.Add(x.Key);
                }
            }

            if (toRemove is not null)
            {
                foreach (var x in toRemove)
                {
                    viewItems.Remove(x);
                }
            }
        }
    }

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
        // Register the window
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

        // Register the viewbox
        if (window.Content is FrameworkElement element)
        {
            var y = element.FindChild<Viewbox>();
            foreach (var x in element.FindChildren())
            {
                if (x is Viewbox viewbox &&
                    viewbox.Name == ViewScaleName)
                {
                    lock (syncViewItems)
                    {
                        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        if (!viewItems.ContainsKey(handle))
                        {
                            var item = new Item(new(viewbox));
                            viewItems[handle] = item;
                            viewbox.Loaded += item.LoadedEventHandler;
                        }
                    }
                }
            }
        }
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
