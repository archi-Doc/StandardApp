// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.State;

public partial class MessagePageState : ObservableObject, IState
{
    private readonly IBasicPresentationService simpleWindowService;

    public MessagePageState(IBasicPresentationService simpleWindowService)
    {
        this.simpleWindowService = simpleWindowService;
    }

    [RelayCommand]
    private void Test()
    {
    }
}
