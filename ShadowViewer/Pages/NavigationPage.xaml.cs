using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.UI.Core;

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
                    LoadingProgressBar.IsIndeterminate = false;
                    LoadingProgressText.Visibility = Visibility.Visible;
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportDecompress");
                    LoadingFileName.Text = (item2 as StorageFile).Name;
                    await Task.Run(async ()=> {
                        Tuple<ShadowEntry, CacheZip> tuple;
                        var comicPath = Config.ComicsPath;
                        var zip = item2.Path;
                        string comicId = LocalComic.RandomId();
                        string path = Path.Combine(comicPath, comicId);
                        string password = "";
                        try
                        {
                            tuple = await CompressHelper.DeCompressAsync(zip, path,
                                new Progress<MemoryStream>(
                                    (ms) => DispatcherQueue.TryEnqueue(async () => {
                                        BitmapImage bitmapImage = new BitmapImage();
                                        await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());
                                        ZipThumb.Source = bitmapImage;
                                    })),
                                new Progress<double>((value) => DispatcherQueue.TryEnqueue(() => LoadingProgressBar.Value = value)));
                        }
                        catch (SharpCompress.Common.CryptographicException)
                        {
                            Log.Error("{Path}需要解压密码", zip);
                            // 尝试读取记录代码
                            while (true)
                            {
                                password = await ComicHelper.ZipPasswordDialog(XamlRoot);
                                try
                                {
                                    tuple = await CompressHelper.DeCompressAsync(zip, path,
                                         new Progress<MemoryStream>(
                                    (ms) => DispatcherQueue.TryEnqueue(async () => {
                                        BitmapImage bitmapImage = new BitmapImage();
                                        await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());
                                        ZipThumb.Source = bitmapImage;
                                    })),
                                        new Progress<double>((value) => DispatcherQueue.TryEnqueue(() => LoadingProgressBar.Value = value)),
                                        new SharpCompress.Readers.ReaderOptions { Password = password });
                                    //TODO:保存密码
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("{Path}解压密码出错:{Pwd}{ex}", zip, password, ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("解压出错:{Ex}", ex);
                            return;
                        }
                    DispatcherQueue.TryEnqueue(() => { 
                        LoadingProgressBar.IsIndeterminate = true;
                        LoadingProgressText.Visibility = Visibility.Collapsed;
                        LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                        });
                    if (tuple.Item1 != null)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(zip).Split(new char[] { '\\', '/' },
                            StringSplitOptions.RemoveEmptyEntries).Last();
                            LocalComic comic = LocalComic.Create(fileName, path, img: ComicHelper.LoadImgFromEntry(tuple.Item1, path),
                                parent: "local", size: tuple.Item1.Size, id: comicId);
                            comic.Add();
                            ShadowEntry.ToLocalComic(tuple.Item1, path, comic.Id);
                            // 销毁资源
                            tuple.Item1.Dispose();
                            GC.SuppressFinalize(tuple.Item1);
                        }
                        else
                        {

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
