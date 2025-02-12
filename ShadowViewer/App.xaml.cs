using System;
using CustomExtensions.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Helpers;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using WinUIEx;

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
            var loadingMainWindow = new LoadingWindow(typeof(MainWindow));
            loadingMainWindow.Completed += (_, startupWindow) =>
            {
                WindowHelper.TrackWindow(startupWindow);
                ThemeHelper.Initialize(startupWindow);
                var firstUri = new Uri("shadow://local/bookshelf");
                var actEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
                if (actEventArgs.Kind == ExtendedActivationKind.Protocol
                    && actEventArgs.Data is IProtocolActivatedEventArgs data)
                {
                    firstUri = data.Uri;
                }

                Debug.WriteLine("启动");
                startupWindow.Activate();
                // 导航
                NavigateHelper.ShadowNavigate(firstUri);
            };
        }
    }
}