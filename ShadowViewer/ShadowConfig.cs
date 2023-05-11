namespace ShadowViewer.Configs
{
    public partial class ShadowConfig: ObservableObject
    {
        private static string container = "ShadowConfig";
        [ObservableProperty]
        private string comicsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Comics");
        [ObservableProperty]
        private bool debug = false;
        partial void OnComicsPathChanged(string oldValue, string newValue)
        {
            if(newValue != oldValue)
            {
                Set(nameof(ComicsPath), newValue);
            }
        }
        partial void OnDebugChanged(bool oldValue, bool newValue)
        {
            if (newValue != oldValue)
            {
                Set(nameof(Debug), newValue);
            }
        }

        private void Set(string key,object value)
        {
            ConfigHelper.Set(container, key, value);
        }
        private object Get(string key)
        {
            return ConfigHelper.Get(container, key);
        }
        private string GetString(string key)
        {
            return (string) Get(key);
        }
        private bool GetBoolean(string key)
        {
            return (bool) Get(key);
        }
    }
}
