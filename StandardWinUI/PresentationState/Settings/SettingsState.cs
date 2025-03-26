﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StandardWinUI.PresentationState;

public partial class SettingsState : ObservableObject, IState
{
    private readonly IApp app;
    private readonly AppSettings settings;

    public SettingsState(IApp app, AppSettings settings)
    {
        this.app = app;
        this.settings = settings;

        this.SetLanguageText();
        this.SetScalingText();
    }

    private void SetLanguageText()
    {
        if (LanguageList.LanguageToIdentifier.TryGetValue(this.settings.Culture, out var identifier))
        {
            this.LanguageText = HashedString.GetOrEmpty(identifier);
        }
    }

    private void SetScalingText()
    {
        this.ScalingText = Scaler.ScaleToText(Scaler.ViewScale);
    }

    [RelayCommand]
    private void OpenDataDirectory()
    {
        try
        {
            System.Diagnostics.Process.Start("Explorer.exe", this.app.DataFolder);
        }
        catch
        {
        }

        /*if (App.Settings.Culture == "ja")
        {
            App.Settings.Culture = "en";
        }
        else
        {
            App.Settings.Culture = "ja";
        }

        HashedString.ChangeCulture(App.Settings.Culture);
        Arc.WinUI.Stringer.Refresh();*/

        // this.GetPresentationService<IMessageDialog>().Show(Hashed.App.Name, Hashed.App.Description);
    }

    [RelayCommand]
    private void SelectLanguage(string language)
    {
        if (this.settings.Culture == language)
        {
            return;
        }

        this.settings.Culture = language;
        HashedString.ChangeCulture(this.settings.Culture);
        Arc.WinUI.Stringer.Refresh();
        this.SetLanguageText();
    }

    [RelayCommand]
    private void SelectScaling(double scaling)
    {
        if (Scaler.ViewScale == scaling)
        {
            return;
        }

        Scaler.ViewScale = scaling;
        Scaler.Refresh();
        this.SetScalingText();
    }

    [ObservableProperty]
    public partial string LanguageText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ScalingText { get; set; } = string.Empty;
}
