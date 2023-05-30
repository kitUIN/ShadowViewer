using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SharpCompress.Readers;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using CommunityToolkit.WinUI;
namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : Page
    {
        public NavigationViewModel ViewModel { get; set; }
        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel= new NavigationViewModel(ContentFrame, TopGrid);
            NavView.SelectedItem = NavView.MenuItems[0]; 
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
                if (navItemTag == "BookShelf")
                {
                    _page = typeof(BookShelfPage);
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
                List<Task> backgrounds = new List<Task>();
                IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
                IEnumerable<IStorageItem> item2s = items.Where(x => x is StorageFile file && file.IsZip());
                // LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                LoadingControl.IsLoading = true; 

                foreach (IStorageItem item2 in item2s)
                {
                    ZipThumb.Source = null;
                    LoadingProgressBar.IsIndeterminate = true;
                    LoadingProgressBar.Value = 0;
                    LoadingProgressText.Visibility = Visibility.Visible;
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportDecompress");
                    LoadingFileName.Text = (item2 as StorageFile).Name;
                    ReaderOptions options = null;
                    bool skip = false;
                    bool flag = false;

                    await Task.Run(() => {
                        flag = CompressHelper.CheckPassword(item2.Path, options);
                    });
                    while (!flag)
                    {
                        ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog("解压密码", XamlRoot, "", "密码", "解压密码");
                        dialog.PrimaryButtonClick += (ContentDialog s, ContentDialogButtonClickEventArgs e) =>
                        {
                            string password = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                            options = new ReaderOptions() { Password = password };
                        };
                        dialog.CloseButtonClick += (ContentDialog s, ContentDialogButtonClickEventArgs e) =>
                        {
                            skip = true;
                            flag = true;
                        };
                        await dialog.ShowAsync();
                       
                        if (skip) break;
                        await Task.Run(() => {
                            flag = CompressHelper.CheckPassword(item2.Path, options);
                        }); 
                    }
                    if (skip) continue;
                    string comicId = LocalComic.RandomId();
                    await Task.Run(async () => {
                        object res = await CompressHelper.DeCompressAsync(item2.Path, Config.ComicsPath, comicId,
                        imgAction: new Progress<MemoryStream>(
                            (MemoryStream ms) => DispatcherQueue.EnqueueAsync(async () => {
                                BitmapImage bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());
                                ZipThumb.Source = bitmapImage;
                            })),
                        progress: new Progress<double>((value) => DispatcherQueue.TryEnqueue(() => LoadingProgressBar.Value = value)),
                        () => { 
                            DispatcherQueue.TryEnqueue(() => {
                                LoadingProgressBar.IsIndeterminate = false;
                        }); }, options);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressText.Visibility = Visibility.Collapsed;
                            LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                        });
                        if (res is CacheZip cache)
                        {
                            StorageFolder folder =  await cache.CachePath.ToStorageFolder();
                            await ComicHelper.ImportComicsFromFolder(folder, "local", cache.ComicId,
                                cache.Name);
                        }
                        else if(res is ShadowEntry root)
                        {
                            string path = Path.Combine(Config.ComicsPath, comicId);
                            string fileName = Path.GetFileNameWithoutExtension(item2.Path).Split(new char[] { '\\', '/' },StringSplitOptions.RemoveEmptyEntries).Last();
                            LocalComic comic = LocalComic.Create(fileName, path, img: ComicHelper.LoadImgFromEntry(root, path),
                                parent: "local", size: root.Size, id: comicId);
                            comic.Add();
                            ShadowEntry.ToLocalComic(root, path, comic.Id);
                            // 销毁资源
                            root.Dispose();
                        }
                    });
                }
                MessageHelper.SendFilesReload();
                LoadingControl.IsLoading = false;
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
