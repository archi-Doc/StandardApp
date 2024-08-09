// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using Arc.WinUI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using StandardWinUI;
using StandardWinUI.Views;
using WinUIEx;

namespace StandardWinUI.PresentationState;

public partial class NaviWindow : WindowEx, IMessageDialog
{
    public NaviWindow()
    {
        this.InitializeComponent();
        this.InitializePresentation();

        this.Title = App.Title;
        this.SetApplicationIcon();
        // this.RemoveIcon();

        this.Activated += this.NaviWindow_Activated;
        this.Closed += this.NaviWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        this.LoadWindowPlacement(App.Settings.WindowPlacement);
        this.nvHome.IsSelected = true;
    }

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true; // Since the Closing function isn't awaiting, I'll cancel first. Sorry for writing such crappy code.
        await this.TryExit();
    }

    private async Task TryExit()
    {
        var result = await this.ShowMessageDialogAsync(0, Hashed.Dialog.Exit, Hashed.Dialog.Yes, Hashed.Dialog.No);
        if (result == Hashed.Dialog.Yes)
        {
            App.Exit();
        }
    }

    #region FieldAndProperty

    #endregion

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

    Task<ulong> IMessageDialog.Show(ulong title, ulong content, ulong defaultCommand, ulong cancelCommand, ulong secondaryCommand)
    {
        return this.ShowMessageDialogAsync(title, content, defaultCommand, cancelCommand, secondaryCommand);
    }
}
