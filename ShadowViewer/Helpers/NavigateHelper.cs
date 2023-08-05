
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
                var db =  DiFactory.Current.Services.GetService<ISqlSugarClient>();
                var _navigationToolKit = DiFactory.Current.Services.GetService<ICallableToolKit>();
                string[] urls = uri.AbsolutePath.Split(new char[] { '/',}, StringSplitOptions.RemoveEmptyEntries);
                // 本地
                switch (uri.Host.ToLower())
                {
                    case "local":
                        if (urls.Length == 0) 
                        {
                            _navigationToolKit.NavigateTo(NavigateMode.Page,typeof(BookShelfPage),null, uri); 
                            return; 
                        }
                        for (int i = 0; i < urls.Length; i++)
                        {
                            if (!db.Queryable<LocalComic>().Any(x => x.Id == urls[i])) 
                            {
                                string s = "shadow://local/" + string.Join("/", urls.Take(i + 1));
                                _navigationToolKit.NavigateTo(NavigateMode.URL,null, urls[i - 1], new Uri(s));
                                return;
                            }
                        }
                        _navigationToolKit.NavigateTo(NavigateMode.URL, null, urls.Last(), uri);
                        break;
                    case "settings":
                        _navigationToolKit.NavigateTo(NavigateMode.Page, typeof(SettingsPage), null, null);
                        break;
                    case "download":
                        break;
                    default:
                        //TODO: 插件注入
                        break;
                } 
            }
        }
    }
}
