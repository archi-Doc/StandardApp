// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Net.Sockets;
using System.Threading.Tasks;
using Arc.Views;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using StandardWinUI.ViewModels;
using WinUIEx;

namespace StandardWinUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : WinUIEx.WindowEx
{
    internal MainViewModel ViewModel { get; }

    public MainWindow()
    {
        this.ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
    }

    private async void myButton_Click(object sender, RoutedEventArgs e)
    {
        /*var children = ((FrameworkElement)this.Content).FindChildren();
        foreach (var x in children)
        {
            // x.ActualHeight
        }
        var content = this.Content;
        var count = VisualTreeHelper.GetChildrenCount(content);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(content, i);
        }*/
        // await this.ShowMessageDialogAsync("test", "test2");
        this.IsMaximizable = false;
        this.myButton.Content = "Clicked";
        // this.Content.Scale = new(2);

        // var grid = (Grid)this.Content;
        // grid.Scale = new(2);

        if (this.Content is FrameworkElement element)
        {
            if (element.FindChild<Viewbox>() is { } viewbox)
            {
                viewbox.Stretch = Stretch.Uniform;
                viewbox.Width = viewbox.ActualWidth * 1.5;
                viewbox.Height = viewbox.ActualHeight * 1.5;
            }
        }
    }
}
