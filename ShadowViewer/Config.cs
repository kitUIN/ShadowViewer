namespace ShadowViewer.Configs
{
    public partial class Config: ObservableObject
    {
        private static string container = "ShadowConfig";
        [ObservableProperty]
        private string comicsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Comics");
        [ObservableProperty]
        private string tempPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Temps");
        [ObservableProperty]
        private bool isDebug = false;
        private Config(bool debug,string comicsPath):base()
        {
            this.isDebug = debug;
            this.comicsPath = comicsPath;
        }
        private Config() 
        {
            ComicsPath.CreateDirectory();
            TempPath.CreateDirectory();
        }
        public static Config CreateConfig()
        {
            if(ConfigHelper.Contains(container, "IsDebug"))
            {
                return new Config(GetBoolean("IsDebug"), GetString("ComicsPath"));
            }
            return new Config();
        }
        partial void OnComicsPathChanged(string oldValue, string newValue)
        {
            if(newValue != oldValue)
            {
                Set(nameof(ComicsPath), newValue);
            }
        }
        
        partial void OnIsDebugChanged(bool oldValue, bool newValue)
        {
            if (newValue != oldValue)
            {
                Set(nameof(IsDebug), newValue);
            }
        }

        private static void Set(string key,object value)
        {
            ConfigHelper.Set(container, key, value);
        }
        private static object Get(string key)
        {
            return ConfigHelper.Get(container, key);
        }
        private static string GetString(string key)
        {
            return (string) Get(key);
        }
        private static bool GetBoolean(string key)
        {
            return (bool) Get(key);
        }
    }
}
