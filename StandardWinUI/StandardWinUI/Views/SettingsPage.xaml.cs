// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StandardWinUI.ViewModels;

namespace StandardWinUI.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.viewModel = App.GetService<SettingsViewModel>();
    }

    private readonly SettingsViewModel viewModel;
}
