using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CustomExtensions.WinUI;
using ShadowViewer.Core;
using DryIoc;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core.Cache;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Models;
using ShadowViewer.Core.Services;
using ShadowViewer.Services;
using ShadowViewer.ViewModels;
using SqlSugar;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using ShadowViewer.Helpers;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Microsoft.UI.Dispatching;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShadowViewer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadingWindow : WinUIEx.SplashScreen
    {
        public LoadingWindow(Type window) : base(window)
        {
            this.InitializeComponent();
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
        }
         
        protected override async Task OnLoading()
        {
            await Task.Delay(5000);
            Debug.WriteLine("123123");
            // 配置文件
            CoreSettings.Init();
            InitDi();
            // 数据库
            InitDatabase();
            // 插件依赖注入

            var pluginServices = DiFactory.Services.Resolve<PluginLoader>();

            var currentCulture = CultureInfo.CurrentUICulture;

            try
            {
                pluginServices.Import(typeof(LocalPlugin));
                pluginServices.Import(typeof(PluginManagerPlugin));
                await pluginServices.ImportFromDirAsync(CoreSettings.PluginsPath);
#if DEBUG
                // 这里是测试插件用的, ImportFromPathAsync里填入你Debug出来的插件dll的文件夹位置
                // await pluginServices.ImportFromPathAsync(@"C:\Users\15854\Documents\GitHub\ShadowViewer.Plugin.Bika\ShadowViewer.Plugin.Bika\bin\Debug\");
#endif
            }
            catch (Exception ex)
            {
                Log.Error("{E}", ex);
            }
        }

        private static void InitDi()
        {
            DiHelper.Init();
            DiFactory.Services.Register<INotifyService, NotifyService>(reuse: Reuse.Singleton);
            DiFactory.Services.Register<ICallableService, CallableService>(reuse: Reuse.Singleton);

            DiFactory.Services.Register<MainViewModel>(reuse: Reuse.Singleton);
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
    }
}