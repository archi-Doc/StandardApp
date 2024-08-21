// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using Arc.WinUI;
using Microsoft.UI.Xaml.Controls;
using StandardWinUI.States;

namespace StandardWinUI.Presentations;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.State = App.GetStateObject<SettingsState>(this);

        // language: en, key: Language.En, text: English
        this.AddLanguage("en", "Language.En");
        this.AddLanguage("ja", "Language.Ja");
    }

    public SettingsState State { get; }

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
            Text = text, // $"{{Arc:Stringer Source=Settings.Language}}",
            Tag = language,
            Command = this.State.SelectLanguageCommand,
            CommandParameter = language,
        };

        Stringer.Register(item, MenuFlyoutItem.TextProperty, key);
        this.menuLanguage.Items.Add(item);

        this.languageToName[language] = text;
        this.languageToName[text] = language;
    }
}
