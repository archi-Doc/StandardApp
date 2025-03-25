// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StandardWinUI.PresentationState;

namespace StandardWinUI.PresentationState;

public sealed partial class MessagePage : Page
{
    public MessagePageState State { get; }

    public MessagePage(App app)
    {
        this.InitializeComponent();
        this.State = app.GetAndPrepareState<MessagePageState>(this);
    }
}
