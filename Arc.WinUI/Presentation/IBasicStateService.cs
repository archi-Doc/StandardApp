// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CrossChannel;

namespace Arc.WinUI;

[RadioServiceInterface]
public interface IBasicStateService : IRadioService
{
    void FlushState();
}
