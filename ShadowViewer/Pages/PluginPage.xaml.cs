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
        
        private void NoPluginInfoBar_Loaded(object sender, RoutedEventArgs e)
        {
            if(PluginService.Plugins.Count == 0)
            {
                NoPluginInfoBar.IsOpen = true;
            }
        }
        /// <summary>
        /// «∞Õ˘≤Âº˛…Ë÷√
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            if(button!=null&& button.Tag is string tag && PluginService.GetPlugin(tag) is IPlugin plugin)
            {
                this.Frame.Navigate(plugin.SettingsPage, null,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
