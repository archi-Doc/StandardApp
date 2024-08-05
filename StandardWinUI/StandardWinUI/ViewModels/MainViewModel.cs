// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    public MainViewModel(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    private readonly AppSettings appSettings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextUpper))]
    private string _text = string.Empty;

    public string TextUpper => this.Text.ToUpper();

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
    private void SwitchLanguage()
    {
        if (this.appSettings.Culture == "ja")
        {
            this.appSettings.Culture = "en";
        }
        else
        {
            this.appSettings.Culture = "ja";
        }

        HashedString.ChangeCulture(this.appSettings.Culture);
        Arc.WinUI.C4.Refresh();
    }
}
