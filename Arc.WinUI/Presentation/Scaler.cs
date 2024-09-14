// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

public static class Scaler
{
    public static double ViewScale { get; set; } = 1.0d;

    public static Style ButtonStyle
    {
        get
        {
            if (buttonStyle is null)
            {
                buttonStyle = new Style(typeof(Button));
                // buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Microsoft.UI.Colors.Green));
                buttonStyle.Setters.Add(new Setter(Button.CornerRadiusProperty, new CornerRadius(5)));
                buttonStyle.Setters.Add(new Setter(Button.FontSizeProperty, 16 * ViewScale));
            }

            return buttonStyle;
        }
    }

    private static object syncObject = new();
    private static LinkedList<Item> items = new();
    private static Style? buttonStyle;

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

                this.previousScale = ViewScale;
            }
        }
    }

    public static string ScaleToText(double scale) => $"{scale * 100:0}%";

    public static void Register(LayoutTransformControl layoutTransformControl)
    {
        lock (syncObject)
        {
            var item = new Item(new(layoutTransformControl));
            items.AddLast(item);
            layoutTransformControl.Loaded += item.LoadedEventHandler;
        }
    }

    public static void Refresh()
    {
        lock (syncObject)
        {
            buttonStyle = default;

            LinkedListNode<Item>? node, nextNode;
            node = items.First;
            while (node is not null)
            {
                nextNode = node.Next;

                if (node.Value.ControlReference.TryGetTarget(out var layoutTransform))
                {
                    node.Value.LoadedEventHandler(layoutTransform, default!);
                    layoutTransform.UpdateLayout();
                }
                else
                {
                    items.Remove(node);
                }

                node = nextNode;
            }
        }
    }

    /*
    /// <summary>
    /// Initializes the presentation for the specified window.
    /// </summary>
    /// <param name="window">The window to initialize the presentation for.</param>
    public static void InitializeWindow(this Window window)
    {
        // Register the viewbox
        if (window.Content is FrameworkElement element)
        {
            foreach (var x in element.FindChildren())
            {
                if (x is LayoutTransformControl layoutTransform &&
                    layoutTransform.Name == ScalerName)
                {
                    lock (syncObject)
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
    }*/
}
