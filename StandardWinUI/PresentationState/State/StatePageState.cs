﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.State;

public partial class StatePageState : ObservableObject
{
    [ObservableProperty]
    public partial string SourceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DestinationText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool EnableButton { get; set; } = false;

    private readonly IBasicPresentationService simpleWindowService;

    public StatePageState(IBasicPresentationService simpleWindowService)
    {
        this.simpleWindowService = simpleWindowService;

        this.SourceText = App.Settings.Baibai.ToString();
    }

    public void StoreState()
    {
        if (int.TryParse(this.SourceText, out int v))
        {
            App.Settings.Baibai = v;
        }
    }

    [RelayCommand]
    private void Baibain()
    {
        App.ExecuteOrEnqueueOnUI((Microsoft.UI.Dispatching.DispatcherQueueHandler)(() =>
        {// Actually, App.ExecuteOnUI() is not necessary.
            if (int.TryParse((string)this.SourceText, out int value))
            {
                this.DestinationText = (value * 3).ToString();
            }

            this.EnableButton = !this.EnableButton;
        }));
    }

    [RelayCommand]
    private async Task Exit()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(3000);

        await this.simpleWindowService.TryExit(cts.Token);
    }
}
