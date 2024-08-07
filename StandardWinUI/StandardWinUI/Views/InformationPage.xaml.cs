// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using StandardWinUI.ViewModels;

namespace StandardWinUI.Views;

public sealed partial class InformationPage : Page
{
    private const string LicenseUri = "https://opensource.org/licenses/MIT";

    public InformationPage()
    {
        this.InitializeComponent();
        this.viewModel = App.GetService<InformationViewModel>();

        var titleRun = new Run();
        titleRun.Text = App.Title;

        var copyrightRun = new Run();
        copyrightRun.Text = "  Copyright (c) 2024 archi-Doc\nReleased under the MIT license\n";

        var hyperlink = new Hyperlink();
        hyperlink.NavigateUri = new Uri(LicenseUri);
        hyperlink.Inlines.Add(new Run() { Text = LicenseUri, });
        hyperlink.Click += (s, e) =>
        {
            try
            {
                Arc.WinUI.Helper.OpenBrowser(hyperlink.NavigateUri.ToString());
            }
            catch
            {
            }
        };

        this.textBlock.Inlines.Add(titleRun);
        this.textBlock.Inlines.Add(copyrightRun);
        this.textBlock.Inlines.Add(hyperlink);

        // License
        this.AddLicense("License.CommunityToolkit", "Community Toolkit");
    }

    private readonly InformationViewModel viewModel;

    private void nvSample5_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
    }

    private void AddLicense(string key, string title)
    {
        /*var license = HashedString.GetOrEmpty(key);
        if (string.IsNullOrEmpty(license))
        {
            return;
        }*/

        var item = new NavigationViewItem()
        {
            Content = title,
            Tag = key,
        };

        this.navigationView.MenuItems.Add(item);
    }
}
