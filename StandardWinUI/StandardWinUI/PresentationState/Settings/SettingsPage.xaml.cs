// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using Arc.WinUI;
using Microsoft.UI.Xaml.Controls;
using StandardWinUI.ViewModels;

namespace StandardWinUI.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.ViewModel = App.GetStateObject<SettingsState>(this);

        // language: en, key: Language.En, text: English
        this.AddLanguage("en", "Language.En");
        this.AddLanguage("ja", "Language.Ja");
    }

    public SettingsState ViewModel { get; }

    private Dictionary<string, string> languageToName = new();
    private Dictionary<string, string> nameToLanguage = new();

    private void AddLanguage(string language, string key)
    {
        if (!HashedString.TryGet(HashedString.IdentifierToHash(key), out var text))
        {
            return;
        }

        var item = new MenuFlyoutItem
        {
            // DataContext = this.ViewModel,
            Text = text, // $"{{Arc:C4 Source=Settings.Language}}",
            Tag = language,
            Command = this.ViewModel.SelectLanguageCommand,
            CommandParameter = language,
        };

        C4.AddExtensionObject(item, MenuFlyoutItem.TextProperty, key);
        this.menuLanguage.Items.Add(item);

        this.languageToName[language] = text;
        this.languageToName[text] = language;
    }
}
