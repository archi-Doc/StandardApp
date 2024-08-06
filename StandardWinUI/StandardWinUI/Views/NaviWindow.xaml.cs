// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Threading.Tasks;
using Arc.WinUI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StandardWinUI.ViewModels;
using WinUIEx;

namespace StandardWinUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NaviWindow : WinUIEx.WindowEx
{
    public NaviWindow()
    {
        this.InitializeComponent();
        this.ViewModel = App.GetService<NaviViewModel>();
        Transformer.Register(this);
        this.Title = App.Title;
        this.SetApplicationIcon();
        // this.RemoveIcon();

        this.Activated += this.NaviWindow_Activated;
        this.Closed += this.NaviWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        this.LoadWindowPlacement(App.Settings.WindowPlacement);
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

    internal NaviViewModel ViewModel { get; }

    #endregion

    private void NaviWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.nvHome.IsSelected = true;
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
                // this.ViewModel.NavigateToHome();
                break;
            case "Settings":
                // this.ViewModel.NavigateToSettings();
                break;
            case "Information":
                // this.ViewModel.NavigateToAbout();
                break;

            // await this.TryExit();
            default:
                break;
        }
    }

    private async void nvExit_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        await this.TryExit();
    }
}
