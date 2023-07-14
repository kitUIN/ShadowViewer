using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ShadowViewer.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShadowViewer.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }
        public IPluginsToolKit PluginsToolKit { get; }
        public MainSettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
            PluginsToolKit = DIFactory.Current.Services.GetService<IPluginsToolKit>();
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
            ViewModel.Pages = new ObservableCollection<BreadcrumbItem> {
                new BreadcrumbItem(ResourcesHelper.GetString(ResourceKey.Settings), typeof(MainSettingsPage))
            };
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
            ViewModel.Pages.Add(new BreadcrumbItem("Book", typeof(BookShelfSettingsPage)));
            this.Frame.Navigate(typeof(BookShelfSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void PlutinCard_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsToolKit.GetPlugin((sender as FrameworkElement).Tag.ToString()) is IPlugin plugin)
            {
                ViewModel.Pages.Add(new BreadcrumbItem(plugin.MetaData.Name, plugin.SettingsPage));
                this.Frame.Navigate(plugin.SettingsPage, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
