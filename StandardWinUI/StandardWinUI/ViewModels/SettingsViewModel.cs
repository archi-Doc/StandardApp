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
    }

    public void Prepare()
    {
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
    }

    [RelayCommand]
    private void SelectLanguage()
    {
    }

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
