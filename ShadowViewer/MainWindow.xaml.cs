using System;
using System.Diagnostics;
using ShadowViewer.Core.Services;
using DryIoc;
using Microsoft.UI.Xaml;
using ShadowPluginLoader.WinUI;
using ShadowViewer.ViewModels;
using Serilog;
using ShadowViewer.Core.Cache;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Models;
using ShadowViewer.Core;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using ShadowViewer.Services;
using SqlSugar;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Pages;
using CommunityToolkit.WinUI.Animations;
using CustomExtensions.WinUI;
using ShadowViewer.Helpers;

namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; set; }
    private NavigationPage? navigationPage;
    private readonly Uri? firstUri;
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(Uri firstUri) : this()
    {
        this.firstUri = firstUri;
    }

    private void AppTitleBar_ThemeChangedEvent(object? sender, EventArgs e)
    {
        AppTitleBar.InvokeThemeChanged(this);
    }

    private async void Content_Loaded(object sender, RoutedEventArgs e)
    {
        await AnimationBuilder.Create()
            .Translation(
                from: new Vector2(0, 255),
                to: new Vector2(0, 0))
            .Opacity(
                from: 0,
                to: 1.0,
                duration: TimeSpan.FromSeconds(0.5))
            .StartAsync(LoadingGrid);
        await OnLoading();
        var caller = DiFactory.Services.Resolve<ICallableService>();
        caller.ThemeChangedEvent -= AppTitleBar_ThemeChangedEvent;
        caller.ThemeChangedEvent += AppTitleBar_ThemeChangedEvent;
        ViewModel = DiFactory.Services.Resolve<MainViewModel>();
        navigationPage = new NavigationPage();
        Grid.SetRow(navigationPage, 1);
        MainGrid.Children.Add(navigationPage);
        AppTitleBar.IsBackButtonVisible = true;
        AppTitleBar.IsPaneButtonVisible = true;
        AppTitleBar.IsHistoryButtonVisible = true;
        AppTitleBar.PaneButtonClick += navigationPage.AppTitleBar_OnPaneButtonClick;
        AppTitleBar.BackButtonClick += navigationPage.AppTitleBar_BackButtonClick;
        SuggestBox.Visibility = Visibility.Visible;
        NavigateHelper.ShadowNavigate(firstUri);
        await AnimationBuilder.Create()
            .Opacity(
                from: 1.0,
                to: 0,
                duration: TimeSpan.FromSeconds(0.5))
            .StartAsync(LoadingGrid);
        LoadingGrid.Visibility = Visibility.Collapsed;
    }

    private async Task OnLoading()
    {
        ApplicationExtensionHost.Initialize(Application.Current);
        // await Task.Delay(5000); // 测试用
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