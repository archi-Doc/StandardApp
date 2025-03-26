// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

public sealed partial class MessagePage : Page
{
    public MessagePageState State { get; }

    public MessagePage(IApp app)
    {
        this.InitializeComponent();
        this.State = app.GetAndPrepareState<MessagePageState>(this);
    }
}
