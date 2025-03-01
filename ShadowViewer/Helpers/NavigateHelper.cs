using System;
using DryIoc;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Responders;
using ShadowViewer.Core.Services;
using ShadowViewer.Pages;

namespace ShadowViewer.Helpers;

public static class NavigateHelper
{
    /// <summary>
    /// 全局导航
    /// </summary>
    public static void ShadowNavigate(Uri uri)
    {
        if (uri.Scheme != "shadow") return;
        var navigationToolKit = DiFactory.Services.Resolve<ICallableService>();
        var urls = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        switch (uri.Host)
        {
            case "settings":
                navigationToolKit.NavigateTo(typeof(SettingsPage), null);
                return;
            default:
                if (ResponderHelper.GetEnabledResponder<INavigationResponder>(uri.Host) is { } responder)
                {
                    responder.Navigate(uri, urls);
                }
                break;
        }
    }
}