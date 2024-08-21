// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrossChannel;
using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.States;

public partial class StatePageState : ObservableObject, IUnitSerializable
{
    private readonly ISimpleWindowService simpleWindowService;

    [ObservableProperty]
    private string sourceText = string.Empty;

    [ObservableProperty]
    private string destinationText = string.Empty;

    public StatePageState(ISimpleWindowService simpleWindowService, IChannel<IUnitSerializable> serializableChannel)
    {
        this.simpleWindowService = simpleWindowService;
        serializableChannel.Open(this, true);

        this.SourceText = App.Settings.Baibai.ToString();
    }

    [RelayCommand]
    private void Baibain()
    {
        if (int.TryParse(this.SourceText, out int value))
        {
            this.DestinationText = (value * 3).ToString();
            App.Settings.Baibai = value;
        }
    }

    [RelayCommand]
    private async Task Exit()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(3000);

        await this.simpleWindowService.Exit(false, cts.Token);
    }

    Task IUnitSerializable.LoadAsync(UnitMessage.LoadAsync message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    Task IUnitSerializable.SaveAsync(UnitMessage.SaveAsync message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
