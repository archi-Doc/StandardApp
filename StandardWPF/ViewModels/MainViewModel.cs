// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Application;
using Arc.Mvvm;
using Arc.WPF;
using CrossChannel;
using StandardWPF.ViewServices;
using ValueLink;

#pragma warning disable SA1201 // Elements should appear in the correct order

namespace StandardWPF;

/// <summary>
/// Exposes the WPF sample's arithmetic, item collection, and UI commands.
/// </summary>
[ValueLinkObject]
public partial class MainViewModel
{
    public AppOptions Options => App.Options;

    public TestItem.GoshujinClass TestItems { get; } = App.Settings.TestItems;

    private IMainViewService ViewService => App.Resolve<IMainViewService>(); // To avoid a circular dependency, get an instance when necessary.

    [Link(AutoNotify = true, GenerateValue = true)]
    private bool hideDialogButton;

    private int addend1;

    public int Addend1
    {
        get
        {
            return this.addend1;
        }

        set
        {
            this.SetProperty(ref this.addend1, value);
            this.SumValue = this.Addend1 + this.Addend2;
        }
    }

    private int addend2;

    public int Addend2
    {
        get
        {
            return this.addend2;
        }

        set
        {
            this.SetProperty(ref this.addend2, value);
            this.SumValue = this.Addend1 + this.Addend2;
        }
    }

    [Link(AutoNotify = true, GenerateValue = true)]
    private int sum;

    [Link(AutoNotify = true, GenerateValue = true)]
    private int toggleCount;

    private ICommand? addItemCommand;

    public ICommand AddItemCommand
    {
        get
        {
            return (this.addItemCommand != null) ? this.addItemCommand : this.addItemCommand = new DelegateCommand(
                () =>
                {
                    if (this.TestItems.QueueChain.Count >= 5)
                    {// Limits the number of objects.
                        this.TestItems.QueueChain.Peek().Goshujin = null;
                    }

                    var last = this.TestItems.IdChain.Last;
                    var id = last == null ? 0 : last.IdValue + 1;
                    var item = new TestItem(id, DateTime.UtcNow);
                    item.Goshujin = this.TestItems;
                });
        }
    }

    private ICommand? clearItemsCommand;

    public ICommand ClearItemsCommand
    {
        get
        {
            return this.clearItemsCommand ?? (this.clearItemsCommand = new DelegateCommand(
                () =>
                {
                    this.TestItems.ClearAll();
                }));
        }
    }

    private ICommand? incrementSelectedIdCommand;

    public ICommand IncrementSelectedIdCommand
    {
        get
        {
            return this.incrementSelectedIdCommand ?? (this.incrementSelectedIdCommand = new DelegateCommand(
                () =>
                {
                    foreach (var x in this.TestItems.ObservableChain.Where(x => x.SelectionState == 2))
                    {
                        x.IdValue++;
                    }
                }));
        }
    }

    private ICommand? decrementSelectedIdCommand;

    public ICommand DecrementSelectedIdCommand
    {
        get
        {
            return this.decrementSelectedIdCommand ?? (this.decrementSelectedIdCommand = new DelegateCommand(
                () =>
                {
                    foreach (var x in this.TestItems.ObservableChain.Where(x => x.SelectionState == 2))
                    {
                        if (x.IdValue > 0)
                        {
                            x.IdValue--;
                        }
                    }
                }));
        }
    }

    private ICommand? sendMessageIdCommand;

    public ICommand SendMessageIdCommand
    {
        get
        {
            return (this.sendMessageIdCommand != null) ? this.sendMessageIdCommand : this.sendMessageIdCommand = new DelegateCommand<string>(
                (param) =>
                { // execute
                    if (param != null)
                    {
                        var id = (MessageId)Enum.Parse(typeof(MessageId), param);
                        this.ViewService.HandleMessage(id);
                    }
                });
        }
    }

    [Link(AutoNotify = true, GenerateValue = true)]
    private bool isToggleBrushColorEnabled = true;

    private ICommand? toggleEnabledStateCommand;

    public ICommand ToggleEnabledStateCommand
    {
        get
        {
            return this.toggleEnabledStateCommand ??= new DelegateCommand(
                async () =>
                { // execute
                    this.HideDialogButtonValue = !this.HideDialogButtonValue;
                    await Task.Delay(1000);
                    this.IsToggleBrushColorEnabledValue = !this.IsToggleBrushColorEnabledValue;
                    this.ToggleCountValue++;

                    // this.ToggleBrushColorCommand.RaiseCanExecuteChanged(); // ObservesProperty(() => this.IsToggleBrushColorEnabledValue)
                });
            /*() =>
            {//execute
                Task.Run(() =>
                {
                    gl.CheckInvokeAsyncOnUI(() => {
                        System.Threading.Thread.Sleep(1000);
                        commandFlag = commandFlag ? false : true;
                        this.TestCommand.RaiseCanExecuteChanged();
                    });

                });
                //commandFlag = commandFlag ? false : true;
            }*/
        }
    }

    private ICommand? showYesNoDialogCommand;

    public ICommand ShowYesNoDialogCommand
    {
        get
        {
            return this.showYesNoDialogCommand ??= new DelegateCommand(
                async () =>
                { // execute
                    var p = default(DialogParameters);
                    p.MessageHash = Hashed.Dialog.Message;
                    p.Button = MessageBoxButton.YesNo;
                    p.Image = MessageBoxImage.Question;
                    var result = await this.ViewService.ShowDialogAsync(p);
                    if (result == MessageBoxResult.Yes)
                    {
                        p.MessageHash = Hashed.Dialog.Yes;
                        p.Button = MessageBoxButton.OK;
                        await this.ViewService.ShowDialogAsync(p);
                    }
                    else
                    {
                        p.MessageHash = Hashed.Dialog.No;
                        p.Button = MessageBoxButton.OK;
                        await this.ViewService.ShowDialogAsync(p);
                    }
                });
        }
    }

    private ICommand? showCustomDialogCommand;

    public ICommand ShowCustomDialogCommand
    {
        get
        {
            return this.showCustomDialogCommand ??= new DelegateCommand(
                () =>
                { // execute
                    var p = default(DialogParameters);
                    p.MessageHash = Hashed.App.Name;
                    p.Button = MessageBoxButton.OK;
                    p.Image = MessageBoxImage.Information;
                    this.ViewService.ShowCustomDialog(p);
                });
        }
    }

    public DelegateCommand ExitWithoutConfirmationCommand { get; private set; }

    public DelegateCommand ShowDescriptionDialogCommand { get; private set; }

    public DateTime CreatedTime { get; private set; } = DateTime.Now;

    public MainViewModel()
    {
        this.ExitWithoutConfirmationCommand = new DelegateCommand(this.ExitWithoutConfirmationAfterDelay);
        this.ShowDescriptionDialogCommand = new DelegateCommand(this.ShowDescriptionDialog);
    }

    private DelegateCommand? testCrossChannelCommand;

    public DelegateCommand TestCrossChannelCommand
    {
        get
        {
            return this.testCrossChannelCommand ??= new DelegateCommand(
                async () =>
                { // CrossChannel version of DialogBox. View service is more preferable.
                    var p = default(DialogParameters);
                    p.Message = "CrossChannel test.\r\nYes or No.";
                    p.Button = MessageBoxButton.YesNo;
                    p.Image = MessageBoxImage.Information;
                    /*var result = await Radio.SendTwoWayAsync<DialogParameters, MessageBoxResult>(p);

                    if (result[0] == MessageBoxResult.Yes)
                    {
                        p.MessageHash = Hashed.Dialog.Yes;
                        p.Button = MessageBoxButton.OK;
                        await Radio.SendTwoWayAsync<DialogParameters, MessageBoxResult>(p);
                    }
                    else
                    {
                        p.MessageHash = Hashed.Dialog.No;
                        p.Button = MessageBoxButton.OK;
                        await Radio.SendTwoWayAsync<DialogParameters, MessageBoxResult>(p);
                    }*/
                });
        }
    }

    private DelegateCommand? testCrossChannel2Command;

    public DelegateCommand TestCrossChannel2Command
    {
        get
        {
            return this.testCrossChannel2Command ??= new DelegateCommand(
                async () =>
                {
                    /*var result = await Radio.SendTwoWayAsync<string, MessageBoxResult>("Test message");
                    if (result.Length > 0)
                    {
                        await Radio.SendTwoWayAsync<string, MessageBoxResult>(result[0].ToString());
                    }*/
                });
        }
    }

    private DelegateCommand? toggleBrushColorCommand;

    public DelegateCommand ToggleBrushColorCommand
    {
        get
        {
            return this.toggleBrushColorCommand ??= new DelegateCommand(
                () =>
                {
                    if (App.Options.BrushCollection.Brush1.Brush?.Color == Colors.Green)
                    {
                        App.Options.BrushCollection.Brush1.SetColor(Colors.Red);
                    }
                    else
                    {
                        App.Options.BrushCollection.Brush1.SetColor(Colors.Green);
                    }

                    this.ViewService.ShowNotification(new NotificationMessage("notification."));
                },
                () => this.IsToggleBrushColorEnabledValue).ObservesProperty(() => this.IsToggleBrushColorEnabledValue);
        }
    }

    private void ExitWithoutConfirmationAfterDelay()
    {
        Task.Run(() =>
        {
            System.Threading.Thread.Sleep(1000);
            this.ViewService.HandleMessage(MessageId.ExitWithoutConfirmation);
            return;
        });
    }

    private void ShowDescriptionDialog()
    {
        var p = default(DialogParameters);
        p.MessageHash = Hashed.App.Description;
        this.ViewService.ShowDialogAsync(p);
        return;
    }
}
