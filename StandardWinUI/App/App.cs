// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

#pragma warning disable SA1202
#pragma warning disable SA1208
#pragma warning disable SA1210
#pragma warning disable SA1514

global using System;
global using Arc.Threading;
global using Arc.Unit;
global using CrystalData;
global using Microsoft.Extensions.DependencyInjection;
global using StandardWinUI;
global using Tinyhand;
using System.Globalization;
using System.Reflection;
using Arc.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StandardWinUI.Presentation;

namespace StandardWinUI;

// TODO: Rename 'StandardWinUI' and modify the app-specific constants, icons and images.
// Dependencies and data persistence: AppUnit.
// Presentation-State model: 5.Advanced is equipped with basic functionalities, it is recommended to use this as a template.

// App.GetService<T>() is used to retrieve a service of type T.
// AppSettings manages the application's settings.
// IBasicPresentationService.TryExit() attempts to exit the app, while App.Exit() exits the app without confirmation.
// NaviWindow_Closed() is called when the main window is closed.

/// <summary>
/// App class is an application-specific class.<br/>
/// It manages various application-specific information, such as language and settings.
/// </summary>
public class App
{
    public const string MutexName = "Arc.StandardWinUI"; // The name of the mutex used to prevent multiple instances of the application. Specify 'string.Empty' to allow multiple instances.
    public const string DataFolderName = "Arc\\StandardWinUI"; // The folder name for application data.
    public const string DefaultCulture = "en"; // The default culture for the application.
    public const double DefaultFontSize = 14; // The default font size for the application.

    #region FieldAndProperty

    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    public string Version { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the title of the application.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the folder path for application data.
    /// </summary>
    public string DataFolder { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the settings for the application.
    /// </summary>
    public AppSettings Settings { get; private set; } = default!;

    public DispatcherQueue UiDispatcherQueue { get; private set; } = default!;

    private readonly IServiceProvider serviceProvider;
    private readonly Crystalizer crystalizer;

    #endregion

    public App(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        this.crystalizer = this.GetService<Crystalizer>();
    }

    public void Initialize()
    {
        this.Version = Entrypoint.Version;
        this.Title = Entrypoint.Title;
        this.DataFolder = Entrypoint.DataFolder;
        this.UiDispatcherQueue = Entrypoint.UiDispatcherQueue;

        this.LoadStrings();
        this.LoadCrystalData();
        this.PrepareCulture();
        this.GetService<StandardApp>();
    }

    internal void LoadCrystalData()
    {
        this.crystalizer.PrepareAndLoadAll(false).Wait();
        this.Settings = this.crystalizer.GetCrystal<AppSettings>().Data;
    }

    /// <summary>
    /// Loads the localized strings for the application.
    /// </summary>
    internal void LoadStrings()
    {
        try
        {
            HashedString.SetDefaultCulture(DefaultCulture); // default culture
            LanguageList.Add("en", "Language.En");
            LanguageList.Add("ja", "Language.Ja");

            var asm = Assembly.GetExecutingAssembly();
            LanguageList.LoadHashedString(asm);
            HashedString.LoadAssembly("en", asm, "Resources.Strings.License.tinyhand"); // license
        }
        catch
        {
        }
    }

    /// <summary>
    /// Prepares the culture for the application.
    /// </summary>
    internal void PrepareCulture()
    {
        try
        {
            if (this.Settings.Culture == string.Empty)
            {
                this.Settings.Culture = "en"; // English
                if (CultureInfo.CurrentUICulture.Name == "ja-JP")
                {
                    this.Settings.Culture = "ja";
                }
            }

            HashedString.ChangeCulture(this.Settings.Culture);
        }
        catch
        {
            this.Settings.Culture = DefaultCulture;
            HashedString.ChangeCulture(this.Settings.Culture);
        }
    }

    public Window GetMainWindow()
        => this.GetService<NaviWindow>();

    /// <summary>
    /// Retrieves a service of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service instance.</returns>
    public T GetService<T>()
        where T : class
    {
        if (this.serviceProvider.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in Configure within AppUnit.cs.");
        }

        return service;
    }

    public T GetAndPrepareState<T>(FrameworkElement element)
        where T : class, IState
    {
        if (this.serviceProvider.GetService(typeof(T)) is not T state)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in Configure within AppUnit.cs.");
        }

        element.Loaded += (sender, e) => state.RestoreState();
        element.Unloaded += (sender, e) => state.StoreState();

        return state;
    }

    /// <summary>
    /// Handles the navigation event and retrieves the corresponding page from the service provider.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event data.</param>
    public void NavigatingHandler(object sender, NavigatingCancelEventArgs args)
    {
        if (args.SourcePageType is not null)
        {
            var page = this.serviceProvider.GetService(args.SourcePageType);
            if (page is not null)
            {
                args.Cancel = true;
                ((Frame)sender).Content = page;
            }
        }
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void Exit()
    {
        this.GetService<StandardApp>()?.Exit();
    }
}
