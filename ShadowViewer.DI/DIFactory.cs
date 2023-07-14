using Microsoft.Extensions.DependencyInjection;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugin.Bika;
using ShadowViewer.Plugins;
using ShadowViewer.ToolKits;
using ShadowViewer.ViewModels;
using System;
namespace ShadowViewer.DI
{
    public class DIFactory
    {
        public DIFactory()
        {
            Services = ConfigureServices();
        }
        public static DIFactory Current;
        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();
            #region Plugin
            services.AddSingleton<IPlugin, BikaPlugin>();
            #endregion
            #region ToolKit
            services.AddSingleton<IResourcesToolKit, BikaResourcesToolKit>();
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
