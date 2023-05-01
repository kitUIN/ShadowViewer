using Microsoft.UI;
using Microsoft.UI.Windowing;
using ShadowViewer.Plugins;
using ShadowViewer.ViewModels;
using Windows.Graphics;

namespace ShadowViewer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "ShadowViewer";
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            
            RootFrame.Navigate(typeof(NavigationPage));
        }
    }
}
