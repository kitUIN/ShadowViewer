namespace ShadowViewer.Configs
{
    public partial class Config
    {
        private static string container = "ShadowConfig";
        public static void ConfigInit()
        {
            Config config = new Config();
        }
        public Config() 
        {
            if (!Contains("IsDebug"))
            {
                IsDebug = false;
            }
            if (!Contains("ComicsPath"))
            {
                ComicsPath= Path.Combine(ApplicationData.Current.LocalFolder.Path, "Comics");
            }
            if (!Contains("TempPath"))
            {
                TempPath=Path.Combine(ApplicationData.Current.LocalFolder.Path, "Temps");
            }
            IsDebugEvent();
            ComicsPath.CreateDirectory();
            TempPath.CreateDirectory();
        } 
        public static string ComicsPath
        {
            get => GetString("ComicsPath");
            set => Set("ComicsPath", value);
        }
        public static string TempPath
        {
            get => GetString("TempPath");
            set => Set("TempPath", value);
        }
        public static bool IsDebug
        {
            get =>  GetBoolean("IsDebug");
            set
            { 
                if(IsDebug != value)
                {
                    Set("IsDebug", value);
                    IsDebugEvent();
                }
            }
        }
        public static bool IsRememberDeleteFilesWithComicDelete
        {
            get => GetBoolean("IsRememberDeleteFilesWithComicDelete");
            set => Set("IsRememberDeleteFilesWithComicDelete", value);
        }
        public static bool IsDeleteFilesWithComicDelete
        {
            get => GetBoolean("IsDeleteFilesWithComicDelete");
            set => Set("IsDeleteFilesWithComicDelete", value);
        }
        private static void IsDebugEvent()
        {
            if(IsDebug)
            {
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "ShadowViewer.log"), outputTemplate: "{Timestamp:MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} | {Message:lj} {Exception}{NewLine}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                Log.ForContext<Config>().Debug("调试模式开启");
            }
            else
            {
                Log.ForContext<Config>().Debug("调试模式关闭");
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "ShadowViewer.log"), outputTemplate: "{Timestamp:MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} | {Message:lj} {Exception}{NewLine}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            }
        }
        private static bool Contains(string key)
        {
            return ConfigHelper.Contains(container, key);
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
            object res = Get(key);
            if (res== null)
            {
                return false;
            }
            return (bool)res;
        }
    }
}
