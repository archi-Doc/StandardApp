// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StandardWinUI.State;

namespace StandardWinUI.Presentation;

public sealed partial class StatePage : Page
{
    public StatePageState State { get; }

    public StatePage(App app)
    {
        this.InitializeComponent();
        this.State = app.GetAndPrepareState<StatePageState>(this);
    }
}
