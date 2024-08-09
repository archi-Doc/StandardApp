// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using StandardWinUI.ViewModels;
using WinUIEx;

namespace StandardWinUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SimpleWindow : WinUIEx.WindowEx
{
    public SimpleWindow()
    {
        this.InitializeComponent();
        this.InitializePresentation();
        this.ViewModel = App.GetService<SimpleViewModel>();
        this.Title = App.Title;
        this.SetApplicationIcon();
        // this.RemoveIcon();

        this.Activated += this.SimpleWindow_Activated;
        this.Closed += this.SimpleWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        this.LoadWindowPlacement(App.Settings.WindowPlacement);
    }

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true; // Since the Closing function isn't awaiting, I'll cancel first. Sorry for writing such crappy code.

        var result = await this.ShowMessageDialogAsync(0, Hashed.Dialog.Exit, Hashed.Dialog.Yes, Hashed.Dialog.No);
        if (result == Hashed.Dialog.Yes)
        {
            App.Exit();
        }
    }

    #region FieldAndProperty

    public SimpleViewModel ViewModel { get; }

    #endregion

    private void SimpleWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
    }

    private void SimpleWindow_Closed(object sender, WindowEventArgs args)
    {
        // Exit1
        App.Settings.WindowPlacement = this.SaveWindowPlacement();
    }

    private async void myButton_Click(object sender, RoutedEventArgs e)
    {
        // this.myButton.Content = "Clicked";

        Transformer.DisplayScaling *= 1.2;
        Transformer.Refresh();
    }

    private async void myButton_Click2(object sender, RoutedEventArgs e)
    {
        // this.myButton.Content = "Clicked2";

        Transformer.DisplayScaling *= 0.9;
        Transformer.Refresh();
    }
}
