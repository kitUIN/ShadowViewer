using System;
using System.IO;
using Windows.Storage;
using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core.Extensions;
using ShadowViewer.Core.Helpers;
using ShadowViewer.ViewModels;

namespace ShadowViewer.Pages;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel ViewModel { get;   } = DiFactory.Services.Resolve<SettingsViewModel>();

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
        var defaultPath = ConfigHelper.IsPackaged
            ? ApplicationData.Current.LocalFolder.Path
            : Environment.CurrentDirectory;
        var folder =
            await StorageFolder.GetFolderFromPathAsync(Path.Combine(defaultPath,
                "Logs"));
        folder.LaunchFolderAsync();
    }
}