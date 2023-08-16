using ShadowViewer.Plugins;

namespace ShadowViewer.Pages
{
    
    public sealed partial class PluginPage : Page
    {
        public IPluginService PluginService { get; }
        public PluginPage()
        {
            this.InitializeComponent();
            PluginService = DiFactory.Services.Resolve<IPluginService>();
        }
        
        /// <summary>
        /// 前往插件设置
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            if(button!=null&& button.Tag is string tag &&PluginService.GetPlugin(tag) is IPlugin plugin && plugin.SettingsPage != null)
            {
                this.Frame.Navigate(plugin.SettingsPage, null,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { Tag: IPlugin plugin })
            {
                var contentDialog= XamlHelper.CreateMessageDialog(XamlRoot,
                    ResourcesHelper.GetString(ResourceKey.DeletePlugin) + plugin.MetaData.Name,
                    ResourcesHelper.GetString(ResourceKey.DeletePluginMessage));
                contentDialog.IsPrimaryButtonEnabled = true;
                contentDialog.DefaultButton = ContentDialogButton.Close;
                contentDialog.PrimaryButtonText = ResourcesHelper.GetString(ResourceKey.Confirm) ;
                contentDialog.PrimaryButtonClick += async (dialog, args) =>
                {
                    var flag = await PluginService.DeleteAsync(plugin.MetaData.Id);
                    
                };
                await contentDialog.ShowAsync();
            }

            
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement source) return;
            if (sender is FrameworkElement { Tag: IPlugin plugin }   && !plugin.CanOpenFolder && !plugin.CanDelete)return;
            var flyout = FlyoutBase.GetAttachedFlyout(source);
            flyout?.ShowAt(source);

        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement { Tag: IPlugin plgin})
            {
                try
                {
                    var file = await plgin.GetType().Assembly.Location.GetFile();
                    var folder = await file.GetParentAsync();
                    folder.LaunchFolderAsync();
                }
                catch(Exception ex)
                {
                    Log.Error("打开文件夹错误{Ex}", ex);
                }
            }
        }

        private void More_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { Tag: IPlugin plugin } source && !plugin.CanOpenFolder && !plugin.CanDelete)
            {
                source.Visibility = Visibility.Collapsed;
                return;
            }
        }
    }
}
