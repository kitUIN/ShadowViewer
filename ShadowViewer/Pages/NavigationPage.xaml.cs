namespace ShadowViewer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {
 
        public NavigationViewModel ViewModel { get; set; }
        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel= new NavigationViewModel(ContentFrame, TopGrid);
            // NavView.SelectedItem = NavView.MenuItems[0]; 
        }
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type _page = null;
            object parameter = null;
            if (args.IsSettingsInvoked == true)
            {
                _page = typeof(SettingsPage);
                parameter = new Uri("shadow://settings/");
            }
            else if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is string navItemTag)
            {
                if (navItemTag == "Home")
                {
                    _page = typeof(HomePage);
                    parameter = new Uri("shadow://local/");
                }
                else if (navItemTag == "Download")
                {
                    _page = typeof(DownloadPage);
                }
                else if (navItemTag == "User")
                {

                }
                else
                {
                    foreach (string name in PluginHelper.EnabledPlugins)
                    {
                        PluginHelper.PluginInstances[name].NavigationViewItemInvokedHandler(navItemTag, out _page,out parameter);
                        if (_page != null) break;
                    }
                }
            }
            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, parameter, args.RecommendedNavigationTransitionInfo);
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
            ViewModel.LoadPluginItems(PluginItem);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is Page page)
            {
                ContentFrame.Content = page;
            } 
        }
        /// <summary>
        /// 外部文件拖入进行响应
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private async void Root_Drop(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && !LoadingControl.IsLoading)
            {
                List<Task> tasks = new List<Task>();
                List<Task> backgrounds = new List<Task>();
                IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
                foreach (IStorageItem item2 in items.Where(x => x is StorageFile file && file.IsZip()))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        ComicHelper.Entrys[item2.Path] = await CompressHelper.ZipDeCompress(item2.Path);
                        ComicHelper.Entrys.Remove(item2.Path);
                        Log.Information("111");
                    }));
                }
                LoadingControl.IsLoading = true;
                LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                await Task.WhenAll(tasks);
                LoadingControl.IsLoading = false;
                /*IsBusy = true;
                LoadingProgressBar.IsIndeterminate = false;
                LoadingProgressBar.Maximum = items.Count;
                LoadingDetail.Visibility = Visibility.Visible;
                foreach (var item1 in items.Where(x => x is StorageFolder))
                { 
                    LoadingControl.IsLoading = true;
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                    await ViewModel.ImportComicsAsync(item1 as StorageFolder);
                    LoadingProgressBar.Value++;
                }
                foreach (var item2 in items.Where(x => x is StorageFile file && file.IsZip()))
                {
                    LoadingControl.IsLoading = true;
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportDecompress");
                    var res = await ViewModel.ImportZipCompress(item2 as StorageFile);
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                    await ViewModel.ImportComicsAsync(res.Item1, res.Item2);
                    LoadingProgressBar.Value++;
                }
                LoadingProgressBar.Value = LoadingProgressBar.Maximum;
                LoadingControl.IsLoading = false;
                IsBusy = false;*/
            }
        }
        /// <summary>
        /// 外部文件拖动悬浮显示
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void Root_DragOver(object sender, DragEventArgs e)
        {

            if (e.DataView.Contains(StandardDataFormats.StorageItems) && !LoadingControl.IsLoading)
            {
                e.AcceptedOperation = DataPackageOperation.Link;
                e.DragUIOverride.Caption = I18nHelper.GetString("Shadow.String.Import");
                OverBorder.Visibility = Visibility.Visible;
                OverBorder.Width = Root.ActualWidth - 30;
                OverBorder.Height = Root.ActualHeight - 30;
                ImportText.Text = I18nHelper.GetString("Shadow.String.ImportText");
            }
        }
        /// <summary>
        /// 外部文件拖动离开
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void Root_DragLeave(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
        }
        private void SmokeGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }

}
