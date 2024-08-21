// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CrossChannel;

namespace Arc.WinUI;

[RadioServiceInterface(SingleLink = true)]
public interface ISimpleWindowService : IRadioService
{
    Task<RadioResult<ulong>> MessageDialog(ulong title, ulong content, ulong defaultCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0, CancellationToken cancellationToken = default);

    Task<RadioResult<bool>> Exit(bool forceExit = false, CancellationToken cancellationToken = default);
}
