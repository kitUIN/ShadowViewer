using System;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ShadowViewer.Core.Helpers;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using CustomExtensions.WinUI;
using Microsoft.UI.Dispatching;

namespace ShadowViewer
{
    public partial class App : Application
    {
        public App()
        {
            ApplicationExtensionHost.Initialize(this);
            this.InitializeComponent();
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