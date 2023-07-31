using Microsoft.Extensions.DependencyInjection;
using CustomExtensions.WinUI;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugins;
using ShadowViewer.ToolKits;
using ShadowViewer.ViewModels;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace ShadowViewer.DI
{
    public class DIFactory
    {
        public DIFactory()
        {
            Services = ConfigureServices();
        }
        public static DIFactory Current;
        public IServiceProvider Services { get; set; }

        private static IServiceProvider ConfigureServices()
        {
            
            ServiceCollection services = new ServiceCollection();
            #region Plugin 

            #endregion
            #region ToolKit
            services.AddSingleton<IPluginsToolKit, PluginsToolKit>();
            services.AddSingleton<ICallableToolKit, CallableToolKit>();
            services.AddSingleton<CompressToolKit>();
            #endregion
            #region ViewModel
            services.AddSingleton<SettingsViewModel>();
            services.AddScoped<BookShelfViewModel>();
            services.AddSingleton<NavigationViewModel>();
            services.AddScoped<AttributesViewModel>();
            #endregion
            return services.BuildServiceProvider();
        }
    }
}
