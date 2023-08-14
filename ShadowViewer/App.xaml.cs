﻿using CustomExtensions.WinUI;
using Serilog;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugins;
using SqlSugar;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            // 配置文件
            Config.Init();
            // 数据库
            InitDatabase();
            this.InitializeComponent();
            // 依赖注入
            ApplicationExtensionHost.Initialize(this);
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
            var startupWindow = new MainWindow();
            WindowHelper.TrackWindow(startupWindow);
            ThemeHelper.Initialize(startupWindow);
            
            // 插件依赖注入
            DiFactory.Services.Register<IPlugin,LocalPlugin>(reuse: Reuse.Singleton);
            var pluginServices = DiFactory.Services.Resolve<IPluginService>();
            pluginServices.Import<LocalPlugin>();
            //await pluginServices.ImportAsync();
            await pluginServices.ImportAsync(
                @"D:\VsProjects\WASDK\ShadowViewer.Plugin.Bika\bin\Debug\net6.0-windows10.0.19041.0\ShadowViewer.Plugin.Bika.dll");
            
            // 导航
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
          
    }
}
