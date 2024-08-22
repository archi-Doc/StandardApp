// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StandardWinUI.State;

namespace StandardWinUI.Presentation;

public sealed partial class StatePage : Page
{
    public StatePageState State { get; }

    public StatePage()
    {
        this.InitializeComponent();
        this.State = App.GetService<StatePageState>();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        this.State.StoreState();
    }
}
