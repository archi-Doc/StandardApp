// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

public static class Scaler
{
    private const string ViewScaleName = "scaler";

    public static double ViewScale { get; set; } = 1.0d;

    private static object syncViewItems = new();
    private static Dictionary<IntPtr, Item> viewItems = new();

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

    /// <summary>
    /// Initializes the presentation for the specified window.
    /// </summary>
    /// <param name="window">The window to initialize the presentation for.</param>
    public static void InitializeWindow(this Window window)
    {
        // Register the window
        /*lock (syncWindows)
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
        }*/

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

    public static void Refresh()
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
}
