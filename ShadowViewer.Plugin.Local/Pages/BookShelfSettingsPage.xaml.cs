using DryIoc;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ShadowViewer.Interfaces;
using ShadowViewer.ViewModels;

namespace ShadowViewer.Plugin.Local.Pages;

public sealed partial class BookShelfSettingsPage : Page
{
    private SettingsViewModel ViewModel { get; }

    public BookShelfSettingsPage()
    {
        InitializeComponent();
        ViewModel = DiFactory.Services.Resolve<SettingsViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
    }
}