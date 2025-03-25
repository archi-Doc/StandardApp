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

public partial class NaviWindow : WindowEx, IMessageDialogService
{
    private readonly App app;

    public NaviWindow(App app, IChannel<IMessageDialogService> messageDialogChannel)
    {
        this.app = app;
        this.InitializeComponent();
        Scaler.Register(this.layoutTransform);
        messageDialogChannel.Open(this, true);

        this.Title = app.Title;
        this.SetApplicationIcon();
        // this.RemoveIcon();

        this.Activated += this.NaviWindow_Activated;
        this.Closed += this.NaviWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        this.contentFrame.Navigating += app.NavigatingHandler; // Frame navigation does not support a DI container, hook into the Navigating event to create instances using a DI container.

        this.LoadWindowPlacement(app.Settings.WindowPlacement);
        this.nvHome.IsSelected = true;
    }

    #region IBasicPresentationService

    Task<RadioResult<ContentDialogResult>> IMessageDialogService.Show(string title, string content, string primaryCommand, string? cancelCommand, string? secondaryCommand, CancellationToken cancellationToken)
        => this.app.UiDispatcherQueue.EnqueueAsync(() => this.ShowMessageDialogAsync(title, content, primaryCommand, cancelCommand, secondaryCommand, cancellationToken));

    #endregion

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {// The close button of the Window was pressed.
        args.Cancel = true; // Since the Closing function isn't awaiting, I'll cancel first. Sorry for writing such crappy code.
        await this.app.TryExit();
    }

    private void NaviWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
    }

    private void NaviWindow_Closed(object sender, WindowEventArgs args)
    {
        // Exit1
        this.app.Settings.WindowPlacement = this.SaveWindowPlacement();
    }

    private async void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = (NavigationViewItem)args.SelectedItem;
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
        await this.app.TryExit();
    }
}
