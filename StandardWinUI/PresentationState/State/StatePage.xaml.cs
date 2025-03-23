// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using StandardWinUI.State;

namespace StandardWinUI.Presentation;

public sealed partial class StatePage : Page
{
    public StatePageState State { get; }

    public StatePage()
    {
        this.InitializeComponent();
        // this.State = state; // To use a DI container, you need to hook into the Navigating event.
        this.State = this.GetAndPrepareState<StatePageState>();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {// OnNavigatedFrom
        // this.State.StoreState();
    }
}
