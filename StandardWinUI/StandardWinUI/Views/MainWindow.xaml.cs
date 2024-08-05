// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;
using CommunityToolkit.WinUI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using StandardWinUI.ViewModels;
using WinUIEx;

namespace StandardWinUI.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : WinUIEx.WindowEx
{
    public MainWindow()
    {
        this.ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();

        this.Activated += this.MainWindow_Activated;
        this.Closed += this.MainWindow_Closed;
        this.AppWindow.Closing += this.AppWindow_Closing;

        // Set window placement
        var windowPlacement = App.Settings.WindowPlacement;
        if (windowPlacement.IsValid)
        {
            var hwnd = this.GetWindowHandle();
            Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            var wp = windowPlacement.ToWINDOWPLACEMENT2(dpiX, dpiY);
            wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Arc.WinAPI.WINDOWPLACEMENT));
            wp.flags = 0;
            wp.showCmd = wp.showCmd == Arc.WinAPI.SW.SHOWMAXIMIZED ? Arc.WinAPI.SW.SHOWMAXIMIZED : Arc.WinAPI.SW.SHOWNORMAL;
            Arc.WinAPI.Methods.SetWindowPlacement(hwnd, ref wp);
        }
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

    internal MainViewModel ViewModel { get; }

    #endregion

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        // Exit1
        var hwnd = this.GetWindowHandle();
        Arc.WinAPI.Methods.GetWindowPlacement(hwnd, out var wp);
        Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
        App.Settings.WindowPlacement.FromWINDOWPLACEMENT2(wp, dpiX, dpiY);
    }

    private async void myButton_Click(object sender, RoutedEventArgs e)
    {
        this.myButton.Content = "Clicked";

        if (this.Content is FrameworkElement element)
        {
            if (element.FindChild<Viewbox>() is { } viewbox)
            {
                viewbox.Stretch = Stretch.Uniform;
                viewbox.Width = viewbox.ActualWidth * 1.5;
                viewbox.Height = viewbox.ActualHeight * 1.5;
            }
        }
    }
}
