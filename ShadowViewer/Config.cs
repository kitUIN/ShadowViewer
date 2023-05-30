namespace ShadowViewer.Configs
{
    public class Config
    { 
        public static void ConfigInit()
        {
            if (!ConfigHelper.Contains("ComicsPath"))
            {
                ComicsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Comics");
            }
            if (!ConfigHelper.Contains("TempPath"))
            {
                TempPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Temps");
            }
            if (!ConfigHelper.Contains("IsBookShelfInfoBar"))
            {
                IsBookShelfInfoBar = true;
            }
            IsDebugEvent();
            ComicsPath.CreateDirectory();
            TempPath.CreateDirectory();
        } 
        public static string ComicsPath
        {
            get => ConfigHelper.GetString("ComicsPath");
            set => ConfigHelper.Set("ComicsPath", value);
        }
        public static string TempPath
        {
            get => ConfigHelper.GetString("TempPath");
            set => ConfigHelper.Set("TempPath", value);
        }
        public static bool IsDebug
        {
            get => ConfigHelper.GetBoolean("IsDebug");
            set
            { 
                if(IsDebug != value)
                {
                    ConfigHelper.Set("IsDebug", value);
                    IsDebugEvent();
                }
            }
        }
        public static bool IsRememberDeleteFilesWithComicDelete
        {
            get => ConfigHelper.GetBoolean("IsRememberDeleteFilesWithComicDelete");
            set => ConfigHelper.Set("IsRememberDeleteFilesWithComicDelete", value);
        }
        public static bool IsDeleteFilesWithComicDelete
        {
            get => ConfigHelper.GetBoolean("IsDeleteFilesWithComicDelete");
            set => ConfigHelper.Set("IsDeleteFilesWithComicDelete", value);
        }
        public static bool IsBookShelfMenuShow
        {
            get => ConfigHelper.GetBoolean("IsBookShelfMenuShow");
            set => ConfigHelper.Set("IsBookShelfMenuShow", value);
        }
        public static bool IsBookShelfDetailShow
        {
            get => ConfigHelper.GetBoolean("IsBookShelfDetailShow");
            set => ConfigHelper.Set("IsBookShelfDetailShow", value);
        }
        public static bool IsBookShelfInfoBar
        {
            get => ConfigHelper.GetBoolean("IsBookShelfInfoBar");
            set => ConfigHelper.Set("IsBookShelfInfoBar", value);
        }
        public static bool IsImportAgain
        {
            get => ConfigHelper.GetBoolean("IsImportAgain");
            set => ConfigHelper.Set("IsImportAgain", value);
        }
        private static void IsDebugEvent()
        {
            if(IsDebug)
            { 
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "ShadowViewer.log"), outputTemplate: "{Timestamp:MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} | {Message:lj} {Exception}{NewLine}", rollingInterval: RollingInterval.Day, shared:true)
                .CreateLogger();
                Log.ForContext<Config>().Debug("调试模式开启");
            }
            else
            {
                Log.ForContext<Config>().Debug("调试模式关闭");
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "ShadowViewer.log"), outputTemplate: "{Timestamp:MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} | {Message:lj} {Exception}{NewLine}", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
            }
        } 
        
    }
}
