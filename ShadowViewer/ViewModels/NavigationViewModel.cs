namespace ShadowViewer.ViewModels
{
    internal class NavigationViewModel: ObservableRecipient, IRecipient<NavigationMessage>
    {
        private Frame frame;
        private NavigationViewItem pluginItem;
        private XamlRoot xamlRoot;
        private Grid topGrid;
        public NavigationViewModel(Frame frame, XamlRoot xamlRoot, Grid topGrid)
        {
            IsActive = true;
            this.frame = frame;
            this.xamlRoot = xamlRoot;
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
        public async void Receive(NavigationMessage message)
        {
            if (message.objects.Length >= 1 && message.objects[0] is string method)
            {
                // 导航操作
                if(method == "Navigate" && message.objects.Length >=2 && message.objects[1] is string str)
                {
                    // 导航栏插件注入重置
                    if (str == "PluginReload")
                    {
                        LoadPluginItems(pluginItem);
                    }
                    // 跳转新的页面
                    else if (str == "Frame" && message.objects.Length == 3
                        && message.objects[2] is Type page)
                    {
                        var preNavPageType = frame.CurrentSourcePageType;
                        if (!(page is null) && !Type.Equals(preNavPageType, page))
                        {
                            frame.Navigate(page);
                        }
                    }
                }
                // 对话框
                else if(method == "ContentDialog" && message.objects.Length >= 2 && message.objects[1] is ContentDialog dialog)
                {
                    
                    await dialog.ShowAsync();
                }
                // 顶部元素
                else if(method == "TopGrid" && message.objects.Length >= 2 && message.objects[1] is UIElement element)
                {
                      
                }
                
            }
        }
    } 
}
