// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace Arc.WinUI;

public abstract class StateObject : ObservableObject
{
    public StateObject()
        : base()
    {
    }

    #region FieldAndProperty

    private object? presentationObject;

    #endregion

    public void InitializeState(object presentationObject)
    {
        this.presentationObject = presentationObject;
    }

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
            {// Since I couldn't retrieve the Window instance holding the UIElement, I'm resorting to a roundabout method. Can someone suggest a better way?
                return windowService;
            }
        }

        throw new InvalidOperationException($"'{this.presentationObject.GetType().Name}' and its parents do not implement '{typeof(TPresentationService).Name}'");

        /*var obj = this.presentationObject;
        while (true)
        {
            if (obj is TPresentationService service2)
            {
                return service2;
            }
            else if (obj is FrameworkElement frameworkElement)
            {
                obj = frameworkElement.Parent;
            }
            else
            {
                obj = null;
            }

            if (obj is null)
            {
                throw new InvalidOperationException($"'{this.presentationObject.GetType().Name}' and its parents do not implement '{typeof(TPresentationService).Name}'");
            }
        }*/
    }
}
