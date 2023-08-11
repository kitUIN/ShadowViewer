
using DryIoc;
using Serilog.Core;
using SqlSugar;

namespace ShadowViewer.Helpers
{
    public static class NavigateHelper
    {
        public static void ShadowNavigate(Uri uri)
        {

            // 本应用协议
            if (uri.Scheme == "shadow")
            {
                var db =  DiFactory.Services.Resolve<ISqlSugarClient>();
                var _navigationToolKit = DiFactory.Services.Resolve<ICallableService>();
                string[] urls = uri.AbsolutePath.Split(new char[] { '/',}, StringSplitOptions.RemoveEmptyEntries);
                // 本地
                switch (uri.Host)
                {
                    case "local":
                        if (urls.Length == 0) 
                        {
                            _navigationToolKit.NavigateTo(NavigateMode.Page,typeof(BookShelfPage),null, uri); 
                            return; 
                        }
                        for (var i = 0; i < urls.Length; i++)
                        {
                            if (!db.Queryable<LocalComic>().Any(x => x.Id == urls[i])) 
                            {
                                var s = "shadow://local/" + string.Join("/", urls.Take(i + 1));
                                _navigationToolKit.NavigateTo(NavigateMode.URL,null, urls[i - 1], new Uri(s));
                                return;
                            }
                        }
                        _navigationToolKit.NavigateTo(NavigateMode.URL, null, urls.Last(), uri);
                        return;
                    case "settings":
                        _navigationToolKit.NavigateTo(NavigateMode.Page, typeof(SettingsPage), null, null);
                        return;
                    case "download":
                        _navigationToolKit.NavigateTo(NavigateMode.Page, typeof(DownloadPage), null, null);
                        return;
                    default:
                        //TODO: 插件注入
                        break; 
                } 
                _navigationToolKit.NavigateTo(NavigateMode.Page,typeof(BookShelfPage),null, uri); 
            }
        }
    }
}
