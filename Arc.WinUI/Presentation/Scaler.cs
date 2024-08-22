// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

public static class Scaler
{
    private const string ScalerName = "scaler";

    public static double ViewScale { get; set; } = 1.0d;

    private static object syncScalerItems = new();
    private static Dictionary<IntPtr, Item> scalerItems = new();

    private record Item(WeakReference<LayoutTransformControl> ControlReference)
    {
        private double previousScale = 1;

        public void LoadedEventHandler(object sender, RoutedEventArgs e)
        {
            if (sender is LayoutTransformControl layoutTransform &&
                this.previousScale != ViewScale)
            {
                layoutTransform.Transform = new ScaleTransform
                {
                    ScaleX = ViewScale,
                    ScaleY = ViewScale,
                };

                /*var ratio = ViewScale / this.previousScale;
                viewbox.Stretch = Stretch.Uniform;
                viewbox.Width = viewbox.ActualWidth * ratio;
                viewbox.Height = viewbox.ActualHeight * ratio;*/

                this.previousScale = ViewScale;
            }
        }
    }

    public static string ScaleToText(double scale) => $"{scale * 100:0}%";

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
            // var y = element.FindChild<LayoutTransformControl>();
            foreach (var x in element.FindChildren())
            {
                if (x is LayoutTransformControl layoutTransform &&
                    layoutTransform.Name == ScalerName)
                {
                    lock (syncScalerItems)
                    {
                        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        if (!scalerItems.ContainsKey(handle))
                        {
                            var item = new Item(new(layoutTransform));
                            scalerItems[handle] = item;
                            layoutTransform.Loaded += item.LoadedEventHandler;
                        }
                    }
                }
            }
        }
    }

    public static void Refresh()
    {
        List<IntPtr>? toRemove = default;

        lock (syncScalerItems)
        {
            foreach (var x in scalerItems)
            {
                if (x.Value.ControlReference.TryGetTarget(out var layoutTransform))
                {
                    x.Value.LoadedEventHandler(layoutTransform, default!);
                    layoutTransform.UpdateLayout();
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
                    scalerItems.Remove(x);
                }
            }
        }
    }
}
