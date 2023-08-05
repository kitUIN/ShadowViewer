namespace ShadowViewer.Pages
{
    
    public sealed partial class PluginPage : Page
    {
        public IPluginsToolKit PluginsToolKit { get; }
        public PluginPage()
        {
            this.InitializeComponent();
            PluginsToolKit = DiFactory.Current.Services.GetService<IPluginsToolKit>();
        }
        /// <summary>
        /// «∞Õ˘≤Âº˛…Ë÷√
        /// </summary>
        private void PluginCard_Click(object sender, RoutedEventArgs e)
        {
            var source = sender as FrameworkElement;
            if (source != null && PluginsToolKit.GetPlugin(source.Tag.ToString()) is { } plugin)
            {
                this.Frame.Navigate(plugin.SettingsPage, null,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private void NoPluginInfoBar_Loaded(object sender, RoutedEventArgs e)
        {
            if(PluginsToolKit.Plugins.Count == 0)
            {
                NoPluginInfoBar.IsOpen = true;
            }
        }
    }
}
