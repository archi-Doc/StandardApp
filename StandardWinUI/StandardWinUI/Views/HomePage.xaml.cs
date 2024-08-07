// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StandardWinUI.ViewModels;

namespace StandardWinUI.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        this.ViewModel = App.GetService<HomeViewModel>();
    }

    public HomeViewModel ViewModel { get; }
}
