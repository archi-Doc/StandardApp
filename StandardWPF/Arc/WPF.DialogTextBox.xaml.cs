// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tinyhand;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace Arc.WPF;

public delegate bool TextValidator(ref string text); // Delegate to validate text. true:valid, false:invalid.

public delegate Task<string> AsyncTextValidator(string text); // Asynchronous version. null:invalid non-null:valid.

/// <summary>
/// Specifies text input, validation callbacks, and dialog buttons.
/// </summary>
public struct TextInputDialogParameters
{ // parameters
    public ulong MessageHash; // 1st: Message hash
    public string Message; // 2nd: Message
    public MessageBoxButton Button;
    public MessageBoxResult Result;
    public string Text;
    public int TextMaxLength; // max length. 0=no limit
    public TextValidator ValidateText;
    public AsyncTextValidator ValidateTextAsync;  // public TaskCompletionSource<DialogStringResult> TCS;
}

/// <summary>
/// Contains the accepted text and the dialog result.
/// </summary>
public struct TextInputDialogResult
{ // result
    public string Text;
    public MessageBoxResult Result;

    public TextInputDialogResult(string text, MessageBoxResult result)
    {
        this.Text = text;
        this.Result = result;
    }
}

/// <summary>
/// Dialog with a text box.
/// </summary>
public partial class TextInputDialog : Window
{
    private string fMessage = string.Empty;
    private MessageBoxButton fButton = MessageBoxButton.OK;
    private MessageBoxResult fResult = MessageBoxResult.None; // focused button and dialog result.

    public string Message
    {
        get { return this.fMessage; } set { this.fMessage = value; }
    }

    public MessageBoxButton Button
    {
        get { return this.fButton; } set { this.fButton = value; }
    }

    public MessageBoxResult Result
    {
        get { return this.fResult; } set { this.fResult = value; }
    }

    public TextBlock TextBlock
    {
        get { return this.PART_TextBlock; }
    }

    public string Text { get; private set; }

    public TextValidator ValidateText { get; private set; }

    public AsyncTextValidator ValidateTextAsync { get; private set; }

    private string captionOK;
    private string captionCancel;
    private string captionYes;
    private string captionNo;

    public TextInputDialog(Window owner, TextInputDialogParameters parameters)
    {
        this.InitializeComponent();

        this.captionOK = HashedString.GetOrAlternative("Dialog.Ok", "O K");
        this.captionCancel = HashedString.GetOrAlternative("Dialog.Cancel", "Cancel");
        this.captionYes = HashedString.GetOrAlternative("Dialog.Yes", "Yes");
        this.captionNo = HashedString.GetOrAlternative("Dialog.No", "No");

        // settings
        this.FontSize = owner.FontSize;
        this.Owner = owner;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.ShowInTaskbar = false;
        if (parameters.TextMaxLength != 0)
        {
            this.textBox.MaxLength = parameters.TextMaxLength;
        }

        // visual
        this.Foreground = Brushes.DarkBlue;

        // set parameters
        if (parameters.MessageHash != 0)
        {
            this.fMessage = HashedString.GetOrEmpty(parameters.MessageHash);
        }

        if (this.fMessage == null || this.fMessage == string.Empty)
        {
            this.fMessage = parameters.Message;
        }

        if (this.fMessage == null)
        {
            this.fMessage = string.Empty;
        }

        this.fButton = parameters.Button;
        this.fResult = parameters.Result;
        this.Text = parameters.Text;
        this.ValidateText = parameters.ValidateText;
        this.ValidateTextAsync = parameters.ValidateTextAsync;

        this.textBox.Text = this.Text;
        if (this.PART_TextBlock.Inlines.Count < 1)
        {
            this.PART_TextBlock.Text = this.fMessage;
        }

        this.SetupButton();

        Transformer.Instance.Register(this);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        this.DragMove();
    }

    // Setup Buttons.
    private void SetupButton()
    {
        if (this.fButton == MessageBoxButton.OK)
        {
            this.CreateButton("btnOK", this.captionOK);
        }
        else if (this.fButton == MessageBoxButton.OKCancel)
        {
            this.CreateButton("btnOK", this.captionOK);
            this.CreateButton("btnCancel", this.captionCancel);
        }
        else if (this.fButton == MessageBoxButton.YesNo)
        {
            this.CreateButton("btnYes", this.captionYes);
            this.CreateButton("btnNo", this.captionNo);
        }
        else if (this.fButton == MessageBoxButton.YesNoCancel)
        {
            this.CreateButton("btnYes", this.captionYes);
            this.CreateButton("btnNo", this.captionNo);
            this.CreateButton("btnCancel", this.captionCancel);
        }

        // right margin.
        var border = new Border();
        border.Width = 10;
        this.PART_StackPanel.Children.Add(border);
    }

    private async Task<bool> Process()
    { // true: success, false: fail
        string t = this.textBox.Text;
        bool flag = true;

        if (this.ValidateText != null)
        {
            flag = this.ValidateText(ref t); // validate.
        }

        if (this.ValidateTextAsync != null)
        {
            t = await this.ValidateTextAsync(t); // validate async
        }

        if (t != null)
        {
            this.textBox.Text = t; // Set textbox.
        }

        if (!flag || t == null)
        { // fail
            this.FocusTextBox();
            return false;
        }

        this.Text = t;
        return true;
    }

    private void FocusTextBox()
    { // focus on the text box
        Keyboard.Focus(this.textBox);
        this.textBox.SelectAll();
    }

    private async void Button_Click(object sender, EventArgs e)
    {
        Button? button = sender as Button;
        if (button == null)
        {
            return;
        }

        if (button.Name == "btnOK")
        {
            this.fResult = MessageBoxResult.OK;
        }
        else if (button.Name == "btnCancel")
        {
            this.fResult = MessageBoxResult.Cancel;
        }
        else if (button.Name == "btnYes")
        {
            this.fResult = MessageBoxResult.Yes;
        }
        else if (button.Name == "btnNo")
        {
            this.fResult = MessageBoxResult.No;
        }

        if (this.fResult == MessageBoxResult.OK || this.fResult == MessageBoxResult.Yes)
        { // check
            bool result = await this.Process();
            if (!result)
            {
                return;
            }
        }

        this.Close();
    }

    private async void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        { // escape key
            if (this.fButton == MessageBoxButton.OK)
            {// ok -> ok
                this.fResult = MessageBoxResult.OK;
                this.Close();
            }
            else if (this.fButton == MessageBoxButton.OKCancel)
            {// ok cancel -> cancel
                this.fResult = MessageBoxResult.Cancel;
                this.Close();
            }
            else if (this.fButton == MessageBoxButton.YesNo)
            {// yes no -> wait
            }
            else if (this.fButton == MessageBoxButton.YesNoCancel)
            {// yes no cancel -> cancel
                this.fResult = MessageBoxResult.Cancel;
                this.Close();
            }
        }
        else if (e.Key == Key.Enter)
        {
            if (this.fButton == MessageBoxButton.OK)
            {// ok -> ok
                this.fResult = MessageBoxResult.OK;
            }
            else if (this.fButton == MessageBoxButton.OKCancel)
            {// ok cancel -> ok
                this.fResult = MessageBoxResult.OK;
            }
            else if (this.fButton == MessageBoxButton.YesNo)
            {// yes no -> yes
                this.fResult = MessageBoxResult.Yes;
            }
            else if (this.fButton == MessageBoxButton.YesNoCancel)
            {// yes no cancel -> yes
                this.fResult = MessageBoxResult.Yes;
            }

            bool result = await this.Process();
            if (result)
            {
                this.Close();
            }
        }
    }

    // Create button and set focus on specified button (fResult).
    private void CreateButton(string name, string caption)
    {
        var button = new Button();
        button.Name = name;
        button.Width = 80;
        button.Content = caption;
        button.Margin = new Thickness(0, 12, 6, 12);

        button.Click += new RoutedEventHandler(this.Button_Click);

        this.PART_StackPanel.Children.Add(button);

        if (this.fResult == MessageBoxResult.None)
        {
            if ((name == "btnOK") || (name == "btnYes"))
            {
                Keyboard.Focus(button);
            }
        }
        else if (this.fResult == MessageBoxResult.OK)
        {
            if (name == "btnOK")
            {
                Keyboard.Focus(button);
            }
        }
        else if (this.fResult == MessageBoxResult.Cancel)
        {
            if (name == "btnCancel")
            {
                Keyboard.Focus(button);
            }
        }
        else if (this.fResult == MessageBoxResult.Yes)
        {
            if (name == "btnYes")
            {
                Keyboard.Focus(button);
            }
        }
        else if (this.fResult == MessageBoxResult.No)
        {
            if (name == "btnNo")
            {
                Keyboard.Focus(button);
            }
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.FocusTextBox();
    }
}
