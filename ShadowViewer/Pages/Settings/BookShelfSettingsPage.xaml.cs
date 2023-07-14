namespace ShadowViewer.Pages
{
    public sealed partial class BookShelfSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }
        public BookShelfSettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
            ViewModel.Pages = new ObservableCollection<BreadcrumbItem> {
                new BreadcrumbItem(ResourcesHelper.GetString(ResourceKey.Settings), typeof(MainSettingsPage)),
                new BreadcrumbItem(ResourcesHelper.GetString(ResourceKey.BookShelfSettings), typeof(BookShelfSettingsPage))
            };
        }
    }
}
