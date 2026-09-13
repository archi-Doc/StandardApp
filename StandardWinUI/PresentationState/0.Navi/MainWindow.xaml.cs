// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using CrossChannel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace StandardWinUI.PresentationState;

/// <summary>
/// Hosts sample navigation, message dialogs, and persistent window placement.
/// </summary>
public partial class MainWindow : Window, IMessageDialogService
{
    private readonly IApp app;
    private readonly AppSettings settings;

    public MainWindow(IApp app, AppSettings settings, IChannel<IMessageDialogService> messageDialogChannel)
    {
        this.InitializeComponent();

        this.app = app;
        this.settings = settings;
        Scaler.Register(this.layoutTransform);
        messageDialogChannel.Open(this, true);

        this.Title = app.Title;
        this.SetApplicationIcon();
        // this.RemoveIcon();

        this.Closed += this.MainWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        this.contentFrame.Navigating += app.OnFrameNavigating; // Frame navigation does not support a DI container, hook into the Navigating event to create instances using a DI container.

        this.ApplyWindowPlacement(this.settings.WindowPlacement);
        this.nvHome.IsSelected = true;
    }

    #region IMessageDialogService

    Task<RadioResult<ContentDialogResult>> IMessageDialogService.ShowAsync(string title, string content, string primaryButtonText, string? cancelButtonText, string? secondaryButtonText, CancellationToken cancellationToken)
        => this.app.UIDispatcherQueue.EnqueueAsync(() => this.ShowMessageDialogAsync(title, content, primaryButtonText, cancelButtonText, secondaryButtonText, cancellationToken));

    #endregion

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {// The close button of the Window was pressed.
        args.Cancel = true; // Since the Closing function isn't awaiting, I'll cancel first. Sorry for writing such crappy code.
        await this.app.TryExitAsync();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        // Exit1
        this.settings.WindowPlacement = this.GetWindowPlacement();
    }

    private void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem selectedItem)
        {
            return;
        }

        switch (selectedItem.Tag)
        {
            case "Home":
                // this.contentFrame.Navigate(typeof(HomePage), null, new SuppressNavigationTransitionInfo());
                this.contentFrame.Navigate(typeof(HelloPage));
                break;
            case "Baibain":
                this.contentFrame.Navigate(typeof(BaibainPage));
                break;
            case "State":
                this.contentFrame.Navigate(typeof(StatePage));
                break;
            case "Message":
                this.contentFrame.Navigate(typeof(MessagePage));
                break;
            case "Advanced":
                this.contentFrame.Navigate(typeof(AdvancedPage));
                break;
            case "Settings":
                this.contentFrame.Navigate(typeof(SettingsPage));
                break;
            case "Information":
                this.contentFrame.Navigate(typeof(InformationPage));
                break;

            default:
                break;
        }
    }

    private async void nvExit_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        await this.app.TryExitAsync();
    }
}
