using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DryIoc;
using Serilog;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core.Models.Interfaces;
using ShadowViewer.Core;
using ShadowViewer.Core.Services;

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
        public SettingsViewModel(ICallableService callableService, PluginLoader pluginService, PluginEventService pluginEventService, ILogger logger)
        {
            Caller = callableService;
            PluginService = pluginService;
            var v = Package.Current.Id.Version;
            Version = $"v{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            Logger = logger;
            InitSettingsFolders();
        }

        #endregion
        /// <summary>
        /// 当前版本号
        /// </summary>
        public string Version { get; }

        public ObservableCollection<ISettingFolder> SettingsFolders { get; } = [];

        [ObservableProperty] private bool isDebug = CoreSettings.IsDebug;
        [ObservableProperty] private string comicsPath = CoreSettings.ComicsPath;
        [ObservableProperty] private string tempPath = CoreSettings.TempPath;
        [ObservableProperty] private string pluginsPath = CoreSettings.PluginsPath;
        [ObservableProperty] private string pluginsUri = CoreSettings.PluginsUri;

        public void InitSettingsFolders()
        {
            SettingsFolders.Clear();
            foreach (var folder in DiFactory.Services.ResolveMany<ISettingFolder>())
            {
                SettingsFolders.Add(folder);
            }
        }
        partial void OnPluginsUriChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                CoreSettings.PluginsUri = PluginsUri;
            }
        }
        partial void OnPluginsPathChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                CoreSettings.PluginsPath = PluginsPath;
            }
        }
        partial void OnComicsPathChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                CoreSettings.ComicsPath = ComicsPath;
            }
        }

        partial void OnIsDebugChanged(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            CoreSettings.IsDebug = IsDebug;
            Caller.Debug();
        }

        partial void OnTempPathChanged(string? oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                CoreSettings.TempPath = TempPath;
            }
        }

    }
}