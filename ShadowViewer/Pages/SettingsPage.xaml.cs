using ShadowViewer.Plugins;

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
        ViewModel.InitPlugins();
        ViewModel.InitSettingsFolders();
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

    /// <summary>
    /// 前往插件设置
    /// </summary>
    private void PluginSetting_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string id } && PluginService.GetPlugin(id) is
            {
                SettingsPage: { } page
            })
            Frame.Navigate(page, null,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }
}