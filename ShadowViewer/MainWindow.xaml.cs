using DryIoc;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using ShadowPluginLoader.WinUI;
using ShadowPluginLoader.WinUI.Config;
using ShadowPluginLoader.WinUI.Extensions;
using ShadowViewer.Pages;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using ShadowViewer.Sdk;
using ShadowViewer.Sdk.Cache;
using ShadowViewer.Sdk.Configs;
using ShadowViewer.Sdk.Helpers;
using ShadowViewer.Sdk.Models;
using ShadowViewer.Sdk.Services;
using ShadowViewer.Services;
using ShadowViewer.ViewModels;
using Microsoft.EntityFrameworkCore;
using ShadowViewer.Sdk.Database;
using ShadowViewer.Plugin.Local.Database;
using Windows.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ShadowViewer;

public sealed partial class MainWindow
{
    private NavigationPage? navigationPage;
    private ShadowTitleBar? shadowTitleBar;
    private readonly Uri? firstUri;

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        Task.Run(async() => await Content_Loaded());
    }

    public MainWindow(Uri firstUri) : this()
    {
        this.firstUri = firstUri;
    }

    private async Task Content_Loaded()
    {
#if DEBUG
        var sw = new Stopwatch();
        sw.Start();
#endif
        try
        {
            await OnLoading();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Application initialization failed");
            DispatcherQueue.TryEnqueue(() =>
            {
                LoadingProgress.IsActive = false;
                LoadingText.Text = "初始化失败，应用未继续启动。请查看日志后重试。";
            });
            return;
        }
#if DEBUG
        sw.Stop();
        Debug.WriteLine("加载插件总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
#endif
        var caller = DiFactory.Services.Resolve<ICallableService>();
        DispatcherQueue.TryEnqueue(() =>
        {
            navigationPage = new NavigationPage();
            Grid.SetRow(navigationPage, 1);
            MainGrid.Children.Add(navigationPage);
            shadowTitleBar = new ShadowTitleBar(this);
            MainGrid.Children.Add(shadowTitleBar);
            shadowTitleBar.InitAppTitleBar_BackButtonClick(navigationPage.AppTitleBar_BackButtonClick);
            shadowTitleBar.InitAppTitleBar_OnPaneButtonClick(navigationPage.AppTitleBar_OnPaneButtonClick);
            caller.ThemeChangedEvent += shadowTitleBar.AppTitleBar_ThemeChangedEvent;

            MainGrid.Visibility = Visibility.Visible;
            LoadingGrid.Visibility = Visibility.Collapsed;
            var navigateService = DiFactory.Services.Resolve<INavigateService>();
            if (firstUri != null) navigateService.Navigate(firstUri);
            caller.AppLoaded();
        });

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task OnLoading()
    {
#if DEBUG
        var sw = new Stopwatch();
        sw.Start();
#endif
        var coreConfig = DiFactory.Services.Resolve<CoreConfig>();
        coreConfig.IsDebug = coreConfig.IsDebug;
        Log.Information($"Debug Mode: {coreConfig.IsDebug}");
        InitDi();
        // 数据库
        InitDatabase();
#if DEBUG
        sw.Stop();
        Debug.WriteLine("插件加载前总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
#endif

        // 插件依赖注入
        var pluginLoader = DiFactory.Services.Resolve<PluginLoader>();

        // var currentCulture = CultureInfo.CurrentUICulture;
        try
        {
            await pluginLoader.CheckUpgradeAndRemoveAsync();

            var session = pluginLoader.CreatePipeline()
                .Feed<LocalPlugin>()
                .Feed<PluginManagerPlugin>();

            // if (DiFactory.Services.Resolve<PluginManagerConfig>().PluginSecurityStatement)
            // {
            //     
            // }
            session.Feed(new DirectoryInfo(DiFactory.Services.Resolve<BaseSdkConfig>().PluginFolderPath));
#if DEBUG
            // 这里是测试插件用的, Scan里填入你Debug出来的插件dll的文件夹位置
            // session.Feed(new FileInfo(
            //     @"D:\VsProject\ShadowViewer.Plugin.Bika\ShadowViewer.Plugin.Bika\bin\Debug\net8.0-windows10.0.22621.0\ShadowViewer.Plugin.Bika\plugin.json"
            //     ));

#endif
            await session.ProcessAsync();
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
        DiFactory.Services.Register<IFilePickerService, FilePickerService>(reuse: Reuse.Singleton);

        DiFactory.Services.Register<SettingsViewModel>(reuse: Reuse.Singleton);
        DiFactory.Services.Register<NavigationViewModel>(reuse: Reuse.Singleton);
        DiFactory.Services.Register<TitleBarViewModel>(reuse: Reuse.Singleton);
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    private static void InitDatabase()
    {
        using var core = DiFactory.Services.Resolve<IDbContextFactory<ShadowDbContext>>().CreateDbContext();
        DatabaseUpgrade.Initialize(core, "__EFMigrationsHistory_Sdk");
        DatabaseRegistration.Register<LocalDbContext>(DiFactory.Services,
            Path.Combine(ApplicationData.Current.LocalFolder.Path, "ShadowViewer.sqlite"),
            "__EFMigrationsHistory_Local", options => new LocalDbContext(options));
        using var local = DiFactory.Services.Resolve<IDbContextFactory<LocalDbContext>>().CreateDbContext();
        DatabaseUpgrade.Initialize(local, "__EFMigrationsHistory_Local");
    }

    private void InAnimationLoadingGridOnLoaded(object sender, RoutedEventArgs e)
    {
        InAnimationLoadingGrid.Start();
    }
}
