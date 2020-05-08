// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Application;
using Arc.Mvvm;
using Arc.Text;
using Arc.WinAPI;
using Arc.WPF;

#pragma warning disable SA1649 // File name should match first type name

namespace StandardApp
{
    public interface IMainViewService
    {
        void Notification(NotificationMessage msg); // Notification Message

        void MessageID(MessageId id); // Message Id

        Task<MessageBoxResult> Dialog(DialogParam p); // Dialog

        void CustomDialog(DialogParam p); // Csustom Dialog
    }

    /// <summary>
    /// Main Window.
    /// </summary>
    public partial class MainWindow : Window, IMainViewService
    {
        private MainViewModel vm; // ViewModel
        private Window? windowClosing = null; // Avoid an exception which occurs when Close () is called while the Window Close confirmation dialog is displayed.

        public MainWindow(MainViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.vm = vm;

            try
            {
                ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(1000));
            }
            catch
            {
            }
        }

        public void Notification(NotificationMessage msg)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            App.InvokeAsyncOnUI(() =>
            {
                this.textBlock.Text = msg.Notification;
                var result = MessageBox.Show(msg.Notification);
            });
        }

        public void MessageID(MessageId id)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            App.InvokeAsyncOnUI(() =>
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

                    App.C4.SetCulture(App.Settings.Culture);
                    Arc.WPF.C4Updater.C4Update();
                }
                else if (id == MessageId.Exit)
                { // Exit application with confirmation.
                    if (this.windowClosing == null)
                    {
                        this.Close();
                    }
                }
                else if (id == MessageId.ExitWithoutConfirmation)
                { // Exit application without confirmation.
                    App.SessionEnding = true;
                    if (this.windowClosing == null)
                    {
                        this.Close();
                    }
                    else
                    {
                        this.windowClosing.Close();
                    }
                }
                else if (id == MessageId.Information)
                {
                    var mit_license = "https://opensource.org/licenses/MIT";
                    var dlg = new Arc.WPF.Dialog(this);
                    dlg.TextBlock.Inlines.Add(
    @"Copyright (c) 2020 archi-Doc
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
            });
        }

        public async Task<MessageBoxResult> Dialog(DialogParam p)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            var dlg = new Arc.WPF.Dialog(this, p);
            var result = await dlg.ShowDialogAsync();
            return result;
            /*var tcs = new TaskCompletionSource<MessageBoxResult>();
            await this.Dispatcher.InvokeAsync(() => { dlg.ShowDialog(); tcs.SetResult(dlg.Result); }); // Avoid dead lock.
            return tcs.Task.Result;*/
        }

        public void CustomDialog(DialogParam p)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            var d = App.UI.InvokeAsync<MessageBoxResult>(() =>
            {
                var dlg = new Arc.WPF.Dialog(this);

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

            var d2 = App.UI.InvokeAsync<MessageBoxResult>(() =>
            {
                var dlg = new Arc.WPF.Dialog(this, p);

                dlg.ShowDialog();
                return dlg.Result;
            });

            d.Wait();

            return;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (!App.Settings.LoadError)
            { // Change the UI before this code. The window will be displayed shortly.
                WINDOWPLACEMENT wp = App.Settings.WindowPlacement;
                wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = wp.showCmd == SW.SHOWMINIMIZED ? SW.SHOWNORMAL : wp.showCmd;
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                Arc.WinAPI.Methods.SetWindowPlacement(hwnd, ref wp);
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
                this.windowClosing = dlg;
                dlg.ShowDialog();
                this.windowClosing = null;
                if (dlg.Result == MessageBoxResult.No)
                {
                    e.Cancel = true; // cancel
                    return;
                }
            }

            // Exit1 (Window is still visible)
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            Arc.WinAPI.Methods.GetWindowPlacement(hwnd, out var wp);
            App.Settings.WindowPlacement = wp;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
