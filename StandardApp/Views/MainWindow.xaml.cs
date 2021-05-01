// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application;
using Arc.CrossChannel;
using Arc.Mvvm;
using Arc.Text;
using Arc.WinAPI;
using Arc.WPF;
using StandardApp.ViewServices;

#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace StandardApp.Views
{
    /// <summary>
    /// Main Window.
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel vm; // ViewModel
        private IMainViewService mainViewService;

        public MainWindow(MainViewModel vm, IMainViewService mainViewService)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.vm = vm;
            this.mainViewService = mainViewService;
            this.mainViewService.Initialize(this);

            CrossChannel.OpenAsync<DialogParam, MessageBoxResult>(this.CrossChannel_Dialog);

            try
            {
                ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(1000));
            }
            catch
            {
            }

            this.listView.DropMoveAction = this.ListView_DropMove;

            Transformer.Instance.Register(this, true, false);

            this.Title = App.Title;
        }

        public async Task<MessageBoxResult> CrossChannel_Dialog(DialogParam p)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            var dlg = new Arc.WPF.Dialog(this, p);
            var result = await dlg.ShowDialogAsync();
            return result;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (!App.Settings.LoadError)
            { // Change the UI before this code. The window will be displayed shortly.
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
                WINDOWPLACEMENT wp = App.Settings.WindowPlacement.ToWINDOWPLACEMENT2(dpiX, dpiY);
                wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = wp.showCmd == SW.SHOWMINIMIZED ? SW.SHOWNORMAL : wp.showCmd;
                Arc.WinAPI.Methods.SetWindowPlacement(hwnd, ref wp);
                Transformer.Instance.AdjustWindowPosition(this);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.SessionEnding == false)
            {
                var dlg = new Arc.WPF.Dialog(this);
                dlg.Message = C4.Instance["dialog.exit"];
                dlg.Button = MessageBoxButton.YesNo; // button
                dlg.Result = MessageBoxResult.Yes; // focus
                dlg.Image = MessageBoxImage.Warning;
                this.mainViewService.SetClosingWindow(dlg);
                dlg.ShowDialog();
                this.mainViewService.SetClosingWindow(null);
                if (dlg.Result == MessageBoxResult.No)
                {
                    e.Cancel = true; // cancel
                    return;
                }
            }

            // Exit1 (Window is still visible)
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            Arc.WinAPI.Methods.GetWindowPlacement(hwnd, out var wp);
            Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            App.Settings.WindowPlacement.FromWINDOWPLACEMENT2(wp, dpiX, dpiY);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void ListView_DropMove(int oldIndex, int newIndex)
        {
            if ((oldIndex >= 0) && (newIndex >= 0))
            {
                this.vm.TestGoshujin.ObservableChain.Move(oldIndex, newIndex);
            }

            return;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems.Cast<TestItem>())
            {
                item.Selection = 0;
            }

            foreach (var item in e.AddedItems.Cast<TestItem>())
            {
                item.Selection = 1;
            }

            var listView = sender as ListView;
            if (listView != null)
            {
                var item = listView.SelectedItem as TestItem;
                if (item != null)
                {
                    item.Selection = 2; // selected+focus
                }
            }
        }
    }
}
