// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

/// <summary>
/// A class for managing window scaling.<br/>
/// 1. Add &lt;Viewbox x ="transformer" Stretch="None"&gt; at the top level in Window.xaml.<br/>
/// 2. In the constructor of the Window, call Transformer.Register(this).<br/>
/// 3. Change Transformer.DisplayScaling and call Transformer.Refresh().
/// </summary>
public static class Transformer
{
    public static double ViewScale { get; set; } = 1.0d;

    private const string TransformerName = "transformer";
    private static Dictionary<IntPtr, Item> dictionary = new();

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

    public static void Register(Window window)
    {
        if (window.Content is FrameworkElement element)
        {
            var y = element.FindChild<Viewbox>();
            foreach (var x in element.FindChildren())
            {
                if (x is Viewbox viewbox &&
                    viewbox.Name == TransformerName)
                {
                    lock (dictionary)
                    {
                        var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        if (!dictionary.ContainsKey(handle))
                        {
                            var item = new Item(new(viewbox));
                            dictionary[handle] = item;
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

        lock (dictionary)
        {
            foreach (var x in dictionary)
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
                    dictionary.Remove(x);
                }
            }
        }
    }
}
