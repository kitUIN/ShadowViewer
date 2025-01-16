using CustomExtensions.WinUI;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using System.Diagnostics;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            ApplicationExtensionHost.Initialize(this);
            // 配置文件
            Config.Init();
            InitDi();
            // 数据库
            InitDatabase();
            this.InitializeComponent();
        }

        private static void InitDi()
        {
            DiHelper.Init();
            DiFactory.Services.Register<MainViewModel>(reuse:Reuse.Singleton);
            DiFactory.Services.Register<SettingsViewModel>(reuse: Reuse.Singleton);
            DiFactory.Services.Register<NavigationViewModel>(reuse: Reuse.Singleton);
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
            // 插件依赖注入
            var pluginServices = DiFactory.Services.Resolve<PluginLoader>();

            var currentCulture = CultureInfo.CurrentUICulture;

            try
            {
                pluginServices.Import(typeof(LocalPlugin));
                //pluginServices.Import(typeof(PluginManagerPlugin));
                //await pluginServices.ImportFromDirAsync(Config.PluginsPath);
            }
            catch (Exception ex)
            {
                Log.Error("{E}", ex);
            }
            var startupWindow = new MainWindow();
            WindowHelper.TrackWindow(startupWindow);
            ThemeHelper.Initialize(startupWindow);
#if DEBUG
            // 这里是测试插件用的, ImportFromPathAsync里填入你Debug出来的插件dll的文件夹位置
            // await pluginServices.ImportFromPathAsync(@"C:\Users\15854\Documents\GitHub\ShadowViewer.Plugin.Bika\ShadowViewer.Plugin.Bika\bin\Debug\");
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
