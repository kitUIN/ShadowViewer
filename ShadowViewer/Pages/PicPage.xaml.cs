namespace ShadowViewer.Pages
{

    public sealed partial class PicPage : Page
    {
        public PicViewModel ViewModel { get; set; }
        public PicPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is LocalComic comic)
            {
                ViewModel = new PicViewModel(comic);
            }
        }
        

        private void ScrollViewer_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.PageDown)
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;
                scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + scrollViewer.ViewportHeight, null);
            }
        }
    }
}
