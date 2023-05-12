using Microsoft.UI.Xaml.Media.Animation;
using ShadowViewer.DataBases;
using ShadowViewer.Messages;

namespace ShadowViewer.Helpers
{
    public static class MessageHelper
    {

        /// <summary>
        /// 通知NavigationPage刷新导航栏插件注入
        /// </summary>
        public static void SendNavigationReloadPlugin() {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("PluginReload"));
        }
        /// <summary>
        /// 通知NavigationPage跳转到新的页面
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="arg">The argument.</param>
        public static void SendNavigationFrame(Type page,object parameter=null, NavigationTransitionInfo arg=null)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Navigate", page, parameter, arg));
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
        /// <summary>
        /// 通知StatusPage更新属性
        /// </summary>
        public static void SendStatusReload()
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage("Reload"));
        } /// <summary>
        /// 通知StatusPage更新属性
        /// </summary>
        public static void SendStatusReloadDB()
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage("ReloadDB"));
        }
        
    }
}
