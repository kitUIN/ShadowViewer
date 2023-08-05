namespace ShadowViewer.Pages
{
    public sealed partial class BookShelfSettingsPage : Page
    {
        private SettingsViewModel ViewModel { get;  }
        private ICallableToolKit Caller { get;  }
        public BookShelfSettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DiFactory.Current.Services.GetService<SettingsViewModel>();
            Caller = DiFactory.Current.Services.GetService<ICallableToolKit>();
        }
         
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Caller.NavigateTo(NavigateMode.Type, e.SourcePageType, ResourcesHelper.GetString(ResourceKey.BookShelfSettings), null);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            
        }
    }
}
