namespace ShadowViewer.Helpers
{
    public static class NavigateHelper
    {
        private static void Navigate(string id,Uri url)
        {
            var comic = ComicDB.Get("id", id)[0];
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
                var urls = uri.AbsolutePath.Split("/").ToList();
                urls.RemoveAll(x=>x=="");
                // 本地
                if (uri.Host == "local")
                {
                    if (urls.Count == 0) { MessageHelper.SendNavigationFrame(typeof(HomePage), uri); return; }
                    for (int i = 0; i < urls.Count; i++)
                    {
                        if (!ComicDB.Contains("id", urls[i]))
                        {
                            var s = "shadow://local/" + string.Join("/", urls.GetRange(0, i));
                            Navigate(urls[i-1], new Uri(s));
                            return;
                        }
                    }
                    Navigate(urls.Last(), uri);
                }
                else
                {
                    //TODO: 插件注入
                }
            }
        }
    }
}
