using CustomExtensions.WinUI;
using Microsoft.Windows.AppLifecycle;
using ShadowObservableConfig.Json;
using ShadowObservableConfig.Yaml;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Sdk.Configs;
using ShadowViewer.Sdk.Helpers;
using System;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using DryIoc;

namespace ShadowViewer
{
    public partial class App
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

            DiFactory.Services.RegisterInstance(CoreConfig.Load());
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
            startupWindow.Activate();
            // // 导航
        }
    }
}