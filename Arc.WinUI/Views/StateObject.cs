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

    public void Initialize(object presentationObject)
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

        var obj = this.presentationObject;
        while (true)
        {
            if (obj is TPresentationService service)
            {
                return service;
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
        }
    }

    private object? presentationObject;
}
