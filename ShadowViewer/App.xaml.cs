

namespace ShadowViewer
{

    public partial class App : Application
    {
        public static ShadowConfig Config { get; set; } 
        public App()
        {
            this.InitializeComponent();
            Config = new ShadowConfig();
            // 文件创建
            _ = Config.ComicsPath.ToStorageFolder();
            _ = ApplicationData.Current.LocalFolder.CreateFileAsync("ShadowViewer.db");
            // log
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "ShadowViewer.log"), outputTemplate: "{Timestamp:MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} | {Message:lj} {Exception}{NewLine}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            
            // 数据库
            ComicDB.Init();
            TagDB.Init();
            // 插件
            PluginHelper.Init();
            // 标签数据
            TagsHelper.Init();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            startupWindow = new MainWindow();
            ThemeHelper.Initialize();
            WindowHelper.TrackWindow(startupWindow);
            startupWindow.Activate();
            Uri firstUri = new Uri("shadow://local/");
            var actEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (actEventArgs.Kind == ExtendedActivationKind.Protocol
                && actEventArgs.Data is IProtocolActivatedEventArgs data && data != null)
            {
                firstUri = data.Uri;
            } 
            NavigateHelper.ShadowNavigate(firstUri);
        }
        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }
        private static Window startupWindow;
        public static Window StartupWindow
        {
            get
            {
                return startupWindow;
            }
        }
         
    }
}
