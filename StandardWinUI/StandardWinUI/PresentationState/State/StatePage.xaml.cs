// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StandardWinUI.States;

namespace StandardWinUI.Presentations;

public sealed partial class StatePage : Page
{
    public StatePage()
    {
        this.InitializeComponent();
        this.State = App.GetStateObject<StatePageState>(this);
    }

    public StatePageState State { get; }
}
