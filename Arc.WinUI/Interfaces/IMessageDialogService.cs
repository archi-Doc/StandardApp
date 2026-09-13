// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

/// <summary>
/// Provides asynchronous message dialogs through a radio channel.
/// </summary>
[RadioService(MaxLinks = 1)]
public interface IMessageDialogService : IRadioService
{
    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryButtonText">The primary (default) button text.</param>
    /// <param name="cancelButtonText">The cancel button text (<see langword="null" />: No cancel button, "": 'Cancel').</param>
    /// <param name="secondaryButtonText">The secondary button text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    Task<RadioResult<ContentDialogResult>> ShowAsync(string title, string content, string primaryButtonText, string? cancelButtonText = default, string? secondaryButtonText = default, CancellationToken cancellationToken = default);
}
