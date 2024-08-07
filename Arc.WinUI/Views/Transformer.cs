﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

public class Transformer
{
    public static double DisplayScaling { get; set; } = 1.0d;

    private const string TransformerName = "transformer";
    private static Dictionary<IntPtr, Item> dictionary = new();

    private record Item(WeakReference ViewboxReference)
    {
        private double previousScale = 1;

        public void LoadedEventHandler(object sender, RoutedEventArgs e)
        {
            if (sender is Viewbox viewbox &&
                this.previousScale != DisplayScaling)
            {
                var ratio = DisplayScaling / this.previousScale;

                viewbox.Stretch = Stretch.Uniform;
                viewbox.Width = viewbox.ActualWidth * ratio;
                viewbox.Height = viewbox.ActualHeight * ratio;

                this.previousScale = DisplayScaling;
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
                if (x.Value.ViewboxReference.Target is Viewbox viewbox)
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
