namespace ShadowViewer.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }
        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<SettingsViewModel>();
            ContentFrame.Navigate(typeof(MainSettingsPage));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
             
        }
        private void TryGoBack(object o,object e)
        {
            if (!ContentFrame.CanGoBack)
            {
                DIFactory.Current.Services.GetService<ICallableToolKit>().MainBack(false);
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
