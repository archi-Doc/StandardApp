// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CrossChannel;
using Microsoft.UI.Xaml.Controls;

namespace Arc.WinUI;

[RadioServiceInterface(MaxLinks = 1)]
public interface IBasicPresentationService : IRadioService
{
    Task<RadioResult<ulong>> MessageDialog(ulong title, ulong content, ulong defaultCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0, CancellationToken cancellationToken = default);

    Task<RadioResult<ContentDialogResult>> MessageDialog(string title, string content, string defaultCommand, string? cancelCommand = default, string? secondaryCommand = default, CancellationToken cancellationToken = default);

    Task<RadioResult<bool>> TryExit(CancellationToken cancellationToken = default);
}
