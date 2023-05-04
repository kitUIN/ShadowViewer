﻿


namespace ShadowViewer.Helpers
{
    public static class UriHelper
    {
        public static ShadowPath ShadowPathTree;
        public static void PathTreeInit()
        {
            ShadowPathTree = new ShadowPath("local", false, null);
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
        public static string JoinToString(this ObservableCollection<string> tags)
        {
            return string.Join(",", tags);
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
    }
}
