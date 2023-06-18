using ShadowViewer.Interfaces;

namespace ShadowViewer.ViewModels
{
    public class NavigationViewModel: ObservableRecipient, IRecipient<NavigationMessage>
    {
        private Frame frame;
        private NavigationViewItem pluginItem;
        private DispatcherTimer timer = new DispatcherTimer();
        private Grid topGrid;
        
        public NavigationViewModel()
        {
             
        }

        private void Timer_Tick(object sender, object e)
        {
        }
        /// <summary>
        /// 导航栏插件栏注入
        /// </summary>
        /// <param name="pluginItem">The plugin item.</param>
        public void LoadPluginItems(NavigationViewItem pluginItem)
        {
            this.pluginItem = pluginItem;
            pluginItem.MenuItems.Clear();
            // TODO
           /* foreach (string name in PluginHelper.EnabledPlugins)
            {
                PluginHelper.PluginInstances[name].NavigationViewItemsHandler(pluginItem);
                Log.ForContext<NavigationViewModel>().Information("[{name}]插件导航栏注入成功",
                    PluginHelper.PluginInstances[name].MetaData().Name);
            }*/
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
                else if(method == "Info" && message.objects.Length == 2 && message.objects[1] is InfoBar infoBar)
                {
                    timer.Interval = TimeSpan.FromSeconds(5);
                    topGrid.Children.Clear();
                    topGrid.Children.Add(infoBar);
                    infoBar.IsOpen = true;
                }
            }
        }
    } 
}
