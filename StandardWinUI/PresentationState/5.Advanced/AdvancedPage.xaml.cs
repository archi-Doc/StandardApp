// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

/// <summary>
/// Binds the advanced sample page to its persistent state object.
/// </summary>
public sealed partial class AdvancedPage : Page
{
    public AdvancedPageState State { get; }

    public AdvancedPage(IApp app)
    {
        this.InitializeComponent();
        // this.State = state; // To use a DI container, you need to hook into the Navigating event.
        this.State = app.GetAndPrepareState<AdvancedPageState>(this);
    }
}
