﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public static class Stringer
{
    private static object syncStringerObject = new();
    private static LinkedList<StringerObject> stringerObjects = new LinkedList<StringerObject>();
    private static GCCountChecker extensionObjectChecker = new GCCountChecker(16);

    private record class StringerObject(WeakReference TargetObject, object? TargetProperty, string Key);

    public static void Register(object targetObject, object? targetProperty, string key)
    {
        lock (syncStringerObject)
        {
            stringerObjects.AddLast(new StringerObject(new WeakReference(targetObject), targetProperty, key));
            if (extensionObjectChecker.Check())
            {
                Clean();
            }
        }
    }

    /// <summary>
    /// Updates the display of Stringer.<br/>
    /// Please call from the UI thread.<br/>
    /// If not on the UI thread, consider using App.TryEnqueueOnUI().
    /// </summary>
    public static void Refresh()
    {
        // GC.Collect();
        lock (syncStringerObject)
        {
            foreach (var x in stringerObjects)
            {
                var target = x.TargetObject?.Target;
                /*if (target is DependencyObject dependencyObject)
                {
                    if (x.TargetProperty is DependencyProperty dependencyProperty)
                    {
                        dependencyObject.SetValue(dependencyProperty, HashedString.GetOrIdentifier(x.Key));
                    }
                    else if (x.TargetProperty is ProvideValueTargetProperty provideValueTargetProperty)
                    {// .....
                    }
                }*/

                if (target is TextBlock textBlock)
                {// TextBlock
                    textBlock.Text = HashedString.GetOrIdentifier(x.Key);
                }
                else if (target is MenuFlyoutItem menuFlyoutItem)
                {
                    menuFlyoutItem.Text = HashedString.GetOrIdentifier(x.Key);
                }
                else if (target is NavigationViewItem navigationViewItem)
                {
                    navigationViewItem.Content = HashedString.GetOrIdentifier(x.Key);
                }
                else if (target is StringerBindingSource stringerBindingSource)
                { // StringerBindingSource
                    stringerBindingSource.CultureChanged();
                }
            }

            // StringerClean();
        }
    }

    private static void Clean()
    {
        LinkedListNode<StringerObject>? x, y;
        x = stringerObjects.First;
        while (x != null)
        {
            y = x.Next;
            if (x.Value.TargetObject.Target == null)
            {
                /* if (x.Value.TargetProperty != null) gl.Trace("_StringerClean: removed (target object)");
                else gl.Trace("_StringerClean: removed (StringerBindingSource)"); */
                stringerObjects.Remove(x);
            }

            x = y;
        }
    }
}