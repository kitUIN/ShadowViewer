using ShadowViewer.Messages;

namespace ShadowViewer.Helpers
{
    public static class MessageHelper
    {
        /// <summary>
        /// 通知NavigationPage
        /// </summary>
        public static void SendNavigationMessage(params object[] args)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(args));
        }
        /// <summary>
        /// 通知NavigationPage刷新导航栏插件注入
        /// </summary>
        public static void SendNavigationReloadPlugin() {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Navigate", "PluginReload"));
        }
        /// <summary>
        /// 通知NavigationPage跳转到新的页面
        /// </summary>
        /// <param name="page">新页面</param>
        public static void SendNavigationFrame(Type page)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Navigate", "Frame", page));
        }
        /// <summary>
        /// 通知NavigationPage显示对话框
        /// </summary>
        /// <param name="ContentDialog">对话框</param>
        public static void SendShowContentDialog(ContentDialog dialog)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("ContentDialog", dialog));
        }

        /// <summary>
        /// 通知NavigationPage显示顶部元素
        /// </summary>
        /// <param name="UIElement">顶部元素</param>
        public static void SendTopUIElement(UIElement uIElement)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("TopGrid", uIElement));
        } 
        /// <summary>
        /// 通知HomePage刷新元素
        /// </summary>
        public static void SendFilesReload()
        {
            WeakReferenceMessenger.Default.Send(new FilesMessage("Reload"));
        }
        /// <summary>
        /// 通知StatusPage关闭窗口
        /// </summary>
        public static void SendStatusClose()
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage("Close"));
        }
    }
}
