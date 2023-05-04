namespace ShadowViewer.Helpers
{
    public class PluginHelper
    {
        /// <summary>
        /// 插件需求版本.
        /// </summary>
        public static int RequireVersion { get; } = 1;
        /// <summary>
        /// 所有插件id
        /// </summary> 
        public static ObservableCollection<string> Plugins { get;  } = new ObservableCollection<string>();
        /// <summary>
        /// 启用的插件id
        /// </summary> 
        public static ObservableCollection<string> EnabledPlugins { get;  } = new ObservableCollection<string>();
        /// <summary>
        /// 所有插件实例
        /// </summary> 
        public static Dictionary<string,IPlugin> PluginInstances { get; } = new Dictionary<string,IPlugin>();
        private static void EnabledPlugins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConfigHelper.Set("EnabledPlugins", string.Join(",", EnabledPlugins));
        }
        private static void Plugins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConfigHelper.Set("Plugins", string.Join(",", Plugins));
            foreach (var plugin in e.NewItems)
            {
                AddPluginInstance(plugin.ToString());
            }
        }
        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="name">插件id</param>
        public static void AddPlugins(string name)
        {
            if (name != "" && !Plugins.Contains(name))
            {
                Plugins.Add(name);
            }
        }
        /// <summary>
        /// 启用插件
        /// </summary>
        /// <param name="name">插件id</param>
        public static void PluginEnabled(string name)
        {
            if (!EnabledPlugins.Contains(name) && Plugins.Contains(name))
            {
                EnabledPlugins.Add(name);
            }
        }
        /// <summary>
        /// 禁用插件
        /// </summary>
        /// <param name="name">插件id</param>
        public static void PluginDisabled(string name)
        {
            if (EnabledPlugins.Contains(name) && Plugins.Contains(name))
            {
                EnabledPlugins.Remove(name);
                string pluginName = PluginInstances[name].MetaData().Name; 
                Log.ForContext<PluginHelper>().Information("[{name}]插件禁用成功", pluginName);
            }
        }
        /// <summary>
        /// 添加插件实例
        /// </summary>
        /// <param name="name">插件id</param>
        /// <returns></returns>
        private static bool AddPluginInstance(string name)
        {
            try
            {
                if (!PluginInstances.ContainsKey(name) && Assembly.Load($"ShadowViewer.Plugin.{name}")
                .CreateInstance($"ShadowViewer.Plugin.{name}.{name}Plugin") is IPlugin pluginPlugin)
                {
                    var meta = pluginPlugin.MetaData();
                    if (meta.MinVersion > RequireVersion)
                    {
                        Log.ForContext<PluginHelper>().Error("[{name}]插件加载失败:所需版本:{min},实际版本:{require}", meta.Name, meta.MinVersion, RequireVersion);
                        return false;
                    }
                    PluginInstances[name] = pluginPlugin;
                    if(EnabledPlugins.Contains(name))
                    {
                        PluginInstances[name].Init();
                        Log.ForContext<PluginHelper>().Information("[{name}]插件加载成功", PluginInstances[name].MetaData().Name);
                    }
                    return true;
                }
            }catch(Exception e)
            {
                Log.ForContext<PluginHelper>().Error("[{name}]插件加载失败:{error}", 
                    PluginInstances[name].MetaData().Name, e.ToString());
            }
            return false;
        }
        public static void Init()
        {
            EnabledPlugins.CollectionChanged += EnabledPlugins_CollectionChanged;
            Plugins.CollectionChanged += Plugins_CollectionChanged;

            if (ConfigHelper.Get("Plugins") is string plugins)
            {
                foreach(string name in plugins.Split(","))
                {
                    AddPlugins(name);
                }
            }
            if (ConfigHelper.Get("EnabledPlugins") is string enablePlugins)
            {
                foreach (string name in enablePlugins.Split(","))
                {
                    PluginEnabled(name);
                }
            }
            AddPlugins("Bika");
        }
    }
}
