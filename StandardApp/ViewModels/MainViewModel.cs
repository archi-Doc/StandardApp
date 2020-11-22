// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Application;
using Arc.CrossChannel;
using Arc.Mvvm;
using Arc.WPF;
using StandardApp.ViewServices;

#pragma warning disable SA1201 // Elements should appear in the correct order

namespace StandardApp
{
    public class MainViewModel : BindableBase
    {
        public AppOptions Options => App.Options;

        private IMainViewService ViewService => App.Resolve<IMainViewService>(); // To avoid a circular dependency, get an instance when necessary.

        private bool hideDialogButton;

        public bool HideDialogButton
        {
            get => this.hideDialogButton;
            set => this.SetProperty(ref this.hideDialogButton, value);
        }

        private int number1;

        public int Number1
        {
            get
            {
                return this.number1;
            }

            set
            {
                this.SetProperty(ref this.number1, value); // this.Set(() => this.Number1, ref this.number1, value);
                this.Number3 = this.Number1 + this.Number2;
            }
        }

        private int number2;

        public int Number2
        {
            get
            {
                return this.number2;
            }

            set
            {
                this.number2 = value;
                this.Number3 = this.Number1 + this.Number2;
            }
        }

        private int number3;

        public int Number3
        {
            get
            {
                return this.number3;
            }

            set
            {
                this.SetProperty(ref this.number3, value); // this.Set(() => this.Number3, ref this.number3, value);
            }
        }

        private int number4;

        public int Number4
        {
            get
            {
                return this.number4;
            }

            set
            {
                this.SetProperty(ref this.number4, value); // this.Set(() => this.Number4, ref this.number4, value);
            }
        }

        private ICommand? commandMessageId;

        public ICommand CommandMessageId
        {
            get
            {
                return (this.commandMessageId != null) ? this.commandMessageId : this.commandMessageId = new DelegateCommand<string>(
                    (param) =>
                    { // execute
                        if (param != null)
                        {
                            var id = (MessageId)Enum.Parse(typeof(MessageId), param);
                            this.ViewService.MessageID(id);
                        }
                    });
            }
        }

        private bool commandFlag = true;

        private bool CommandFlag
        {
            get
            {
                return this.commandFlag;
            }

            set
            {
                this.SetProperty(ref this.commandFlag, value);
            }
        }

        private ICommand? testCommand4;

        public ICommand TestCommand4
        {
            get
            {
                return this.testCommand4 ??= new DelegateCommand(
                    async () =>
                    { // execute
                        this.HideDialogButton = !this.HideDialogButton;
                        await Task.Delay(1000);
                        this.CommandFlag = this.CommandFlag ? false : true;
                        this.Number4++;

                        // this.TestCommand.RaiseCanExecuteChanged(); // ObservesProperty(() => this.CommandFlag)
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

        private ICommand? testCommand5;

        public ICommand TestCommand5
        {
            get
            {
                return this.testCommand5 ??= new DelegateCommand(
                    async () =>
                    { // execute
                        // TestCommand4.Execute(null);
                        var p = default(DialogParam);
                        p.C4Name = "dialog.message";
                        p.Button = MessageBoxButton.YesNo;
                        p.Image = MessageBoxImage.Question;
                        var result = await this.ViewService.Dialog(p);
                        if (result == MessageBoxResult.Yes)
                        {
                            p.C4Name = "dialog.yes";
                            p.Button = MessageBoxButton.OK;
                            await this.ViewService.Dialog(p);
                        }
                        else
                        {
                            p.C4Name = "dialog.no";
                            p.Button = MessageBoxButton.OK;
                            await this.ViewService.Dialog(p);
                        }
                    });
            }
        }

        private ICommand? testCommand6;

        public ICommand TestCommand6
        {
            get
            {
                return this.testCommand6 ??= new DelegateCommand(
                    () =>
                    { // execute
                        var p = default(DialogParam);
                        p.C4Name = "app.name";
                        p.Button = MessageBoxButton.OK;
                        p.Image = MessageBoxImage.Information;
                        this.ViewService.CustomDialog(p);
                    });
            }
        }

        public DelegateCommand TestCommand2 { get; private set; }

        public DelegateCommand TestCommand3 { get; private set; }

        public DateTime Time1 { get; private set; } = DateTime.Now;

        public MainViewModel()
        {
            // this.TestCommand = new RelayCommand(this.TestExecute, () => { return this.commandFlag; });
            this.TestCommand2 = new DelegateCommand(this.TestExecute2);
            this.TestCommand3 = new DelegateCommand(this.TestExecute3);
        }

        private DelegateCommand? testCrossChannel;

        public DelegateCommand TestCrossChannel
        {
            get
            {
                return this.testCrossChannel ??= new DelegateCommand(
                    async () =>
                    { // CrossChannel version of DialogBox. View service is more preferable.
                        var p = default(DialogParam);
                        p.Message = "CrossChannel test.\r\nYes or No.";
                        p.Button = MessageBoxButton.YesNo;
                        p.Image = MessageBoxImage.Information;
                        var result = await CrossChannel.SendAsync<DialogParam, MessageBoxResult>(p);

                        if (result[0] == MessageBoxResult.Yes)
                        {
                            p.C4Name = "dialog.yes";
                            p.Button = MessageBoxButton.OK;
                            await CrossChannel.SendAsync<DialogParam, MessageBoxResult>(p);
                        }
                        else
                        {
                            p.C4Name = "dialog.no";
                            p.Button = MessageBoxButton.OK;
                            await CrossChannel.SendAsync<DialogParam, MessageBoxResult>(p);
                        }
                    });
            }
        }

        private DelegateCommand? dCommand;

        public DelegateCommand TestCommand
        {
            get
            {
                return this.dCommand ??= new DelegateCommand(
                    () =>
                {
                    if (App.Options.BrushCollection.Brush1.Brush?.Color == Colors.Green)
                    {
                        App.Options.BrushCollection.Brush1.Change(Colors.Red);
                    }
                    else
                    {
                        App.Options.BrushCollection.Brush1.Change(Colors.Green);
                    }

                    this.ViewService.Notification(new NotificationMessage("notification."));
                }, () => this.CommandFlag).ObservesProperty(() => this.CommandFlag);
            }
        }

        private void TestExecute2()
        {
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
                this.ViewService.MessageID(MessageId.ExitWithoutConfirmation);
                return;
            });
        }

        private void TestExecute3()
        {
            var p = default(DialogParam);
            p.C4Name = "app.description";
            this.ViewService.Dialog(p);
            return;
        }
    }
}
