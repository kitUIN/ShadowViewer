using System;
using System.Globalization;
using Windows.ApplicationModel.Activation;
using CustomExtensions.WinUI;
using DryIoc;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core;
using ShadowViewer.Core.Cache;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Models;
using ShadowViewer.Core.Services;
using ShadowViewer.Helpers;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using ShadowViewer.Services;
using ShadowViewer.ViewModels;
using SqlSugar;
using Microsoft.UI.Xaml;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            ApplicationExtensionHost.Initialize(this);
            // 配置文件
            CoreSettings.Init();
            InitDi();
            // 数据库
            InitDatabase();
            this.InitializeComponent();
        }

        private static void InitDi()
        {
            DiHelper.Init();
            DiFactory.Services.Register<INotifyService, NotifyService>(reuse: Reuse.Singleton);
            DiFactory.Services.Register<ICallableService, CallableService>(reuse: Reuse.Singleton);
            
            DiFactory.Services.Register<MainViewModel>(reuse:Reuse.Singleton);
            DiFactory.Services.Register<SettingsViewModel>(reuse: Reuse.Singleton);
            DiFactory.Services.Register<NavigationViewModel>(reuse: Reuse.Singleton);
        }
        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static void InitDatabase()
        {
            SnowFlakeSingle.WorkId = 4;
            var db = DiFactory.Services.Resolve<ISqlSugarClient>();
            db.DbMaintenance.CreateDatabase();
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
                pluginServices.Import(typeof(PluginManagerPlugin));
                await pluginServices.ImportFromDirAsync(CoreSettings.PluginsPath);
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
            var firstUri = new Uri("shadow://local/bookshelf");
            var actEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
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
