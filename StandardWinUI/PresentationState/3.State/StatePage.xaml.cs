// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

/// <summary>
/// Binds the arithmetic sample page to its state object.
/// </summary>
public sealed partial class StatePage : Page
{
    public StatePageState State { get; }

    public StatePage(IApp app)
    {
        this.InitializeComponent();
        this.State = app.GetAndPrepareState<StatePageState>(this);
    }
}
