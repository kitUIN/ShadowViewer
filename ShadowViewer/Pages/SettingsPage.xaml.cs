using System;
using System.IO;
using Windows.Storage;
using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core;
using ShadowViewer.Core.Extensions;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Services;
using ShadowViewer.ViewModels;

namespace ShadowViewer.Pages;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel ViewModel { get; } = DiFactory.Services.Resolve<SettingsViewModel>();
    private PluginLoader PluginService { get; } = DiFactory.Services.Resolve<PluginLoader>();
    private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
    }
    private async void LogButton_Click(object sender, RoutedEventArgs e)
    {
        var defaultPath = ConfigHelper.IsPackaged
            ? ApplicationData.Current.LocalFolder.Path
            : Environment.CurrentDirectory;
        var folder =
            await StorageFolder.GetFolderFromPathAsync(Path.Combine(defaultPath,
                "Logs"));
        folder.LaunchFolderAsync();
    }


    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null || button.Tag.ToString() is not { } tag) return;
        var uri = new Uri(tag);
        uri.LaunchUriAsync();
    }

}