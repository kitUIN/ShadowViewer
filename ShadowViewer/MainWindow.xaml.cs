namespace ShadowViewer
{
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
