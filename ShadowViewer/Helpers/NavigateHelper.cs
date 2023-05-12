using System.Linq;

namespace ShadowViewer.Helpers
{
    public static class NavigateHelper
    {
        private static void Navigate(string id,Uri url)
        {
            var comic = ComicDB.GetFirst("id", id);
            if (comic.IsFolder)
            {
                MessageHelper.SendNavigationFrame(typeof(HomePage), url);
            }
            else
            {

            }
        }
        public static void ShadowNavigate(Uri uri)
        {
            // 本应用协议
            if (uri.Scheme == "shadow")
            {
                var urls = uri.AbsolutePath.Split(new char[] { '/',}, StringSplitOptions.RemoveEmptyEntries);
                // 本地
                switch (uri.Host.ToLower())
                {
                    case "local":
                        if (urls.Length == 0) { MessageHelper.SendNavigationFrame(typeof(HomePage), uri); return; }
                        for (int i = 0; i < urls.Length; i++)
                        {
                            if (!ComicDB.Contains("id", urls[i]))
                            {
                                var s = "shadow://local/" + string.Join("/", urls.Take(i + 1));
                                Navigate(urls[i - 1], new Uri(s));
                                return;
                            }
                        }
                        Navigate(urls.Last(), uri);
                        break;
                    case "settings":
                        MessageHelper.SendNavigationFrame(typeof(SettingsPage));
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
