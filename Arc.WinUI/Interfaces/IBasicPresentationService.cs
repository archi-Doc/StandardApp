// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

[RadioServiceInterface(MaxLinks = 1)]
public interface IBasicPresentationService : IRadioService
{
    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryCommand">The primary(default) command hash.</param>
    /// <param name="cancelCommand">The cancel command hash (0: No cancel button, 1: 'Cancel').</param>
    /// <param name="secondaryCommand">The secondary command hash.</param>
    /// <param name="cancellationToken">The cancellation hash.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    Task<RadioResult<ContentDialogResult>> MessageDialog(ulong title, ulong content, ulong primaryCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The content of the dialog.</param>
    /// <param name="primaryCommand">The primary(default) command text.</param>
    /// <param name="cancelCommand">The cancel command text.</param>
    /// <param name="secondaryCommand">The secondary command text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
    Task<RadioResult<ContentDialogResult>> MessageDialog(string title, string content, string primaryCommand, string? cancelCommand = default, string? secondaryCommand = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to exit the application.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the exit was successful.</returns>
    Task<RadioResult<bool>> TryExit(CancellationToken cancellationToken = default);
}
