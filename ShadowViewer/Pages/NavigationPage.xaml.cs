using SharpCompress.Readers;
using CommunityToolkit.WinUI;
using System.Threading;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using ShadowViewer.ToolKits;
using ShadowViewer.Interfaces;
using System.Security.Policy;

namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : Page
    {
        public static ILogger Logger { get; } = Log.ForContext<NavigationPage>();
        private static CancellationTokenSource cancelTokenSource;
        public NavigationViewModel ViewModel { get; }
        private ICallableToolKit _navigationToolKit;
        public NavigationPage()
        {
            this.InitializeComponent();
            _navigationToolKit = DIFactory.Current.Services.GetService<ICallableToolKit>();
            _navigationToolKit.NavigateToEvent += _navigationToolKit_NavigateTo;
            Logger.Debug("加载NavigateTo事件");
            ViewModel = DIFactory.Current.Services.GetService<NavigationViewModel>();
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void _navigationToolKit_NavigateTo(object sender, NavigateToEventArgs e)
        {
            if (e.Mode == NavigateMode.URL)
            {
                LocalComic comic = DBHelper.Db.Queryable<LocalComic>().First(x => x.Id == e.Id);
                if (comic.IsFolder)
                {
                    DispatcherQueue.EnqueueAsync(() =>
                    {
                        ContentFrame.Navigate(typeof(BookShelfPage), e.Url);
                    });
                }
                else
                {

                }
            }
            else
            {
                DispatcherQueue.EnqueueAsync(() =>
                {
                    ContentFrame.Navigate(e.Page, e.Url);
                });
            };
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
                    /*foreach (string name in PluginHelper.EnabledPlugins)
                    {
                        PluginHelper.PluginInstances[name].NavigationViewItemInvokedHandler(navItemTag, out _page,out parameter);
                        if (_page != null) break;
                    }*/
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

                IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
                LoadingControl.IsLoading = true;
                try
                {
                    foreach (IStorageItem item in items)
                    {
                        cancelTokenSource = new CancellationTokenSource();
                        ZipThumb.Source = null;
                        if (item is StorageFile file && file.IsZip())
                        {
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressBar.Value = 0;
                            LoadingProgressText.Visibility = LoadingProgressBar.Visibility = Visibility.Visible;
                            LoadingControlText.Text = AppResourcesToolKit.GetString("Shadow.String.ImportDecompress");
                            LoadingFileName.Text = file.Name;
                            bool again = false;
                            await Task.Run(() => DispatcherQueue.EnqueueAsync(async () => again = await ComicHelper.ImportAgainDialog(XamlRoot, zip: file.Path)));
                            if (again)
                            {
                                continue;
                            }
                            ReaderOptions options = new ReaderOptions();
                            bool skip = false;
                            bool flag = false;
                            await Task.Run(() => {
                                flag = CompressHelper.CheckPassword(file.Path, ref options);
                            });
                            while (!flag)
                            {
                                ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog(AppResourcesToolKit.GetString("Shadow.String.ZipPasswordTitle"), XamlRoot, "", AppResourcesToolKit.GetString("Shadow.String.ZipPasswordTitle"), AppResourcesToolKit.GetString("Shadow.String.ZipPasswordTitle"));
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
                                    flag = CompressHelper.CheckPassword(file.Path, ref options);
                                });
                            }
                            if (skip) continue;
                            string comicId = LocalComic.RandomId();
                            await Task.Run(async () => {
                                object res = await CompressHelper.DeCompressAsync(file.Path, Config.ComicsPath, comicId,
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
                                    });
                                }, cancelTokenSource.Token, options);
                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    LoadingProgressBar.IsIndeterminate = true;
                                    LoadingProgressText.Visibility = Visibility.Collapsed;
                                    LoadingControlText.Text = AppResourcesToolKit.GetString("Shadow.String.ImportLoading");
                                });
                                if (res is CacheZip cache)
                                {
                                    StorageFolder folder = await cache.CachePath.ToStorageFolder();
                                    await Task.Run(async () => {
                                        try
                                        {
                                            await ComicHelper.ImportComicsFromFolder(folder, "local", cache.ComicId, cache.Name);
                                        }
                                        catch (Exception)
                                        {
                                            Log.Error("无效文件夹{F},忽略", folder.Path);
                                        }
                                    }, cancelTokenSource.Token);
                                }
                                else if (res is ShadowEntry root)
                                {
                                    string path = Path.Combine(Config.ComicsPath, comicId);
                                    string fileName = Path.GetFileNameWithoutExtension(file.Path).Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                    LocalComic comic = LocalComic.Create(fileName, path, img: ComicHelper.LoadImgFromEntry(root, path, comicId),
                                        parent: "local", size: root.Size, id: comicId);
                                    comic.Add();
                                    await Task.Run(() => ShadowEntry.ToLocalComic(root, path, comic.Id), cancelTokenSource.Token);
                                }
                            }, cancelTokenSource.Token);
                        }
                        else if(item is StorageFolder folder)
                        {
                            bool again = false;
                            await Task.Run(() => DispatcherQueue.EnqueueAsync(async () => again = !Config.IsImportAgain && await ComicHelper.ImportAgainDialog(XamlRoot, path: folder.Path)));
                            if (again)
                            {
                                LoadingControl.IsLoading = false;
                                continue;
                            }
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressText.Visibility = Visibility.Collapsed;
                            LoadingControlText.Text = AppResourcesToolKit.GetString("Shadow.String.ImportLoading");
                            LoadingFileName.Text = folder.Name;
                            await Task.Run(async () => {
                                try
                                {
                                    await ComicHelper.ImportComicsFromFolder(folder, "local");
                                }
                                catch (Exception)
                                {
                                    Log.Warning("导入无效文件夹:{F},忽略", folder.Path);
                                }
                            }, cancelTokenSource.Token);
                        }
                        else
                        {
                            Log.Warning("导入无效文件:{F},忽略", item.Path);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    Log.ForContext<NavigationPage>().Information("中断导入");
                }
                _navigationToolKit.RefreshBook();
                LoadingControl.IsLoading = false;
            }
        }
        /// <summary>
        /// 外部文件拖动悬浮显示
        /// </summary>
        private void Root_DragOver(object sender, DragEventArgs e)
        {

            if (e.DataView.Contains(StandardDataFormats.StorageItems) && !LoadingControl.IsLoading)
            {
                e.AcceptedOperation = DataPackageOperation.Link;
                e.DragUIOverride.Caption = AppResourcesToolKit.GetString("Shadow.String.Import");
                OverBorder.Visibility = Visibility.Visible;
                OverBorder.Width = Root.ActualWidth - 30;
                OverBorder.Height = Root.ActualHeight - 30;
                ImportText.Text = AppResourcesToolKit.GetString("Shadow.String.ImportText");
            }
        }
        /// <summary>
        /// 外部文件拖动离开
        /// </summary>
        private void Root_DragLeave(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
        }
        private void SmokeGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        /// <summary>
        /// 取消导入
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
        }
    }

}
