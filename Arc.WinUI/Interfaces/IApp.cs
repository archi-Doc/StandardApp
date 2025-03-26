// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

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
    /// Retrieves a service of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the service is not registered.</exception>
    public T GetService<T>()
        where T : class;

    /// <summary>
    /// Retrieves and prepares the state for the specified element.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="element">The framework element.</param>
    /// <returns>The state instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the state is not registered.</exception>
    public T GetAndPrepareState<T>(FrameworkElement element)
        where T : class, IState;

    /// <summary>
    /// Gets the application instance.
    /// </summary>
    /// <returns>The application instance.</returns>
    Application GetApplication();

    /// <summary>
    /// Gets the main window of the application.
    /// </summary>
    /// <returns>The main window.</returns>
    Window GetMainWindow();

    /// <summary>
    /// Exits the application.
    /// </summary>
    void Exit();

    /// <summary>
    /// Attempts to exit the application.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel the exit operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation.<br/>
    /// Returns <see langword="true"/> if the exit was successful, otherwise returns <see langword="false"/>.
    /// </returns>
    Task<bool> TryExit(CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the navigation event and retrieves the corresponding page from the service provider.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event data.</param>
    void NavigatingHandler(object sender, NavigatingCancelEventArgs args);
}
