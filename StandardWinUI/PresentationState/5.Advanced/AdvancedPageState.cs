// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.PresentationState;

public partial class AdvancedPageState : ObservableObject, IState
{
    [ObservableProperty]
    public partial string SourceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DestinationText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExitCommand))]
    public partial bool CanExit { get; set; } = true;

    private readonly IApp app;
    private readonly AppSettings settings;
    private readonly IMessageDialogService messageDialogService;

    public AdvancedPageState(IApp app, AppSettings settings, IMessageDialogService messageDialogService)
    {
        this.app = app;
        this.settings = settings;
        this.messageDialogService = messageDialogService;
    }

    /// <summary>
    /// Restores the state (load persisted data and reflect it in the state).<br/>
    /// This method is added to the Loaded event of the FrameworkElement when IApp.GetAndPrepareState() is called.
    /// </summary>
    void IState.RestoreState()
    {
        this.SourceText = this.settings.BaibainNumber.ToString();
    }

    /// <summary>
    /// Stores the current state (persist the state or convert it into data for persistence).<br/>
    /// This method is added to the Unloaded event of the FrameworkElement when IApp.GetAndPrepareState() is called.
    /// </summary>
    void IState.StoreState()
    {
        if (int.TryParse(this.SourceText, out int v))
        {
            this.settings.BaibainNumber = v;
        }
    }

    [RelayCommand]
    private void Multiply()
    { // this.app.UIDispatcherQueue.TryEnqueue(() => { });
        if (int.TryParse((string)this.SourceText, out int value))
        {
            this.DestinationText = (value * 3).ToString();
        }

        this.CanExit = !this.CanExit;
    }

    [RelayCommand(CanExecute = nameof(CanExit))]
    private async Task Exit()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(2000);

        await this.app.TryExitAsync(cts.Token);
    }
}
