namespace ShadowViewer.Helpers
{
    public class I18nHelper
    {
        private static ResourceLoader resourceLoader = new ResourceLoader();
        private ResourceManager resourceManager;
        private string prefix;
        public I18nHelper(string prefix)
        {
            resourceManager = new ResourceManager();
            this.prefix = prefix;
        }
        public string Get(string key)
        {
            return resourceManager.MainResourceMap.GetValue(prefix + key.Replace(".","/")).ValueAsString;
        }
        public static string GetString(string key)
        {
            return resourceLoader.GetString(key.Replace(".", "/"));
        }
    }
}
