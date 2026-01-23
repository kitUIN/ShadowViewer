using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using Microsoft.UI.Xaml.Controls;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Sdk.Models.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using ShadowViewer.I18n;

namespace ShadowViewer.ViewModels
{
    /// <summary>
    /// 设置
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        #region DI        
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel()
        {
            var v = Package.Current.Id.Version;
            var preview = v.Build != 0;
            var minor = preview ? v.Minor + 1 : v.Minor;
            Version = $"v{v.Major}.{minor}";
            Preview = preview ? I18N.PreviewVersion + v.Build : "";
            InitSettingsFolders();
        }

        #endregion

        /// <summary>
        /// 当前版本号
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 预览版标记
        /// </summary>
        public string Preview { get; }

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