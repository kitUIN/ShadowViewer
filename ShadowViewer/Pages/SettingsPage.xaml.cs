using ShadowViewer.Plugins;
using ShadowViewer.Services.Interfaces;

namespace ShadowViewer.Pages;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel ViewModel { get; } = DiFactory.Services.Resolve<SettingsViewModel>();
    private IPluginService PluginService { get; } = DiFactory.Services.Resolve<IPluginService>();
    private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
    public bool IsUnPackaged = !ConfigHelper.IsPackaged;
    public SettingsPage()
    {
        InitializeComponent();
        var currentTheme = ThemeHelper.RootTheme;
        switch (currentTheme)
        {
            case ElementTheme.Light:
                ThemeModeSetting.SelectedIndex = 0;
                break;
            case ElementTheme.Dark:
                ThemeModeSetting.SelectedIndex = 1;
                break;
            case ElementTheme.Default:
                ThemeModeSetting.SelectedIndex = 2;
                break;
        }
        
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.InitPlugins();
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

    private void ThemeModeSetting_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var selectedTheme = ((ComboBoxItem)((ComboBox)sender).SelectedItem)?.Tag?.ToString();
        if (selectedTheme == null) return;
        ThemeHelper.RootTheme = EnumHelper.GetEnum<ElementTheme>(selectedTheme);
        UIHelper.AnnounceActionForAccessibility((UIElement)sender, $"Theme changed to {selectedTheme}",
            "ThemeChangedNotificationActivityId");
    }

    private void Uri_Click(object sender, RoutedEventArgs e)
    {
        var source = sender as FrameworkElement;
        if (source == null || source.Tag.ToString() is not { } tag) return;
        var uri = new Uri(tag);
        uri.LaunchUriAsync();
    }

    private async void TempPath_Click(object sender, RoutedEventArgs e)
    {
        var folder = await FileHelper.SelectFolderAsync(this, "TempPath");
        ViewModel.TempPath = folder.Path;
    }

    private async void ComicsPath_Click(object sender, RoutedEventArgs e)
    {
        var folder = await FileHelper.SelectFolderAsync(this, "ComicsPath");
        ViewModel.ComicsPath = folder.Path;
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