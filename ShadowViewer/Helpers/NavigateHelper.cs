using ShadowViewer.Responders;

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
        var responderService = DiFactory.Services.Resolve<ResponderService>();
        var urls = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        switch (uri.Host)
        {
            case "settings":
                navigationToolKit.NavigateTo(typeof(SettingsPage), null);
                return;
            default:
                if (responderService.GetEnabledResponder<INavigationResponder>(uri.Host) is { } responder)
                {
                    responder.Navigate(uri, urls);
                }
                break;
        }
    }
}