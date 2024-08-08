// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace StandardWinUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
        this.Language = App.Settings.Culture;

        this.AddLanguage("Language.En", "en");
        this.AddLanguage("Language.Ja", "ja");
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
    }

    [RelayCommand]
    private void SelectLanguage(string key)
    {
        this.Language = this.language;
    }

    [ObservableProperty]
    private string language = string.Empty;

    private Dictionary<string, string> languageToName = new();
    private Dictionary<string, string> nameToLanguage = new();

    private void AddLanguage(string key, string language)
    {
        if (!HashedString.TryGet(key, out var result))
        {
            return;
        }

        var item = new MenuFlyoutItem
        {
            Text = result,
            Tag = language,
            Command = this.SelectLanguageCommand,
            CommandParameter = language,
        };

        // this.menuLanguage.Items.Add(item);
        this.languageToName[language] = result;
        this.languageToName[result] = language;
    }
}
