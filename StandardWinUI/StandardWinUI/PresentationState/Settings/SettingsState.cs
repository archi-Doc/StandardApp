// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using Arc.WinUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.State;

public partial class SettingsState : StateObject
{
    public SettingsState()
    {
        this.Language = App.Settings.Culture;
    }

    [RelayCommand]
    private void OpenDataDirectory()
    {
        try
        {
            System.Diagnostics.Process.Start("Explorer.exe", App.DataFolder);
        }
        catch
        {
        }

        if (App.Settings.Culture == "ja")
        {
            App.Settings.Culture = "en";
        }
        else
        {
            App.Settings.Culture = "ja";
        }

        HashedString.ChangeCulture(App.Settings.Culture);
        Arc.WinUI.C4.Refresh();

        this.GetPresentationService<IMessageDialog>().Show(Hashed.App.Name, Hashed.App.Description);
    }

    [RelayCommand]
    private void SelectLanguage(string language)
    {
        if (App.Settings.Culture == language)
        {
            return;
        }

        App.Settings.Culture = language;
        HashedString.ChangeCulture(App.Settings.Culture);
        Arc.WinUI.C4.Refresh();

        this.Language = HashedString.GetOrEmpty(language);
    }

    [ObservableProperty]
    private string language = string.Empty;
}
