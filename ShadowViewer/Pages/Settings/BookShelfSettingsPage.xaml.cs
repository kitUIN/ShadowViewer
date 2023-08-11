using DryIoc;

namespace ShadowViewer.Pages
{
    public sealed partial class BookShelfSettingsPage : Page
    {
        private SettingsViewModel ViewModel { get;  }
        private ICallableService Caller { get;  }
        public BookShelfSettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DiFactory.Services.Resolve<SettingsViewModel>();
            Caller = DiFactory.Services.Resolve<ICallableService>();
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
