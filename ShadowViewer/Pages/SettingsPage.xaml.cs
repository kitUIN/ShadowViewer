using ShadowViewer.Plugins;

namespace ShadowViewer.Pages
{
    public sealed partial class SettingsPage :  Page
    {
        public SettingsViewModel ViewModel { get; }
        public IPluginsToolKit PluginsToolKit { get; }
        private ICallableToolKit Caller { get; }
        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
            PluginsToolKit = DIFactory.Current.Services.GetService<IPluginsToolKit>();
            Caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            ElementTheme currentTheme = ThemeHelper.RootTheme;
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

            Caller.NavigateTo(NavigateMode.Type, e.SourcePageType, null, null);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

        }
        /// <summary>
        /// 插件的启动与关闭事件
        /// </summary>
        private void PluginToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;
            string id = toggle.Tag.ToString();
            if (toggle.IsOn)
            {
                PluginsToolKit.PluginEnabled(id);
            }
            else
            {
                PluginsToolKit.PluginDisabled(id);
            }
            //TODO 启动关闭事件MessageHelper.SendNavigationReloadPlugin();

        }
        /// <summary>
        /// 跳转到插件的WebUri
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PluginWebLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SettingsCard card && card.Tag is Uri webUri)
            {
                webUri.LaunchUriAsync();
            }
        }

        private async void LogButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs"));
            folder.LaunchFolderAsync();
        }
        private void ThemeModeSetting_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string selectedTheme = ((ComboBoxItem)((ComboBox)sender).SelectedItem)?.Tag?.ToString();
            Window window = WindowHelper.GetWindowForElement(this);
            if (selectedTheme != null)
            {
                ThemeHelper.RootTheme = EnumHelper.GetEnum<ElementTheme>(selectedTheme);
                UIHelper.AnnounceActionForAccessibility(sender as UIElement, $"Theme changed to {selectedTheme}",
                                                                                "ThemeChangedNotificationActivityId");
            }
        }

        private void Uri_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri((sender as SettingsCard).Tag.ToString());
            uri.LaunchUriAsync();
        }

        private async void TempPath_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await FileHelper.SelectFolderAsync(this, "TempPath");
            if (folder != null)
            {
                ViewModel.TempPath = folder.Path;
            }
        }

        private async void ComicsPath_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await FileHelper.SelectFolderAsync(this, "ComicsPath");
            if (folder != null)
            {
                ViewModel.ComicsPath = folder.Path;
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var uri = new Uri(button.Tag.ToString());
            uri.LaunchUriAsync();
        }

        private void HomeSettingCard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Pages.Add(new BreadcrumbItem("Book", typeof(BookShelfSettingsPage)));
            this.Frame.Navigate(typeof(BookShelfSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void PlutinCard_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsToolKit.GetPlugin((sender as FrameworkElement).Tag.ToString()) is IPlugin plugin)
            {
                this.Frame.Navigate(plugin.SettingsPage, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
