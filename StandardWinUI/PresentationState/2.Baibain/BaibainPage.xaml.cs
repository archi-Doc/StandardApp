// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.PresentationState;

public sealed partial class BaibainPage : Page
{
    public BaibainPage()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (int.TryParse(this.textBox1.Text, out int value))
        {
            this.textBox2.Text = (value * 2).ToString();
        }
    }
}
