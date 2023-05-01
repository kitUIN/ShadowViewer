using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using System.Reflection;

namespace ShadowViewer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            _ = FileHelper.CreateFolderAsync(ApplicationData.Current.LocalFolder, "Logs");
            _ = FileHelper.CreateFileAsync(ApplicationData.Current.LocalFolder, "ShadowViewer.db");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs", "ShadowViewer.log"), outputTemplate: "{Timestamp:MM-dd HH:mm:ss.fff} [{Level:u4}] {SourceContext} | {Message:lj} {Exception}{NewLine}", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            DBHelper.InitializeDatabase();
            PluginHelper.Init();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var actEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (actEventArgs.Kind == ExtendedActivationKind.Protocol 
                && actEventArgs.Data is IProtocolActivatedEventArgs data)
            {
                if (data != null)
                {
                    var uri = data.Uri;
                    var uriString = uri.AbsoluteUri;
                    Log.Information(uriString);
                }
            }
            startupWindow = new MainWindow();
            ThemeHelper.Initialize();
            WindowHelper.TrackWindow(startupWindow);
            startupWindow.Activate();
            
        }
        private void NotificationManager_NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            Log.Information("1111");
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
