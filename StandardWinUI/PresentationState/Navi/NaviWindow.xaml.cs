// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Arc.WinUI;
using CrossChannel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace StandardWinUI.Presentation;

public interface INavigationService
{
    bool Navigate<TPage>(object? parameter = default)
        where TPage : Page;

    void Initialize(Frame frame);
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider serviceProvider;
    private Frame? frame;

    public NavigationService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Initialize(Frame frame)
    {
        this.frame = frame;
    }

    public bool Navigate<TPage>(object? parameter = default)
        where TPage : Page
    {
        if (this.frame is null)
        {
            return false;
        }

        var page = this.serviceProvider.GetRequiredService<TPage>();
        return this.frame.Navigate(page.GetType(), parameter);
    }
}

public partial class NaviWindow : WindowEx, IBasicPresentationService
{
    public NaviWindow(IChannel<IBasicPresentationService> basicPresentationChannel)
    {
        this.InitializeComponent();
        Scaler.Register(this.layoutTransform);
        basicPresentationChannel.Open(this, true);

        this.Title = App.Title;
        this.SetApplicationIcon();
        // this.RemoveIcon();

        this.Activated += this.NaviWindow_Activated;
        this.Closed += this.NaviWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        // this.contentFrame.Navigating += App.NavigatingHandler; // Frame navigation does not support a DI container, hook into the Navigating event to create instances using a DI container.

        this.LoadWindowPlacement(App.Settings.WindowPlacement);
        this.nvHome.IsSelected = true;
    }

    #region FieldAndProperty

    #endregion

    public Task<RadioResult<ulong>> MessageDialog(ulong title, ulong content, ulong defaultCommand, ulong cancelCommand, ulong secondaryCommand, CancellationToken cancellationToken)
    {
        return this.ShowMessageDialogAsync(title, content, defaultCommand, cancelCommand, secondaryCommand, cancellationToken);
    }

    public async Task<RadioResult<bool>> TryExit(CancellationToken cancellationToken = default)
    {
        var result = await this.ShowMessageDialogAsync(0, Hashed.Dialog.Exit, Hashed.Dialog.Yes, Hashed.Dialog.No, 0, cancellationToken);
        if (result.TryGetSingleResult(out var r) && r == Hashed.Dialog.Yes)
        {// Exit
            App.Exit();
            return new(true);
        }
        else
        {// Canceled
            return new(false);
        }
    }

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {// The close button of the Window was pressed.
        args.Cancel = true; // Since the Closing function isn't awaiting, I'll cancel first. Sorry for writing such crappy code.
        await this.TryExit();
    }

    private void NaviWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
    }

    private void NaviWindow_Closed(object sender, WindowEventArgs args)
    {
        // Exit1
        App.Settings.WindowPlacement = this.SaveWindowPlacement();
    }

    private async void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedItem = (NavigationViewItem)args.SelectedItem;
        switch (selectedItem.Tag)
        {
            case "Home":
                // this.contentFrame.Navigate(typeof(HomePage), null, new SuppressNavigationTransitionInfo());
                this.contentFrame.Navigate(typeof(HomePage));
                break;
            case "Presentation":
                this.contentFrame.Navigate(typeof(PresentationPage));
                break;
            case "State":
                this.contentFrame.Navigate(typeof(StatePage));
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
        await this.TryExit();
    }
}
