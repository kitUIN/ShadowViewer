namespace ShadowViewer.Plugins
{
    public interface IPlugin
    {
        
        /// <summary>
        /// 元数据(包含相关数据)
        /// </summary>
        /// <returns></returns>
        public PluginMetaData MetaData();
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init();
        /// <summary>
        /// 插件跳转页面
        /// </summary>
        /// <returns></returns>
        public Type NavigationPage();
        /// <summary>
        /// 导航插件栏注入
        /// </summary>
        /// <param name="nav">插件栏</param>
        public void NavigationViewItemsHandler(NavigationViewItem navItem);
        /// <summary>
        /// 导航点击事件注入
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public void NavigationViewItemInvokedHandler(string tag,out Type _page,out object parameter);
        /// <summary>
        /// 插件设置注入
        /// </summary>
        /// <returns></returns>
        public void PluginSettingsExpander(SettingsExpander expander);
        /// <summary>
        /// 插件标签注入
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public ShadowTag PluginTagHandler(string tag);
    }
}
