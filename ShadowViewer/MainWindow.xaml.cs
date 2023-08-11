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
        private async void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            var pluginServices = DiFactory.Services.Resolve<IPluginService>();
            await pluginServices.ImportAsync();
            await pluginServices.ImportAsync(
                @"D:\VsProjects\WASDK\ShadowViewer.Plugin.Bika\bin\Debug\net6.0-windows10.0.19041.0\ShadowViewer.Plugin.Bika.dll");
            pluginServices.InitAllPlugins();
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
