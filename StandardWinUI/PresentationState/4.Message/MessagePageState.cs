// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

/// <summary>
/// Runs the sample message-dialog interaction.
/// </summary>
public partial class MessagePageState : ObservableObject, IState
{
    private readonly IMessageDialogService messageDialogService;

    public MessagePageState(IMessageDialogService messageDialogService)
    {
        this.messageDialogService = messageDialogService;
    }

    [RelayCommand]
    private async Task ShowSampleDialog()
    {
        var r = await this.messageDialogService.ShowAsync(string.Empty, "Like or Love?", "Like", "Love");
        if (!r.TryGetFirst(out var result))
        {
            return;
        }

        if (result == ContentDialogResult.Primary)
        {
            await this.messageDialogService.ShowAsync(string.Empty, "Hikaru-chan...", string.Empty);
        }
        else
        {
            await this.messageDialogService.ShowAsync(string.Empty, "Ooh, Ayukawa.", string.Empty);
        }
    }
}
