using Windows.ApplicationModel;

using ShadowViewer.Plugins;

namespace ShadowViewer.ViewModels
{
    /// <summary>
    /// 设置
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        #region DI
        private ICallableService Caller { get; }
        private ILogger Logger { get; }
        private PluginLoader PluginService { get; }
        public SettingsViewModel(ICallableService callableService, PluginLoader pluginService, ILogger logger)
        {
            Caller = callableService;
            PluginService = pluginService;
            var v = Package.Current.Id.Version;
            Version = $"v{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            Logger = logger;
        }
        #endregion
        /// <summary>
        /// 当前版本号
        /// </summary>
        public string Version { get; }

        public ObservableCollection<PluginBase> Plugins { get; } = new();

        [ObservableProperty] private bool isDebug = Config.IsDebug;
        [ObservableProperty] private string comicsPath = Config.ComicsPath;
        [ObservableProperty] private string tempPath = Config.TempPath;
        [ObservableProperty] private string pluginsPath = Config.PluginsPath;
        [ObservableProperty] private string pluginsUri = Config.PluginsUri;

        public void InitPlugins()
        {
            Plugins.Clear();
            foreach (var plugin in PluginService.GetPlugins())
            {
                Plugins.Add(plugin);
            }
        }
        partial void OnPluginsUriChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                Config.PluginsUri = PluginsUri;
            }
        }
        partial void OnPluginsPathChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                Config.PluginsPath = PluginsPath;
            }
        }
        partial void OnComicsPathChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                Config.ComicsPath = ComicsPath;
            }
        }

        partial void OnIsDebugChanged(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            Config.IsDebug = IsDebug;
            Caller.Debug();
        }

        partial void OnTempPathChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                Config.TempPath = TempPath;
            }
        }

    }
}