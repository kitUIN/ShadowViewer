namespace ShadowViewer.ViewModels
{
    internal class NavigationViewModel: ObservableRecipient, IRecipient<NavigationMessage>
    {
        private Frame frame;
        private NavigationViewItem pluginItem;
 
        private Grid topGrid;
        public void Navigate(Frame frame,   Grid topGrid)
        {
            IsActive = true;
            this.frame = frame; 
            this.topGrid = topGrid;
        }
        /// <summary>
        /// 导航栏插件栏注入
        /// </summary>
        /// <param name="pluginItem">The plugin item.</param>
        public void LoadPluginItems(NavigationViewItem pluginItem)
        {
            this.pluginItem = pluginItem;
            pluginItem.MenuItems.Clear();
            foreach (string name in PluginHelper.EnabledPlugins)
            {
                PluginHelper.PluginInstances[name].NavigationViewItemsHandler(pluginItem);
                Log.ForContext<NavigationViewModel>().Information("[{name}]插件导航栏注入成功",
                    PluginHelper.PluginInstances[name].MetaData().Name);
            }
        }
        public void Receive(NavigationMessage message)
        {
            if (message.objects.Length >= 1 && message.objects[0] is string method)
            {
                // 导航栏插件注入重置
                if (method == "PluginReload" && message.objects.Length == 1)
                {
                    LoadPluginItems(pluginItem);
                }
                // 跳转新的页面
                else if (method == "Navigate" && message.objects.Length ==4
                    && message.objects[1] is Type page
                    && !(page is null) && !Type.Equals(frame.CurrentSourcePageType, page))
                {
                    frame.Navigate(page, message.objects[2], (NavigationTransitionInfo)message.objects[3]);
                } 
                // 顶部元素
                else if(method == "TopGrid" && message.objects.Length >= 2 && message.objects[1] is UIElement element)
                {
                      
                }
                
            }
        }
    } 
}
