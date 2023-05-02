namespace ShadowViewer.Configs
{
    public class ShadowConfig
    {
        
        private static string container = "ShadowConfig";
        public static readonly ShadowConfig Instance = new ShadowConfig();

       
        private void Set(string key,string value)
        {
            ConfigHelper.Set(container, key, value);
        }
        private string Get(string key)
        {
            return ConfigHelper.Get(container,key);
        }
    }
}
