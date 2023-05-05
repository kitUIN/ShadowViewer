// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

namespace ShadowViewer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {
        private NavigationViewModel viewModel;
        public NavigationPage()
        {
            this.InitializeComponent();
            viewModel = new NavigationViewModel(ContentFrame, this.XamlRoot, TopGrid);
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(HomePage));
        }
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type _page = null;
            if (args.IsSettingsInvoked == true)
            {
                _page = typeof(SettingsPage);
            }
            else if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is string navItemTag)
            {
                if (navItemTag == "Home")
                {
                    _page = typeof(HomePage);
                }
                else
                {
                    foreach (string name in PluginHelper.EnabledPlugins)
                    {
                        _page = PluginHelper.PluginInstances[name].NavigationViewItemInvokedHandler(navItemTag);
                        if (_page != null) break;
                    }
                }
            }
            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
                return false;
            ContentFrame.GoBack();
            return true;
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.LoadPluginItems(PluginItem);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is Page page)
            {
                ContentFrame.Content = page;
            }
             
        }

        private void NavView_Drop(object sender, DragEventArgs e)
        {

        }
    }

}
