using ShadowViewer.Interfaces;
using ShadowViewer.Plugins;

namespace ShadowViewer.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        private IPluginsToolKit PluginsTool{get;}
        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
            PluginsTool = DIFactory.Current.Services.GetService<IPluginsToolKit>();

        }
        private void PluginSettingsStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
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
            foreach (IPlugin plugin in PluginsTool.GetEnabledPlugins())
            {
                var meta = plugin.MetaData;
                var expander = new SettingsExpander
                {
                    Tag = meta.ID,
                    Header = meta.Name,
                    HeaderIcon = XamlHelper.CreateImageIcon(meta.Logo),
                    Description = meta.Description,
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock()
                            {
                                Text = meta.Version,
                                IsTextSelectionEnabled = true,
                                FontSize = 16,
                                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Margin = new Thickness(0, 0, 20, 0),
                            },
                        }
                    }
                };

                var switchButton = new ToggleSwitch()
                {
                    IsOn = PluginsTool.IsEnabled(meta.ID),
                    Tag = meta.ID,
                };
                switchButton.Toggled += PluginToggleSwitch_Toggled;
                (expander.Content as StackPanel).Children.Add(switchButton);
                plugin.PluginSettingsExpander(expander);
                Log.ForContext<SettingsPage>().Information("[{name}]插件设置注入成功",
                    plugin.MetaData.Name);
                PluginSettingsStackPanel.Children.Add(expander);
            }
            LoadSettingsStackPanel();
        }
        /// <summary>
        /// 插件的启动与关闭事件
        /// </summary>
        private void PluginToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;
            var id = toggle.Tag.ToString();
            if (toggle.IsOn)
            {
                PluginsTool.PluginEnabled(id);
            }
            else
            {
                PluginsTool.PluginDisabled(id);
            }
            //TODO MessageHelper.SendNavigationReloadPlugin();
            LoadSettingsStackPanel();
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
        /// <summary>
        /// 重载插件设置
        /// </summary>
        public void LoadSettingsStackPanel()
        {
            foreach (SettingsExpander expander in PluginSettingsStackPanel.Children.Cast<SettingsExpander>())
            {
                if (expander.Tag is string id)
                {
                    foreach (SettingsCard item in expander.Items.Cast<SettingsCard>())
                    {
                        item.IsEnabled = PluginsTool.IsEnabled(id);
                        if (item.Tag is bool arg)
                        {
                            item.IsEnabled = arg;
                        }
                    }
                }
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
            string accessToken = "TempPath";
            StorageFolder folder = await FileHelper.SelectFolderAsync(this, accessToken);
            if (folder != null)
            {
                ViewModel.TempPath = folder.Path;
            }
        }

        private async void ComicsPath_Click(object sender, RoutedEventArgs e)
        { 
            string accessToken = "ComicsPath";
            StorageFolder folder = await FileHelper.SelectFolderAsync(this, accessToken);
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
            this.Frame.Navigate(typeof(BookShelfSettingsPage) ,null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
