namespace ShadowViewer.Helpers
{
    public static class ResourcesHelper
    {
        private static readonly ResourceLoader resourceLoader = new ResourceLoader();
        public static string GetString(string key)
        {
            return resourceLoader.GetString(key.Replace(".","/"));
        }
        public static string GetString(ResourceKey key)
        {
            return GetString(key.ToString());
        }
    }
}
