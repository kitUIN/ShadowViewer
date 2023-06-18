using Serilog;
using ShadowViewer.Plugin.Bika;
using ShadowViewer.Plugins;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            Config.Init();
            // 文件创建
            _ = ApplicationData.Current.LocalFolder.CreateFileAsync("ShadowViewer.db");
            // 数据库
            DBHelper.Init();
            // 插件
            // PluginHelper.Init();
            // 标签数据
            TagsHelper.Init();
            var bika = DIFactory.Current.Services.GetService<IPlugin>();
            Log.Information(bika.MetaData().ID);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            startupWindow = new MainWindow();
            startupWindow.ExtendsContentIntoTitleBar = true;
            WindowHelper.TrackWindow(startupWindow);
            ThemeHelper.Initialize();
            Uri firstUri = new Uri("shadow://local/");
            AppActivationArguments actEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (actEventArgs.Kind == ExtendedActivationKind.Protocol
                && actEventArgs.Data is IProtocolActivatedEventArgs data && data != null)
            {
                firstUri = data.Uri;
            }
            NavigateHelper.ShadowNavigate(firstUri);
            startupWindow.Activate();
        }
        private static Window startupWindow;
        public static Window StartupWindow
        {
            get
            {
                return startupWindow;
            }
        }
    }
}
