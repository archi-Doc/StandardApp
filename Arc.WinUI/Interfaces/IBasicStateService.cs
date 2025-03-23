// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace Arc.WinUI;

/// <summary>
/// Defines the interface of a state object.
/// </summary>
public interface IState
{
    /// <summary>
    /// Restores the state (load persisted data and reflect it in the state.).
    /// </summary>
    void RestoreState();

    /// <summary>
    /// Stores the current state (persist the state or convert it into data for persistence).
    /// </summary>
    void StoreState();
}
