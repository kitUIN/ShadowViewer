namespace ShadowViewer.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// 为项目提交bug或者建议
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BugRequestCard_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("https://github.com/kitUIN/ShadowViewer");
            uri.LaunchUriAsync();
        }

        private void PluginSettingsStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
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
            foreach (string name in PluginHelper.Plugins)
            {
                var meta = PluginHelper.PluginInstances[name].MetaData();
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
                    IsOn = PluginHelper.EnabledPlugins.Contains(name),
                    Tag = name,
                };
                switchButton.Toggled += PluginToggleSwitch_Toggled;
                (expander.Content as StackPanel).Children.Add(switchButton);
                PluginHelper.PluginInstances[name].PluginSettingsExpander(expander);
                Log.ForContext<SettingsPage>().Information("[{name}]插件设置注入成功",
                    PluginHelper.PluginInstances[name].MetaData().Name);
                PluginSettingsStackPanel.Children.Add(expander);
            }
            LoadSettingsStackPanel();
        }
        /// <summary>
        /// 插件的启动与关闭事件
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PluginToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;
            var name = toggle.Tag.ToString();
            if (toggle.IsOn)
            {
                PluginHelper.PluginEnabled(name);
            }
            else
            {
                PluginHelper.PluginDisabled(name);
            }
            MessageHelper.SendNavigationReloadPlugin();
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
        /// <param name="panel">The panel.</param>
        public void LoadSettingsStackPanel()
        {
            foreach (SettingsExpander expander in PluginSettingsStackPanel.Children)
            {
                if (expander.Tag is string name)
                {
                    foreach (SettingsCard item in expander.Items)
                    {
                        item.IsEnabled = PluginHelper.EnabledPlugins.Contains(name);
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
            var selectedTheme = ((ComboBoxItem)((ComboBox)sender).SelectedItem)?.Tag?.ToString();
            var window = WindowHelper.GetWindowForElement(this);
            if (selectedTheme != null)
            {
                ThemeHelper.RootTheme = App.GetEnum<ElementTheme>(selectedTheme);
                UIHelper.AnnounceActionForAccessibility(sender as UIElement, $"Theme changed to {selectedTheme}",
                                                                                "ThemeChangedNotificationActivityId");
            }
        }

        private void SponsorCard_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("https://afdian.net/@kituin");
            uri.LaunchUriAsync();
        }
    }
}
