// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Net.Sockets;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using StandardWinUI.ViewModels;

namespace StandardWinUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    internal MainViewModel ViewModel { get; }

    public MainWindow()
    {
        this.ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        var children = ((FrameworkElement)this.Content).FindChildren();
        foreach (var x in children)
        {
            // x.ActualHeight
        }
        var content = this.Content;
        var count = VisualTreeHelper.GetChildrenCount(content);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(content, i);
        }
        this.myButton.Content = "Clicked";
    }
}
