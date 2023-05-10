


using System.Linq;

namespace ShadowViewer.Helpers
{
    public static class UriHelper
    {
        public static ShadowPath PathTreeInit(List<string> black)
        {
            return new ShadowPath("local", false, null, black);
        }
        public static async void LaunchUriAsync(this Uri uri)
        {
            if (uri != null)
            {
                await Launcher.LaunchUriAsync(uri);
            }
        }
        public static async void LaunchFolderAsync(this StorageFolder folder)
        {
            if (folder != null)
            {
                await Launcher.LaunchFolderAsync(folder);
            }
        }
        public static string JoinToString(this ObservableCollection<string> tags,string separator = ",")
        {
            return string.Join(separator, tags);
        }
        public static async Task<StorageFile> GetFile(this Uri uri)
        {
            return await StorageFile.GetFileFromPathAsync(uri.DecodePath());
        }
        public static string DecodePath(this StorageFile file)
        {
            return HttpUtility.UrlDecode(file.Path);
        }
        public static string DecodePath(this Uri uri)
        {
            return HttpUtility.UrlDecode(uri.AbsolutePath);
        }
        public static string DecodeUri(this Uri uri)
        {
            return HttpUtility.UrlDecode(uri.AbsoluteUri);
        }
        public static void ShadowNavigate(Uri uri)
        {
            // 本应用协议
            if (uri.Scheme == "shadow")
            {
                var urls = uri.AbsolutePath.Split("/").ToList();
                if (urls[0] == "/") urls.RemoveAt(0);
                // 本地
                if (uri.Host == "local")
                {
                    
                    for(int i=0;i<urls.Count;i++)
                    {
                        if(!ComicDB.Contains("id", urls[i]))
                        {
                            var s = "shadow://local/"+string.Join("/", urls.GetRange(0, i));

                        } 
                    }
                    //TODO: 导航
                    var comic = ComicDB.Get("id", urls.Last())[0];
                    if (comic.IsFolder)
                    {

                    }
                    else
                    {

                    }
                }
                else
                {
                    //TODO: 插件注入
                }
            }
        }
    }
}
