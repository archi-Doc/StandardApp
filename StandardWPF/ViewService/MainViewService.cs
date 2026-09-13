// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Application;
using Arc.Mvvm;
using Arc.WPF;
using DryIoc;
using StandardWPF.Views;

namespace StandardWPF.ViewServices;

/// <summary>
/// Provides WPF dialogs, notifications, and application-level UI actions.
/// </summary>
public interface IMainViewService
{
    // void SetWindow(Window window);

    // void SetClosingWindow(Window? closingWindow); // Avoid an exception which occurs when Close () is called while the Window Close confirmation dialog is displayed.

    /// <summary>
    /// Shows a notification message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    void ShowNotification(NotificationMessage message);

    /// <summary>
    /// Handles the specified message id (e.g. exit, switch culture, open settings).
    /// </summary>
    /// <param name="id">The message id.</param>
    void HandleMessage(MessageId id);

    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    /// <param name="parameters">The dialog parameters.</param>
    /// <returns>The dialog result.</returns>
    Task<MessageBoxResult> ShowDialogAsync(DialogParameters parameters);

    /// <summary>
    /// Shows a custom dialog.
    /// </summary>
    /// <param name="parameters">The dialog parameters.</param>
    void ShowCustomDialog(DialogParameters parameters);
}

/*internal class MainViewService : IMainViewService
{
    private Window? window;
    private Window? closingWindow;

    public void SetWindow(Window window)
    {
        this.window = window;
    }

    public void SetClosingWindow(Window? closingWindow)
    {
        this.closingWindow = closingWindow;
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
        if (this.window == null)
        {
            throw ThrowNotInitialized();
        }

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
                    this.window.Close();
                }
            }
            else if (id == MessageId.ExitWithoutConfirmation)
            { // Exit application without confirmation.
                App.IsSessionEnding = true;
                if (this.closingWindow == null)
                {
                    this.window.Close();
                }
                else
                {
                    this.closingWindow.Close();
                }
            }
            else if (id == MessageId.Information)
            {
                var mit_license = "https://opensource.org/licenses/MIT";
                var dlg = new Arc.WPF.MessageDialog(this.window);
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
                var dialog = App.Container.Resolve<SettingsWindow>();
                dialog.Initialize(this.window);
                dialog.ShowDialog();
            }
            else if (id == MessageId.DataFolder)
            {
                this.ShowNotification(new NotificationMessage(App.DataDirectory));
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
        if (this.window == null)
        {
            throw ThrowNotInitialized();
        }

        var dlg = new Arc.WPF.MessageDialog(this.window, parameters);
        var result = await dlg.ShowDialogAsync();
        return result;
        // var tcs = new TaskCompletionSource<MessageBoxResult>();
        // await this.Dispatcher.InvokeAsync(() => { dlg.ShowDialog(); tcs.SetResult(dlg.Result); }); // Avoid dead lock.
        // return tcs.Task.Result;
    }

    public void ShowCustomDialog(DialogParameters parameters)
    { // Multi-thread safe, may be called from non-UI thread/context. App.UIDispatcher.InvokeAsync()
        if (this.window == null)
        {
            throw ThrowNotInitialized();
        }

        var d = App.UIDispatcher.InvokeAsync<MessageBoxResult>(() =>
        {
            var dlg = new Arc.WPF.MessageDialog(this.window);

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
            var dlg = new Arc.WPF.MessageDialog(this.window, parameters);

            dlg.ShowDialog();
            return dlg.Result;
        });

        d.Wait();

        return;
    }

    private static Exception ThrowNotInitialized() => throw new InvalidOperationException("Call Initialize() before use.");
}*/
