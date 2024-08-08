// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

public class MenuFlyoutEx : MenuFlyout
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(default, OnItemsSourcePropertyChanged));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(MenuFlyoutEx),
            new PropertyMetadata(default));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }

    private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MenuFlyoutEx menuFlyoutEx ||
            e.NewValue is not IEnumerable itemsSource)
        {
            return;
        }

        menuFlyoutEx.Items.Clear();

        foreach (var item in itemsSource)
        {
            MenuFlyoutItem menuFlyoutItem = new()
            {
                Text = item.ToString(),
                Command = menuFlyoutEx.Command,
                CommandParameter = item,
            };

            menuFlyoutEx.Items.Add(menuFlyoutItem);
        }
    }
}
