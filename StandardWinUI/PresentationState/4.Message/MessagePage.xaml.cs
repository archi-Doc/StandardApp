// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

/// <summary>
/// Binds the dialog sample page to its state object.
/// </summary>
public sealed partial class MessagePage : Page
{
    public MessagePageState State { get; }

    public MessagePage(IApp app)
    {
        this.InitializeComponent();
        this.State = app.GetAndPrepareState<MessagePageState>(this);
    }
}
