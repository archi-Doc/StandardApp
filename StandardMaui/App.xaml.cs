// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace StandardMaui;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        this.MainPage = new AppShell();
    }
}
