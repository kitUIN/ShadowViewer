using DryIoc;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core;
using ShadowViewer.Core.Cache;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Models;
using ShadowViewer.Core.Services;
using ShadowViewer.Core.Settings;
using ShadowViewer.Pages;
using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using ShadowViewer.Services;
using ShadowViewer.ViewModels;
using SqlSugar;
using System;
using System.Collections.Generic;
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
    }

    public MainWindow(Uri firstUri) : this()
    {
        this.firstUri = firstUri;
    }


    private async void Content_Loaded(object sender, RoutedEventArgs e)
    {
        await InAnimationLoadingGrid.StartAsync();
#if DEBUG
        var sw = new Stopwatch();
        sw.Start();
#endif
        await OnLoading();
#if DEBUG
        sw.Stop();
        Debug.WriteLine("加载插件总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
#endif
        LoadingText.Text = "加载标题栏...";
        var caller = DiFactory.Services.Resolve<ICallableService>();
        navigationPage = new NavigationPage();
        Grid.SetRow(navigationPage, 1);
        MainGrid.Children.Add(navigationPage);
        shadowTitleBar = new ShadowTitleBar(this);
        MainGrid.Children.Add(shadowTitleBar);
        shadowTitleBar.InitAppTitleBar_BackButtonClick(navigationPage.AppTitleBar_BackButtonClick);
        shadowTitleBar.InitAppTitleBar_OnPaneButtonClick(navigationPage.AppTitleBar_OnPaneButtonClick);
        caller.ThemeChangedEvent += shadowTitleBar.AppTitleBar_ThemeChangedEvent;
        caller.DebugEvent += shadowTitleBar.AppTitleBar_DebugEvent;
        // await OutAnimationLoadingGrid.StartAsync();
        MainGrid.Visibility = Visibility.Visible;
        LoadingGrid.Visibility = Visibility.Collapsed;
        var navigateService = DiFactory.Services.Resolve<INavigateService>();
        if (firstUri != null) navigateService.Navigate(firstUri);
        caller.AppLoaded();

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
        Debug.WriteLine(CoreSettings.Instance.IsDebug);
        InitDi();
        // 数据库
        InitDatabase();
#if DEBUG
        sw.Stop();
        Debug.WriteLine("插件加载前总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
#endif

        // 插件依赖注入
        var pluginServices = DiFactory.Services.Resolve<PluginLoader>();

        // var currentCulture = CultureInfo.CurrentUICulture;
        try
        {
            await pluginServices.CheckUpgradeAndRemoveAsync();
            await pluginServices
                .Scan<LocalPlugin>()
                .Scan<PluginManagerPlugin>()
                .Load();
            if (PluginManagerPlugin.Settings.PluginSecurityStatement)
            {
                pluginServices.Scan(new DirectoryInfo(CoreSettings.Instance.PluginsPath));
                
            }
#if DEBUG
            // 这里是测试插件用的, Scan里填入你Debug出来的插件dll的文件夹位置
            pluginServices.Scan(new FileInfo(
                @"D:\VsProject\ShadowViewer.Plugin.Bika\ShadowViewer.Plugin.Bika\bin\Debug\net8.0-windows10.0.22621\ShadowViewer.Plugin.Bika\Assets\plugin.json"
                ));

#endif
            await pluginServices.Load();
        }
        catch (Exception ex)
        {
            Log.Error("{E}", ex);
        }

        // 添加类别标签
        _ = Task.Run(async () =>
        {
            var db = DiFactory.Services.Resolve<ISqlSugarClient>();
            var insertTags = new List<ShadowTag>();
            var updateTags = new List<ShadowTag>();
            foreach (var plugin in pluginServices.GetPlugins())
            {
                if (plugin.MetaData.AffiliationTag?.Name == null) continue;
                var tagId = await db.Queryable<ShadowTag>().Where(x =>
                    x.PluginId == plugin.Id && x.TagType == 0).Select(it => it.Id).ToListAsync();
                if (tagId is { Count: > 0 })
                {
                    plugin.MetaData.AffiliationTag.Id = tagId[0];
                    updateTags.Add(plugin.MetaData.AffiliationTag);
                }
                else
                {
                    insertTags.Add(plugin.MetaData.AffiliationTag);
                }
            }

            if (insertTags.Count != 0) await db.Insertable(insertTags).ExecuteReturnSnowflakeIdListAsync();
            if (updateTags.Count != 0) await db.Updateable(updateTags).ExecuteCommandAsync();
        });
    }

    private static void InitDi()
    {
        DiHelper.Init();
        DiFactory.Services.Register<INotifyService, NotifyService>(reuse: Reuse.Singleton);
        DiFactory.Services.Register<ICallableService, CallableService>(reuse: Reuse.Singleton);

        DiFactory.Services.Register<SettingsViewModel>(reuse: Reuse.Singleton);
        DiFactory.Services.Register<NavigationViewModel>(reuse: Reuse.Singleton);
        DiFactory.Services.Register<TitleBarViewModel>(reuse: Reuse.Singleton);
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    private static void InitDatabase()
    {
        SnowFlakeSingle.WorkId = 4;
        var db = DiFactory.Services.Resolve<ISqlSugarClient>();
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables<ShadowTag>();
        db.CodeFirst.InitTables<CacheZip>();
    }
}