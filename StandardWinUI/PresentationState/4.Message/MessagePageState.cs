// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

public partial class MessagePageState : ObservableObject, IState
{
    private readonly IMessageDialogService messageDialogService;

    public MessagePageState(IMessageDialogService messageDialogService)
    {
        this.messageDialogService = messageDialogService;
    }

    [RelayCommand]
    private async Task Test()
    {
        var r = await this.messageDialogService.Show(string.Empty, "Like or Love?", "Like", "Love");
        if (!r.TryGetSingleResult(out var result))
        {
            return;
        }

        if (result == ContentDialogResult.Primary)
        {
            await this.messageDialogService.Show(string.Empty, "Hikaru-chan...", string.Empty);
        }
        else
        {
            await this.messageDialogService.Show(string.Empty, "Ooh, Ayukawa.", string.Empty);
        }
    }
}
