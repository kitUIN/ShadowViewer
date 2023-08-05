using CustomExtensions.WinUI;
using Serilog;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugins;
using SqlSugar;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            // 依赖注入
            ApplicationExtensionHost.Initialize(this);
            DiFactory.Current = new DiFactory(); 
            // 配置文件
            Config.Init();
            // 数据库
            InitDatabase();
        }
        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static void InitDatabase()
        {
            var db = DiFactory.Current.Services.GetService<ISqlSugarClient>();
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
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            startupWindow = new MainWindow();
            startupWindow.ExtendsContentIntoTitleBar = true;
            WindowHelper.TrackWindow(startupWindow);
            ThemeHelper.Initialize(startupWindow);
            var firstUri = new Uri("shadow://local/");
            var actEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (actEventArgs.Kind == ExtendedActivationKind.Protocol
                && actEventArgs.Data is IProtocolActivatedEventArgs data)
            {
                firstUri = data.Uri;
            }
            startupWindow.Activate();
            NavigateHelper.ShadowNavigate(firstUri);
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
