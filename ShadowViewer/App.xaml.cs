using CustomExtensions.WinUI;
using Serilog;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugins;
using SqlSugar;
using System.Globalization;
using ShadowViewer.Plugin.Local.ViewModels;
using ShadowViewer.Responders;
using System.Diagnostics;
using ShadowViewer.Services.Interfaces;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            ApplicationExtensionHost.Initialize(this);
            InitDi();
            // 配置文件
            Config.Init();
            // 数据库
            InitDatabase();
            this.InitializeComponent();
        }

        private static void InitDi()
        {
            DiFactory.Services.Register<MainViewModel>(reuse:Reuse.Singleton);
        }
        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static void InitDatabase()
        {
            var db = DiFactory.Services.Resolve<ISqlSugarClient>();
            db.DbMaintenance.CreateDatabase();
            db.CodeFirst.InitTables<LocalComic>();
            db.CodeFirst.InitTables<LocalEpisode>();
            db.CodeFirst.InitTables<LocalPicture>();
            db.CodeFirst.InitTables<LocalTag>();
            db.CodeFirst.InitTables<CacheImg>();
            db.CodeFirst.InitTables<CacheZip>();
        }
        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var startupWindow = new MainWindow();
            WindowHelper.TrackWindow(startupWindow);
            ThemeHelper.Initialize(startupWindow);
            // 插件依赖注入
            var pluginServices = DiFactory.Services.Resolve<IPluginService>();
            pluginServices.ImportOnePlugin<LocalPlugin>();
            try
            {
                await pluginServices.ImportFromPluginsPathAsync();
            }
            catch(Exception ex)
            {
                Log.Error("{E}", ex);
            }
#if DEBUG
            // 这里是测试插件用的, ImportAsync里填入你Debug出来的插件dll位置
            // await pluginServices.ImportAsync(@"C:\Users\15854\Documents\GitHub\ShadowViewer.Plugin.Bika\ShadowViewer.Plugin.Bika\bin\Debug\net6.0-windows10.0.19041.0\ShadowViewer.Plugin.Bika.dll");
#endif
            // 导航
            var firstUri = new Uri("shadow://Local/");
            var actEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (actEventArgs.Kind == ExtendedActivationKind.Protocol
                && actEventArgs.Data is IProtocolActivatedEventArgs data)
            {
                firstUri = data.Uri;
            }
            startupWindow.Activate();
            NavigateHelper.ShadowNavigate(firstUri);
        }
    }
}
