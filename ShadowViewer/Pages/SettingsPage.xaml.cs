namespace ShadowViewer.Pages
{
    public sealed partial class SettingsPage :  Page
    {
        private SettingsViewModel ViewModel { get; }
        private ICallableToolKit Caller { get; }
        public SettingsPage(): base() 
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
            Caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            ContentFrame.Navigate(typeof(MainSettingsPage));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Caller.SettingsBackEvent += TryGoBack;
            Caller.NavigateTo(NavigateMode.Type, e.SourcePageType, ResourcesHelper.GetString(ResourceKey.Settings), null);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Caller.SettingsBackEvent -= TryGoBack;
        }
        public void TryGoBack(object sender, EventArgs e)
        {
            if (!ContentFrame.CanGoBack)
            {
                Caller.MainBack(false);
            }
            else
            {
                ContentFrame.GoBack();
            }
        }
        private void TopBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            for (int i = ViewModel.Pages.Count - 1; i >= args.Index + 1; i--)
            {
                ViewModel.Pages.RemoveAt(i);
            }
            ContentFrame.Navigate(ViewModel.Pages.Last().Type, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }) ;
        }
    }
}
