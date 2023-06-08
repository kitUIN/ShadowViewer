namespace ShadowViewer.Pages
{

    public sealed partial class BookShelfSettingsPage : Page
    {
        public BookShelfSettingsViewModel ViewModel { get; set; }
        public BookShelfSettingsPage()
        {
            this.InitializeComponent();
            ViewModel = new BookShelfSettingsViewModel();
        }
    }
}
