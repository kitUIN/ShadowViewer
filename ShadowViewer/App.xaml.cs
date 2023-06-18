﻿using Serilog;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugin.Bika;
using ShadowViewer.Plugins;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            DIFactory.Current = new DIFactory();
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
            startupWindow.Activate();
            ShadowNavigate(firstUri);
        }
        public void ShadowNavigate(Uri uri)
        {
            // 本应用协议
            if (uri.Scheme == "shadow")
            {
                INavigationToolKit _navigationToolKit = DIFactory.Current.Services.GetService<INavigationToolKit>();
                string[] urls = uri.AbsolutePath.Split(new char[] { '/', }, StringSplitOptions.RemoveEmptyEntries);
                // 本地
                switch (uri.Host.ToLower())
                {
                    case "local":
                        if (urls.Length == 0)
                        {
                            _navigationToolKit.NavigateToPage(Enums.NavigateMode.Page, typeof(BookShelfPage), null, uri);
                            return;
                        }
                        for (int i = 0; i < urls.Length; i++)
                        {
                            if (!DBHelper.Db.Queryable<LocalComic>().Any(x => x.Id == urls[i]))
                            {
                                string s = "shadow://local/" + string.Join("/", urls.Take(i + 1));
                                _navigationToolKit.NavigateToPage(Enums.NavigateMode.URL, null, urls[i - 1], new Uri(s));
                                return;
                            }
                        }
                        _navigationToolKit.NavigateToPage(Enums.NavigateMode.URL, null, urls.Last(), uri);
                        break;
                    case "settings":
                        _navigationToolKit.NavigateToPage(Enums.NavigateMode.Page, typeof(SettingsPage), null, null);
                        break;
                    case "download":
                        break;
                    default:
                        //TODO: 插件注入
                        break;
                }
            }
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
