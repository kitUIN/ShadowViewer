namespace ShadowViewer.Helpers
{
    public static class AppResourcesHelper
    {
        private static readonly ResourceLoader resourceLoader = new ResourceLoader();
        public static string GetString(string key)
        {
            return resourceLoader.GetString(key.Replace(".","/"));
        }
    }
}
