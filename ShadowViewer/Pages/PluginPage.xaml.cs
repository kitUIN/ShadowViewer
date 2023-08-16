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

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            var flyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
            flyout?.ShowAt((FrameworkElement)sender);
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement { Tag: IPlugin plgin})
            {
                try
                {
                    var file = await plgin.GetType().Assembly.Location.GetFile();
                    var folder =await file.GetParentAsync();
                    folder.LaunchFolderAsync();
                }
                catch(Exception ex)
                {
                    Log.Error("打开文件夹错误{Ex}", ex);
                }
            }
        }
    }
}
