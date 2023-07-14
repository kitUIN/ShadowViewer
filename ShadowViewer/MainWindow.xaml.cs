namespace ShadowViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "ShadowViewer";
            this.SetTitleBar(AppTitleBar);
            DebugIcon.Visibility = Config.IsDebug.ToVisibility();
            DIFactory.Current.Services.GetService<ICallableToolKit>().DebugEvent += MainWindow_DebugEvent;
        }

        private void MainWindow_DebugEvent(object sender, EventArgs e)
        {
            DebugIcon.Visibility = Config.IsDebug.ToVisibility();
        }
    }
}
