using System;
using System.Collections.ObjectModel;
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
using Microsoft.UI.Windowing;
using ShadowViewer.Core.Models.Interfaces;
using ShadowViewer.Helpers;
using ShadowViewer.Plugin.Local.Models;

namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get;  } = new();
    private NavigationPage? navigationPage;
    private readonly Uri? firstUri;

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
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
        await InAnimationLoadingGrid.StartAsync();
#if DEBUG
        var sw = new Stopwatch();
        sw.Start();
#endif
        await OnLoading(new Progress<string>(s => DispatcherQueue.TryEnqueue(() => LoadingText.Text = s)));
#if DEBUG
        sw.Stop();
        Debug.WriteLine("加载插件总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
#endif
        LoadingText.Text = "加载标题栏...";
        var caller = DiFactory.Services.Resolve<ICallableService>();
        ViewModel.PluginService = DiFactory.Services.Resolve<PluginLoader>();
        caller.ThemeChangedEvent -= AppTitleBar_ThemeChangedEvent;
        caller.ThemeChangedEvent += AppTitleBar_ThemeChangedEvent;
        navigationPage = new NavigationPage();
        Grid.SetRow(navigationPage, 1);
        MainGrid.Children.Add(navigationPage);
        AppTitleBar.IsBackButtonVisible = true;
        AppTitleBar.IsPaneButtonVisible = true;
        AppTitleBar.IsHistoryButtonVisible = true;
        AppTitleBar.PaneButtonClick += navigationPage.AppTitleBar_OnPaneButtonClick;
        AppTitleBar.BackButtonClick += navigationPage.AppTitleBar_BackButtonClick;
        // await OutAnimationLoadingGrid.StartAsync();
        LoadingGrid.Visibility = Visibility.Collapsed;
        MainGrid.Visibility = Visibility.Visible;
        SuggestBox.Visibility = Visibility.Visible;
        if (firstUri != null) NavigateHelper.ShadowNavigate(firstUri);
    }


    private async Task OnLoading(IProgress<string>? loadingProgress)
    {
        // await Task.Delay(5000);
        loadingProgress?.Report("初始化插件加载器...");
        ApplicationExtensionHost.Initialize(Application.Current);
        // await Task.Delay(5000); // 测试用
        Debug.WriteLine("123123");
        // 配置文件
        loadingProgress?.Report("加载配置文件与数据库...");
        CoreSettings.Init();
        InitDi();
        // 数据库
        InitDatabase();
        // 插件依赖注入

        var pluginServices = DiFactory.Services.Resolve<PluginLoader>();

        var currentCulture = CultureInfo.CurrentUICulture;

        loadingProgress?.Report("加载插件...");
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
        db.CodeFirst.InitTables<CacheZip>();
    }
    /// <summary>
    /// 历史记录显示
    /// </summary>
    public void HistoryFlyout_OnOpening(object? sender, object e)
    {
        ViewModel.ReLoadHistory();
    }
}