// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Arc.WinUI;

/// <summary>
/// Represents the application interface.
/// </summary>
public interface IApp
{
    /// <summary>
    /// Gets the UI dispatcher queue.
    /// </summary>
    DispatcherQueue UiDispatcherQueue { get; }

    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the title of the application.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the folder path for application data.
    /// </summary>
    string DataFolder { get; }

    /// <summary>
    /// Gets the main window of the application.
    /// </summary>
    /// <returns>The main window.</returns>
    Window GetMainWindow();

    /// <summary>
    /// Exits the application.
    /// </summary>
    void Exit();

    Task TryExit(CancellationToken cancellationToken = default);
}
