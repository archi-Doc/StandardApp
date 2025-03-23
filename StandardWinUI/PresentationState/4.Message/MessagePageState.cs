// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.State;

public partial class MessagePageState : ObservableObject, IState
{
    private readonly IBasicPresentationService simpleWindowService;

    public MessagePageState(IBasicPresentationService simpleWindowService)
    {
        this.simpleWindowService = simpleWindowService;
    }

    [RelayCommand]
    private async Task Test()
    {
        var r = await this.simpleWindowService.MessageDialog(string.Empty, "Like or Love?", "Like", "Love");
        if (!r.TryGetSingleResult(out var result))
        {
            return;
        }

        if (result == ContentDialogResult.Primary)
        {
            await this.simpleWindowService.MessageDialog(string.Empty, "Hikaru-chan...", string.Empty);
        }
        else
        {
            await this.simpleWindowService.MessageDialog(string.Empty, "Ooh, Ayukawa.", string.Empty);
        }
    }
}
