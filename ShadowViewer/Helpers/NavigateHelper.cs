
using Serilog.Core;

namespace ShadowViewer.Helpers
{
    public static class NavigateHelper
    {
        public static void ShadowNavigate(Uri uri)
        {

            // 本应用协议
            if (uri.Scheme == "shadow")
            {
                INavigationToolKit _navigationToolKit = DIFactory.Current.Services.GetService<INavigationToolKit>();
                string[] urls = uri.AbsolutePath.Split(new char[] { '/',}, StringSplitOptions.RemoveEmptyEntries);
                // 本地
                switch (uri.Host.ToLower())
                {
                    case "local":
                        if (urls.Length == 0) 
                        {
                            _navigationToolKit.NavigateToPage(Enums.NavigateMode.Page,typeof(BookShelfPage),null, uri); 
                            return; 
                        }
                        for (int i = 0; i < urls.Length; i++)
                        {
                            if (!DBHelper.Db.Queryable<LocalComic>().Any(x => x.Id == urls[i])) 
                            {
                                string s = "shadow://local/" + string.Join("/", urls.Take(i + 1));
                                _navigationToolKit.NavigateToPage(Enums.NavigateMode.URL,null, urls[i - 1], new Uri(s));
                                return;
                            }
                        }
                        _navigationToolKit.NavigateToPage(Enums.NavigateMode.URL, null, urls.Last(), uri);
                        break;
                    case "settings":
                        _navigationToolKit.NavigateToPage(Enums.NavigateMode.Page, typeof(SettingsPage), null, null);
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
