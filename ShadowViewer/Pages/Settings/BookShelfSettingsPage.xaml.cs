namespace ShadowViewer.Pages
{
    public sealed partial class BookShelfSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        public BookShelfSettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
        }
    }
}
