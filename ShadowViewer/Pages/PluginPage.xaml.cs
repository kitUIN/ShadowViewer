using ShadowViewer.Plugins;

namespace ShadowViewer.Pages
{
    
    public sealed partial class PluginPage : Page
    {
        public IPluginsToolKit PluginsToolKit { get; }
        public PluginPage()
        {
            this.InitializeComponent();
            PluginsToolKit = DiFactory.Services.Resolve<IPluginsToolKit>();
        }
        
        private void NoPluginInfoBar_Loaded(object sender, RoutedEventArgs e)
        {
            if(PluginsToolKit.Plugins.Count == 0)
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
            if(button!=null&& button.Tag is string tag && PluginsToolKit.GetPlugin(tag) is IPlugin plugin)
            {
                this.Frame.Navigate(plugin.SettingsPage, null,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
