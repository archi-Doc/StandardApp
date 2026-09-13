// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Application;
using Arc.Mvvm;
using Arc.WinAPI;
using Arc.WPF;
using CrossChannel;
using StandardWPF.ViewServices;
using Tinyhand;

#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace StandardWPF.Views;

/// <summary>
/// Hosts the WPF samples and implements application-level view services.
/// </summary>
public partial class MainWindow : Window, IMainViewService
{
    private MainViewModel vm; // ViewModel
    private Window? closingWindow = null; // Avoid an exception which occurs when Close () is called while the Window Close confirmation dialog is displayed.

    public MainWindow(MainViewModel vm)
    {
        this.InitializeComponent();
        this.DataContext = vm;
        this.vm = vm;

        /* Radio.OpenTwoWayAsync<DialogParameters, MessageBoxResult>(this.ShowCrossChannelDialogAsync, this);
        Radio.OpenTwoWayAsync<string, MessageBoxResult>(
            x =>
            {
                var result = App.UIDispatcher.InvokeAsync<MessageBoxResult>(() => MessageBox.Show(x, "test", MessageBoxButton.OKCancel));
                return result.Task;
            },
            this);*/

        try
        {
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(1000));
        }
        catch
        {
        }

        this.listView.ItemMovedAction = this.ListView_ItemMoved;

        Transformer.Instance.Register(this, true, false);

        this.Title = App.Title;
    }

    public void ShowNotification(NotificationMessage message)
    { // Multi-thread safe, may be called from non-UI thread/context. App.UIDispatcher.InvokeAsync()
        App.ExecuteOrEnqueueOnUI(() =>
        {
            var result = MessageBox.Show(message.Notification);
        });
    }

    public void HandleMessage(MessageId id)
    {// Multi-thread safe, may be called from non-UI thread/context. App.UIDispatcher.InvokeAsync()
        App.ExecuteOrEnqueueOnUI(() =>
        { // UI thread.
            if (id == MessageId.SwitchCulture)
            { // Switch culture.
                if (App.Settings.Culture == "ja")
                {
                    App.Settings.Culture = "en";
                }
                else
                {
                    App.Settings.Culture = "ja";
                }

                HashedString.TrySetCurrentCulture(App.Settings.Culture);
                Arc.WPF.Stringer.Refresh();
            }
            else if (id == MessageId.Exit)
            { // Exit application with confirmation.
                if (this.closingWindow == null)
                {
                    this.Close();
                }
            }
            else if (id == MessageId.ExitWithoutConfirmation)
            { // Exit application without confirmation.
                App.IsSessionEnding = true;
                if (this.closingWindow == null)
                {
                    this.Close();
                }
                else
                {
                    this.closingWindow.Close();
                }
            }
            else if (id == MessageId.Information)
            {
                var mit_license = "https://opensource.org/licenses/MIT";
                var dlg = new Arc.WPF.MessageDialog(this);
                dlg.TextBlock.Inlines.Add(
@"Copyright (c) 2021 archi-Doc
Released under the MIT license
");
                var h = new Hyperlink() { NavigateUri = new Uri(mit_license) };
                h.Inlines.Add(mit_license);
                h.RequestNavigate += (s, e) =>
                {
                    try
                    {
                        App.OpenBrowser(e.Uri.ToString());
                    }
                    catch
                    {
                    }
                };
                dlg.TextBlock.Inlines.Add(h);
                dlg.ShowDialog();
            }
            else if (id == MessageId.Settings)
            {
                var dialog = App.Resolve<SettingsWindow>();
                dialog.Initialize(this);
                dialog.ShowDialog();
            }
            else if (id == MessageId.DataFolder)
            {
                // this.ShowNotification(new NotificationMessage(App.DataDirectory));
                System.Diagnostics.Process.Start("Explorer.exe", App.DataDirectory);
            }
            else if (id == MessageId.DisplayScaling)
            {
                Transformer.Instance.Transform(App.Settings.DisplayScaling, App.Settings.DisplayScaling);

                // this.FontSize = AppConst.DefaultFontSize * App.Settings.DisplayScaling;
            }
        });
    }

    public async Task<MessageBoxResult> ShowDialogAsync(DialogParameters parameters)
    { // Multi-thread safe, may be called from non-UI thread/context. App.UIDispatcher.InvokeAsync()
        return await this.Dispatcher.InvokeAsync(() =>
        {
            var dlg = new Arc.WPF.MessageDialog(this, parameters);
            dlg.ShowDialog();
            return dlg.Result;
        });
        /*var tcs = new TaskCompletionSource<MessageBoxResult>();
        await this.Dispatcher.InvokeAsync(() => { dlg.ShowDialog(); tcs.SetResult(dlg.Result); }); // Avoid dead lock.
        return tcs.Task.Result;*/
    }

    public void ShowCustomDialog(DialogParameters parameters)
    { // Multi-thread safe, may be called from non-UI thread/context. App.UIDispatcher.InvokeAsync()
        var d = App.UIDispatcher.InvokeAsync<MessageBoxResult>(() =>
        {
            var dlg = new Arc.WPF.MessageDialog(this);

            dlg.TextBlock.Inlines.Add("Normal text...\r\n");
            dlg.TextBlock.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run("Bold text")));
            dlg.TextBlock.Inlines.Add("\r\nand\r\n");
            dlg.TextBlock.Inlines.Add(new System.Windows.Documents.Italic(new System.Windows.Documents.Run("Italic text")));
            dlg.TextBlock.Inlines.Add("\r\n");
            dlg.TextBlock.Inlines.Add("You can change\r\n");

            var span = new System.Windows.Documents.Span(new System.Windows.Documents.Run("Text color"));
            span.Foreground = new SolidColorBrush(Colors.Red);
            dlg.TextBlock.Inlines.Add(span);

            dlg.Button = MessageBoxButton.YesNoCancel; // button
            dlg.Result = MessageBoxResult.Cancel; // focus
            dlg.Image = MessageBoxImage.Warning;

            dlg.ShowDialog();

            MessageBoxResult result = dlg.Result;
            return result;
        });

        var d2 = App.UIDispatcher.InvokeAsync<MessageBoxResult>(() =>
        {
            var dlg = new Arc.WPF.MessageDialog(this, parameters);

            dlg.ShowDialog();
            return dlg.Result;
        });

        d.Wait();

        return;
    }

    public async Task<MessageBoxResult> ShowCrossChannelDialogAsync(DialogParameters parameters)
    { // Multi-thread safe, may be called from non-UI thread/context. App.UIDispatcher.InvokeAsync()
        var dlg = new Arc.WPF.MessageDialog(this, parameters);
        var result = await dlg.ShowDialogAsync();
        return result;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        if (!App.Settings.HasLoadError)
        { // Change the UI before this code. The window will be displayed shortly.
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            Arc.WinAPI.NativeMethods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            WINDOWPLACEMENT wp = App.Settings.WindowPlacement.ToWindowPlacementWithPhysicalPosition(dpiX, dpiY);
            wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            wp.flags = 0;
            wp.showCmd = wp.showCmd == SW.SHOWMINIMIZED ? SW.SHOWNORMAL : wp.showCmd;
            Arc.WinAPI.NativeMethods.SetWindowPlacement(hwnd, ref wp);
            Transformer.Instance.AdjustWindowPosition(this);
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (App.IsSessionEnding == false)
        {
            var dlg = new Arc.WPF.MessageDialog(this);
            dlg.Message = HashedString.Get("Dialog.Exit");
            dlg.Button = MessageBoxButton.YesNo; // button
            dlg.Result = MessageBoxResult.Yes; // focus
            dlg.Image = MessageBoxImage.Warning;
            this.closingWindow = dlg;
            dlg.ShowDialog();
            this.closingWindow = null;
            if (dlg.Result == MessageBoxResult.No)
            {
                e.Cancel = true; // cancel
                return;
            }
        }

        // Exit1 (Window is still visible)
        IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        if (Arc.WinAPI.NativeMethods.GetWindowPlacement(hwnd, out var wp))
        {
            Arc.WinAPI.NativeMethods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            App.Settings.WindowPlacement.FromWindowPlacementWithPhysicalPosition(wp, dpiX, dpiY);
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

    private void ListView_ItemMoved(int oldIndex, int newIndex)
    {
        if ((oldIndex >= 0) && (newIndex >= 0))
        {
            this.vm.TestItems.ObservableChain.Move(oldIndex, newIndex);
        }

        return;
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.RemovedItems.Cast<TestItem>())
        {
            item.SelectionState = 0;
        }

        foreach (var item in e.AddedItems.Cast<TestItem>())
        {
            item.SelectionState = 1;
        }

        var listView = sender as ListView;
        if (listView != null)
        {
            var item = listView.SelectedItem as TestItem;
            if (item != null)
            {
                item.SelectionState = 2; // selected+focus
            }
        }
    }
}
