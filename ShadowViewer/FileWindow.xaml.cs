namespace ShadowViewer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileWindow : Window
    {
        public FileWindow()
        {
            this.InitializeComponent(); 
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            var ScreenHeight = DisplayArea.Primary.WorkArea.Height;
            var ScreenWidth = DisplayArea.Primary.WorkArea.Width;
            this.AppWindow.MoveAndResize(new RectInt32(ScreenWidth / 2 - 320, ScreenHeight / 2 - 375, 640, 750));
            
        }
        public void Navigate(Type page, List<object> args, string title)
        {
            RootFrame.Navigate(page, args);
            FileAppTitle.Text = title;
        }


    }
}
