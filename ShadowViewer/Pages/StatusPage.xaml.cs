namespace ShadowViewer.Pages
{
    public sealed partial class StatusPage : Page
    { 
        private LocalComic comic;
        public StatusPage()
        {
            this.InitializeComponent();
            this.RequestedTheme = ThemeHelper.RootTheme;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is LocalComic comic)
            {
                this.comic = comic;
            }
        }
    }
}
