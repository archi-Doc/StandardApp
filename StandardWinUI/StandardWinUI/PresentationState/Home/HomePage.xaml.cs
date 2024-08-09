// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StandardWinUI.States;

namespace StandardWinUI.Presentations;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        this.State = App.GetStateObject<HomeState>(this);
    }

    public HomeState State { get; }
}
