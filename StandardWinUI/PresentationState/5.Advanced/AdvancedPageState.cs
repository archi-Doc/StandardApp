// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.State;

public partial class AdvancedPageState : ObservableObject, IState
{
    [ObservableProperty]
    public partial string SourceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DestinationText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool EnableButton { get; set; } = false;

    private readonly IBasicPresentationService simpleWindowService;

    public AdvancedPageState(IBasicPresentationService simpleWindowService)
    {
        this.simpleWindowService = simpleWindowService;
    }

    /// <summary>
    /// Restores the state (load persisted data and reflect it in the state).<br/>
    /// This method is added to the Loaded event of the FrameworkElement when App.GetAndPrepareState() is called.
    /// </summary>
    void IState.RestoreState()
    {
        this.SourceText = App.Settings.Baibai.ToString();
    }

    /// <summary>
    /// Stores the current state (persist the state or convert it into data for persistence).<br/>
    /// This method is added to the Unloaded event of the FrameworkElement when App.GetAndPrepareState() is called.
    /// </summary>
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

    [RelayCommand]
    private async Task Exit()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(3000);

        await this.simpleWindowService.TryExit(cts.Token);
    }
}
