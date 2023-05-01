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
            this.Title = "ShadowViewer-File";
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            var ScreenHeight = DisplayArea.Primary.WorkArea.Height;
            var ScreenWidth = DisplayArea.Primary.WorkArea.Width;
            this.AppWindow.MoveAndResize(new RectInt32(ScreenWidth / 2 - 250, ScreenHeight / 2 - 350, 500, 700));
            
        }
        public void Navigate(Type page,LocalComic comic)
        {
            RootFrame.Navigate(page,comic);
        }


    }
}
