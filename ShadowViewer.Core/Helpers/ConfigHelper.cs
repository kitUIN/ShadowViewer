namespace ShadowViewer.Helpers
{
    public static class ConfigHelper
    {
        
        public static string Get(string container,string key)
        {
            ApplicationDataContainer CoreSettings = ApplicationData.Current.LocalSettings.CreateContainer(container, ApplicationDataCreateDisposition.Always);
            if (CoreSettings.Values.ContainsKey(key))
            {
                return (string)CoreSettings.Values[key];
            }
            return null;
        }
        public static void Set(string container, string key, string value)
        {
            ApplicationDataContainer CoreSettings = ApplicationData.Current.LocalSettings.CreateContainer(container, ApplicationDataCreateDisposition.Always);
            CoreSettings.Values[key] = value;
        }
        public static ApplicationDataCompositeValue CreateDict()
        {
            return new ApplicationDataCompositeValue();
        }
        public static ApplicationDataCompositeValue GetDict(string container, string key)
        {
            ApplicationDataContainer CoreSettings = ApplicationData.Current.LocalSettings.CreateContainer(container, ApplicationDataCreateDisposition.Always);
            if (CoreSettings.Values.ContainsKey(key))
            {
                return CoreSettings.Values[key] as ApplicationDataCompositeValue;
            }
            return null;
        }
        public static string Get(string key)
        {
            return Get("ShadowViewer", key);
        }
        public static void Set(string key, string value)
        {
            Set("ShadowViewer", key, value);
        }
        public static ApplicationDataCompositeValue GetDict(string key)
        {
            return GetDict("ShadowViewer", key);
        }
        
    }
}
