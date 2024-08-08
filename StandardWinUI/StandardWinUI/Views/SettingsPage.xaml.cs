// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using Arc.WinUI;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using StandardWinUI.ViewModels;

namespace StandardWinUI.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.ViewModel = App.GetService<SettingsViewModel>();

        this.AddLanguage("Language.En", "en");
        this.AddLanguage("Language.Ja", "ja");
        // this.ViewModel.Prepare();
    }

    public SettingsViewModel ViewModel { get; }

    private Dictionary<string, string> languageToName = new();
    private Dictionary<string, string> nameToLanguage = new();

    private void AddLanguage(string key, string language)
    {
        if (!HashedString.TryGet(HashedString.IdentifierToHash(key), out var result))
        {
            return;
        }

        var item = new MenuFlyoutItem
        {
            // DataContext = this.ViewModel,
            Text = HashedString.GetOrIdentifier(key), // $"{{Arc:C4 Source=Settings.Language}}",
            Tag = language,
            Command = this.ViewModel.SelectLanguageCommand,
            CommandParameter = language,
        };

        C4.AddExtensionObject(item, MenuFlyoutItem.TextProperty, key);
        this.menuLanguage.Items.Add(item);

        this.languageToName[language] = result;
        this.languageToName[result] = language;
    }
}
