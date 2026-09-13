// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Arc.WinUI;

/// <summary>
/// Provides a base implementation for the application.
/// </summary>
public abstract class AppBase : IApp
{
    /// <inheritdoc/>
    public DispatcherQueue UiDispatcherQueue { get; protected set; } = default!;

    /// <inheritdoc/>
    public string Version { get; protected set; } = string.Empty;

    /// <inheritdoc/>
    public string Title { get; protected set; } = string.Empty;

    /// <inheritdoc/>
    public string DataFolder { get; protected set; } = string.Empty;

    private readonly IServiceProvider serviceProvider;

    public AppBase(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public T GetService<T>()
        where T : class
    {
        if (this.serviceProvider.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in Configure within AppUnit.cs.");
        }

        return service;
    }

    public T GetAndPrepareState<T>(FrameworkElement element)
        where T : class, IState
    {
        if (this.serviceProvider.GetService(typeof(T)) is not T state)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in Configure within AppUnit.cs.");
        }

        element.Loaded += (sender, e) => state.RestoreState();
        element.Unloaded += (sender, e) => state.StoreState();

        return state;
    }

    public void NavigatingHandler(object sender, NavigatingCancelEventArgs args)
    {
        if (args.SourcePageType is not null)
        {
            var page = this.serviceProvider.GetService(args.SourcePageType);
            if (page is not null)
            {
                args.Cancel = true;
                ((Frame)sender).Content = page;
            }
        }
    }

    public abstract Application GetApplication();

    public abstract Window GetMainWindow();

    public void Exit()
        => this.GetApplication().Exit();

    public abstract Task<bool> TryExit(CancellationToken cancellationToken);
}
