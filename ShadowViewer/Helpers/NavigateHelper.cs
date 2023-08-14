
using DryIoc;
using Serilog.Core;
using ShadowViewer.Plugins;
using SqlSugar;

namespace ShadowViewer.Helpers
{
    public static class NavigateHelper
    {
        public static void ShadowNavigate(Uri uri)
        {
            if (uri.Scheme != "shadow") return;
            var db =  DiFactory.Services.Resolve<ISqlSugarClient>();
            var navigationToolKit = DiFactory.Services.Resolve<ICallableService>();
            var pluginService = DiFactory.Services.Resolve<IPluginService>();
            var urls = uri.AbsolutePath.Split(new char[] { '/',}, StringSplitOptions.RemoveEmptyEntries);
            // 本地
            switch (uri.Host)
            {
                case "local":
                    /*for (var i = 0; i < urls.Length; i++)
                    {
                        if (!db.Queryable<LocalComic>().Any(x => x.Id == urls[i])) 
                        {
                            var s = "shadow://local/" + string.Join("/", urls.Take(i + 1));
                            navigationToolKit.NavigateTo(NavigateMode.URL,null, urls[i - 1], new Uri(s));
                            return;
                        }
                    }*/
                    navigationToolKit.NavigateTo(typeof(BookShelfPage),uri); 
                    return; 
                case "settings":
                    navigationToolKit.NavigateTo(typeof(SettingsPage), null);
                    return;
                case "download":
                    navigationToolKit.NavigateTo( typeof(DownloadPage), null);
                    return;
                default:
                    if (pluginService.GetEnabledPlugin(uri.Host) is IPlugin plugin)
                    {
                        plugin.Navigate(uri, urls);
                    }
                    else
                    {
                        
                    }
                    break; 
            } 
            
        }
    }
}
