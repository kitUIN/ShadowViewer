using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Sdk.Models.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace ShadowViewer.ViewModels
{
    /// <summary>
    /// 设置
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        #region DI

        public SettingsViewModel()
        {
            var v = Package.Current.Id.Version;
            Version = $"v{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            InitSettingsFolders();
        }

        #endregion

        /// <summary>
        /// 当前版本号
        /// </summary>
        public string Version { get; }

        public ObservableCollection<ISettingFolder> SettingsFolders { get; } = [];

        public void InitSettingsFolders()
        {
            SettingsFolders.Clear();
            foreach (var folder in DiFactory.Services.ResolveMany<ISettingFolder>())
            {
                SettingsFolders.Add(folder);
            }
        }

        /// <summary>
        /// 跳转到uri
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task NavigateToUri(string url)
        {
            var uri = new Uri(url);
            await Launcher.LaunchUriAsync(uri);
        }
    }
}