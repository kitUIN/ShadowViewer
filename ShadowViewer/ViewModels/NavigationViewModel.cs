using ShadowViewer.Responders;
using System.Diagnostics;

namespace ShadowViewer.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private ILogger Logger { get; }
    private ICallableService Caller { get; }
    private PluginLoader PluginService { get; }
    private ResponderService ResponderService { get; }

    /// <summary />
    /// <param name="callableService"></param>
    /// <param name="pluginService"></param>
    /// <param name="logger"></param>
    /// <param name="responderService"></param>
    public NavigationViewModel(ICallableService callableService, PluginLoader pluginService, ILogger logger,
        ResponderService responderService)
    {
        Logger = logger;
        Caller = callableService;
        PluginService = pluginService;
        ResponderService = responderService;
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
        var responder = ResponderService.GetEnabledResponder<INavigationResponder>(item.PluginId);
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
        if (MenuItems.FirstOrDefault(x => x.Id == item.Id) is { } i) MenuItems.Remove(i);
    }

    /// <summary>
    /// 添加底部导航栏个体
    /// </summary>
    private void AddFooterMenuItems(IShadowNavigationItem item)
    {
        if (FooterMenuItems.All(x => x.Id != item.Id)) FooterMenuItems.Add(item);
    }

    /// <summary>
    /// 删除底部导航栏个体
    /// </summary>
    private void DeleteFooterMenuItems(IShadowNavigationItem item)
    {
        if (FooterMenuItems.FirstOrDefault(x => x.Id == item.Id) is { } i) FooterMenuItems.Remove(i);
    }

    /// <summary>
    /// 重载导航栏
    /// </summary>
    public void ReloadItems()
    {
        Logger.Information(ResponderService.GetResponders<INavigationResponder>().Count().ToString());
        foreach (var responder in ResponderService.GetResponders<INavigationResponder>())
        {
            if (PluginService.GetPlugin(responder.Id) is not { } plugin) continue;
            foreach (var item2 in responder.NavigationViewMenuItems)
                if (plugin.IsEnabled)
                    AddMenuItem(item2);
                else
                    DeleteMenuItem(item2);
            foreach (var item1 in responder.NavigationViewFooterItems)
                if (plugin.IsEnabled)
                    AddFooterMenuItems(item1);
                else
                    DeleteFooterMenuItems(item1);
        }
    }
}