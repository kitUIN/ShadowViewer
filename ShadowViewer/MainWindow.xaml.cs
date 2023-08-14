using System.Diagnostics;

namespace ShadowViewer
{
    public sealed partial class MainWindow : Window
    {
        private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
        public MainWindow()
        {
            this.InitializeComponent();
            AppTitleBar.Window = this;
            //this.Title = "ShadowViewer";
            AppTitleBar.Subtitle = Config.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
            Caller.DebugEvent += MainWindow_DebugEvent;
        }
        /// <summary>
        /// ³õÊ¼»¯²å¼þ
        /// </summary>
        private   void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        private void MainWindow_DebugEvent(object sender, EventArgs e)
        {
            AppTitleBar.Subtitle = Config.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
        }

        private void AppTitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            Caller.NavigationViewBackRequested(sender);
        }

        private void AppTitleBar_OnPaneButtonClick(object sender, RoutedEventArgs e)
        {
            Caller.NavigationViewPane(sender);
        }
    }
}
