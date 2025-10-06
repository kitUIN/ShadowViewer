using System;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ShadowViewer.Sdk.Helpers;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using CustomExtensions.WinUI;
using ShadowObservableConfig.Json;
using ShadowObservableConfig.Yaml;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            ApplicationExtensionHost.Initialize(this);
            this.InitializeComponent();
            ShadowObservableConfig.GlobalSetting.Init(
                ApplicationData.Current.LocalFolder.Path,
                [
                    new JsonConfigLoader(),
                    new YamlConfigLoader()
                ]);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var firstUri = new Uri("shadow://local/bookshelf");
            var actEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (actEventArgs.Kind == ExtendedActivationKind.Protocol
                && actEventArgs.Data is IProtocolActivatedEventArgs data)
            {
                firstUri = data.Uri;
            }
            var startupWindow = new MainWindow(firstUri);
            WindowHelper.TrackWindow(startupWindow);
            ThemeHelper.Initialize(startupWindow);
             
            startupWindow.Activate();
            // // 导航
        }
    }
}