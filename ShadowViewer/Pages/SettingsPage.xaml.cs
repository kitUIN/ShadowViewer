namespace ShadowViewer.Pages
{
    public sealed partial class SettingsPage : Page
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
            //TODO �����ر��¼�MessageHelper.SendNavigationReloadPlugin();
        }

        
        private void PluginWebLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SettingsCard { Tag: Uri webUri })
            {
                webUri.LaunchUriAsync();
            }
        }

        private async void LogButton_Click(object sender, RoutedEventArgs e)
        {
            var folder =
                await StorageFolder.GetFolderFromPathAsync(Path.Combine(ApplicationData.Current.LocalFolder.Path,
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
            if (folder != null)
            {
                ViewModel.TempPath = folder.Path;
            }
        }

        private async void ComicsPath_Click(object sender, RoutedEventArgs e)
        {
            var folder = await FileHelper.SelectFolderAsync(this, "ComicsPath");
            if (folder != null)
            {
                ViewModel.ComicsPath = folder.Path;
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null || button.Tag.ToString() is not { } tag) return;
            var uri = new Uri(tag);
            uri.LaunchUriAsync();
        }

        private void HomeSettingCard_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BookShelfSettingsPage), null,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void PluginCard_Click(object sender, RoutedEventArgs e)
        {
            var source = sender as FrameworkElement;
            if (source != null && PluginsToolKit.GetPlugin(source.Tag.ToString()) is { } plugin)
            {
                this.Frame.Navigate(plugin.SettingsPage, null,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}