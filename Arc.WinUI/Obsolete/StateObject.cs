// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace Arc.WinUI;

/*
/// <summary>
/// Represents a base class for state objects.
/// </summary>
public abstract class StateObject : ObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StateObject"/> class.
    /// </summary>
    public StateObject()
        : base()
    {
    }

    #region FieldAndProperty

    private object? presentationObject;

    #endregion

    /// <summary>
    /// Initializes the state object with the specified presentation object.
    /// </summary>
    /// <param name="presentationObject">The presentation object.</param>
    public void InitializeState(object presentationObject)
    {
        this.presentationObject = presentationObject;
    }

    /// <summary>
    /// Gets the presentation service of the specified type.
    /// </summary>
    /// <typeparam name="TPresentationService">The type of the presentation service.</typeparam>
    /// <returns>The presentation service of the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the presentation object or its parents do not implement the specified presentation service.</exception>
    public TPresentationService GetPresentationService<TPresentationService>()
        where TPresentationService : IPresentationService
    {
        if (this.presentationObject is null)
        {
            throw new InvalidOperationException("Presentation object is not set.");
        }

        if (this.presentationObject is TPresentationService service)
        {
            return service;
        }
        else if (this.presentationObject is UIElement element)
        {
            if (element.TryGetWindow(out var window) &&
                window is TPresentationService windowService)
            {// Since we couldn't retrieve the Window instance holding the UIElement, we're using a workaround. Does anyone have a better solution?
                return windowService;
            }
        }

        throw new InvalidOperationException($"'{this.presentationObject.GetType().Name}' and its parents do not implement '{typeof(TPresentationService).Name}'");
    }
}*/
