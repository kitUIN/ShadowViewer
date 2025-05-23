using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Xaml.Controls;
using ShadowPluginLoader.WinUI.Enums;
using Serilog;
using ShadowViewer.Core.Responders;
using ShadowViewer.Core.Models.Interfaces;
using ShadowViewer.Core.Services;
using ShadowViewer.Core;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Models;
using ShadowViewer.Core.Utils;
using ShadowViewer.Plugin.Local;

namespace ShadowViewer.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private ILogger Logger { get; }
    private ICallableService Caller { get; }
    private PluginLoader PluginService { get; }

    /// <summary />
    /// <param name="callableService"></param>
    /// <param name="pluginService"></param>
    /// <param name="logger"></param>
    public NavigationViewModel(ICallableService callableService, PluginLoader pluginService, ILogger logger)
    {
        Logger = logger;
        Caller = callableService;
        PluginService = pluginService;
    }

    /// <summary>
    /// 导航栏菜单
    /// </summary>
    public readonly ObservableCollection<IShadowNavigationItem> MenuItems = [];

    /// <summary>
    /// 导航栏底部菜单
    /// </summary>
    public readonly ObservableCollection<IShadowNavigationItem> FooterMenuItems = [];

    public ShadowNavigation? NavigationViewItemInvokedHandler(IShadowNavigationItem item)
    {
        var responder = ResponderHelper.GetEnabledResponder<INavigationResponder>(item.PluginId);
        return responder?.NavigationViewItemInvokedHandler(item);
    }

    /// <summary>
    /// 添加导航栏个体
    /// </summary>
    private void AddMenuItem(IShadowNavigationItem item)
    {
        if (MenuItems.All(x => x.Id != item.Id)) MenuItems.Add(item);
    }

    /// <summary>
    /// 添加导航栏个体
    /// </summary>
    private void DeleteMenuItem(IShadowNavigationItem item)
    {
        if (MenuItems.Any(x => x.Id == item.Id)) MenuItems.Remove(item);
    }

    /// <summary>
    /// 添加底部导航栏个体
    /// </summary>
    private void AddFooterMenuItems(IShadowNavigationItem item)
    {
        if (FooterMenuItems.All(x => x.Id != item.Id)) FooterMenuItems.Insert(0, item);
    }

    /// <summary>
    /// 删除底部导航栏个体
    /// </summary>
    private void DeleteFooterMenuItems(IShadowNavigationItem item)
    {
        if (FooterMenuItems.Any(x => x.Id == item.Id)) FooterMenuItems.Remove(item);
    }

    /// <summary>
    /// 重载导航栏
    /// </summary>
    public void InitItems()
    {
        foreach (var responder in ResponderHelper.GetResponders<INavigationResponder>())
        {
            if (PluginService.GetPlugin(responder.Id) is not { } plugin) continue;
            foreach (var item2 in responder.NavigationViewMenuItems)
                if (plugin.IsEnabled) AddMenuItem(item2);
                else DeleteMenuItem(item2);
            foreach (var item1 in responder.NavigationViewFooterItems)
                if (plugin.IsEnabled) AddFooterMenuItems(item1);
                else DeleteFooterMenuItems(item1);
        }
    }

    public void ReloadItems(string pluginId, PluginStatus status)
    {
        var responder = ResponderHelper.GetResponder<INavigationResponder>(pluginId);
        if (responder == null) return;
        switch (status)
        {
            case PluginStatus.Enabled:
            {
                foreach (var item2 in responder.NavigationViewMenuItems)
                    AddMenuItem(item2);
                foreach (var item1 in responder.NavigationViewFooterItems)
                    AddFooterMenuItems(item1);
                break;
            }
            case PluginStatus.Disabled:
            {
                foreach (var item2 in responder.NavigationViewMenuItems)
                    DeleteMenuItem(item2);
                foreach (var item1 in responder.NavigationViewFooterItems)
                    DeleteFooterMenuItems(item1);
                break;
            }
            default:
            {
                break;
            }
        }
    }
}