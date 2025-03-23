// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.State;

public partial class StatePageState : ObservableObject, IState
{
    [ObservableProperty]
    public partial string SourceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DestinationText { get; set; } = string.Empty;

    public StatePageState()
    {
    }

    /// <summary>
    /// Restores the state (load persisted data and reflect it in the state).<br/>
    /// This method is added to the Loaded event of the FrameworkElement when <see cref="App.GetAndPrepareState{T}(Microsoft.UI.Xaml.FrameworkElement)"/> is called.
    /// </summary>
    void IState.RestoreState()
    {
        this.SourceText = App.Settings.Baibai.ToString();
    }

    /// <summary>
    /// Stores the current state (persist the state or convert it into data for persistence).<br/>
    /// This method is added to the Unloaded event of the FrameworkElement when <see cref="App.GetAndPrepareState{T}(Microsoft.UI.Xaml.FrameworkElement)"/> is called.
    void IState.StoreState()
    {
        if (int.TryParse(this.SourceText, out int v))
        {
            App.Settings.Baibai = v;
        }
    }

    [RelayCommand]
    private void Baibain()
    { // App.ExecuteOrEnqueueOnUI((Microsoft.UI.Dispatching.DispatcherQueueHandler)(() => { }));
        if (int.TryParse((string)this.SourceText, out int value))
        {
            this.DestinationText = (value * 3).ToString();
        }
    }
}
