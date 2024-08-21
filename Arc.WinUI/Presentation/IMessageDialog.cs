// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using CrossChannel;

namespace Arc.WinUI;

[RadioServiceInterface(SingleLink = true)]
public interface IMessageDialog : IRadioService
{
    RadioResult<Task<ulong>> Show(ulong title, ulong content, ulong defaultCommand = 0, ulong cancelCommand = 0, ulong secondaryCommand = 0);
}
