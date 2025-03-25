// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.PresentationState;

public partial class StatePageState : ObservableObject, IState
{
    [ObservableProperty]
    public partial string SourceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DestinationText { get; set; } = string.Empty;

    public StatePageState()
    {
    }

    [RelayCommand]
    private void Baibain()
    {
        if (int.TryParse((string)this.SourceText, out int value))
        {
            this.DestinationText = (value * 3).ToString();
        }
    }
}
