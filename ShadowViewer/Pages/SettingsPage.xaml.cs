using System;
using System.IO;
using Windows.Storage;
using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Sdk.Configs;
using ShadowViewer.Sdk.Extensions;
using ShadowViewer.Sdk.Helpers;
using ShadowViewer.ViewModels;

namespace ShadowViewer.Pages;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel ViewModel { get; } = DiFactory.Services.Resolve<SettingsViewModel>();
    private CoreConfig CoreConfig { get; } = DiFactory.Services.Resolve<CoreConfig>();

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // var sw = Stopwatch.StartNew();
        // ViewModel ??= DiFactory.Services.Resolve<SettingsViewModel>();
        // sw.Stop();
        // Debug.WriteLine($"ViewModel create:{sw.ElapsedMilliseconds}ms");
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
    }

    private async void LogButton_Click(object sender, RoutedEventArgs e)
    {
        await CoreConfig.LaunchLogFolderAsync();
    }
}