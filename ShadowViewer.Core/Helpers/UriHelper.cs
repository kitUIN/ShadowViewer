namespace ShadowViewer.Helpers
{
    public static class UriHelper
    {
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
    }
}
