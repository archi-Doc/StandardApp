// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml;
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
        this.myButton.Content = "Clicked";
    }
}
