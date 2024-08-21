// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public static class Stringer
{
    private static object syncC4Object = new();
    private static LinkedList<C4Object> c4Objects = new LinkedList<C4Object>();
    private static GCCountChecker extensionObjectChecker = new GCCountChecker(16);

    private record class C4Object(WeakReference TargetObject, object? TargetProperty, string Key);

    public static void Register(object targetObject, object? targetProperty, string key)
    {
        lock (syncC4Object)
        {
            c4Objects.AddLast(new C4Object(new WeakReference(targetObject), targetProperty, key));
            if (extensionObjectChecker.Check())
            {
                Clean();
            }
        }
    }

    /// <summary>
    /// Updates the display of C4.<br/>
    /// Please call from the UI thread.<br/>
    /// If not on the UI thread, consider using App.TryEnqueueOnUI().
    /// </summary>
    public static void Refresh()
    {
        // GC.Collect();
        lock (syncC4Object)
        {
            foreach (var x in c4Objects)
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
                else if (target is C4BindingSource c4BindingSource)
                { // C4BindingSource
                    c4BindingSource.CultureChanged();
                }
            }

            // C4Clean();
        }
    }

    private static void Clean()
    {
        LinkedListNode<C4Object>? x, y;
        x = c4Objects.First;
        while (x != null)
        {
            y = x.Next;
            if (x.Value.TargetObject.Target == null)
            {
                /* if (x.Value.TargetProperty != null) gl.Trace("_C4Clean: removed (target object)");
                else gl.Trace("_C4Clean: removed (C4BindingSource)"); */
                c4Objects.Remove(x);
            }

            x = y;
        }
    }
}
