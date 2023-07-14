namespace ShadowViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "ShadowViewer";
            this.SetTitleBar(AppTitleBar);
        }
    }
}
