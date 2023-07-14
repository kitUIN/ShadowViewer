using SharpCompress.Readers;
using CommunityToolkit.WinUI;
using System.Threading;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using ShadowViewer.ToolKits;
using ShadowViewer.Interfaces;
using System.Security.Policy;
using ShadowViewer.Utils.Args;
using Microsoft.UI.Xaml;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.IO;
using System;

namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : Page
    {
        public static ILogger Logger { get; } = Log.ForContext<NavigationPage>();
        private static CancellationTokenSource cancelTokenSource;
        public NavigationViewModel ViewModel { get; }
        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<NavigationViewModel>();
            ICallableToolKit caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            caller.ImportComicEvent += Caller_ImportComicEvent;
            caller.ImportComicProgressEvent += Caller_ImportComicProgressEvent;
            caller.ImportComicErrorEvent += Caller_ImportComicErrorEvent;
            caller.ImportComicThumbEvent += Caller_ImportComicThumbEvent;
            caller.ImportComicCompletedEvent += Caller_ImportComicCompletedEvent;
            caller.NavigateToEvent += Caller_NavigationToolKit_NavigateTo;
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void Caller_ImportComicCompletedEvent(object sender, EventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                DIFactory.Current.Services.GetService<ICallableToolKit>().RefreshBook();
                LoadingControl.IsLoading = false;
            });
            
        }

        private void Caller_ImportComicThumbEvent(object sender, ImportComicThumbEventArgs e)
        {
            DispatcherQueue.EnqueueAsync(async () =>
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(e.Thumb.AsRandomAccessStream());
                ZipThumb.Source = bitmapImage;
            });
        }

        private async void Caller_ImportComicErrorEvent(object sender, ImportComicErrorEventArgs args)
        {
            ICallableToolKit caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            if (args.Error == ImportComicError.Password)
            { 
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog( args.Message, XamlRoot);
                    dialog.PrimaryButtonClick += (ContentDialog s, ContentDialogButtonClickEventArgs e) =>
                    {
                        // 重新开始
                        string password = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                        args.Password[args.Index] = password == "" ? null: password;
                        caller.ImportComic(args.Items, args.Password, args.Index);
                    };
                    dialog.CloseButtonClick += (ContentDialog s, ContentDialogButtonClickEventArgs e) =>
                    {
                        // 跳过本个
                        if(args.Items.Count > args.Index + 1)
                        {
                            caller.ImportComic(args.Items, args.Password, args.Index + 1);
                        }
                        else
                        {
                            caller.ImportComicCompleted();
                        }
                    };
                    await dialog.ShowAsync();
                });
            }
        }

        private void Caller_ImportComicProgressEvent(object sender, ImportComicProgressEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (LoadingProgressBar.IsIndeterminate)
                {
                    LoadingProgressBar.IsIndeterminate = false;
                }
                LoadingProgressBar.Value = e.Progress;
            });
        }

        private async void Caller_ImportComicEvent(object sender, ImportComicEventArgs e)
        {
            try
            {
                for (int i = e.Index; i < e.Items.Count;i++)
                {
                    var item = e.Items[i];
                    cancelTokenSource = new CancellationTokenSource();
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        LoadingControl.IsLoading = true;
                        ZipThumb.Source = null;
                    });
                    if (item is StorageFile file && file.IsZip())
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressBar.Value = 0;
                            LoadingProgressText.Visibility = LoadingProgressBar.Visibility = Visibility.Visible;
                            LoadingControlText.Text = AppResourcesHelper.GetString("Shadow.String.ImportDecompress");
                            LoadingFileName.Text = file.Name;
                        });
                        ReaderOptions options = new ReaderOptions();
                        options.Password = e.Passwords[i];
                        bool flag = CompressHelper.CheckPassword(file.Path, ref options);
                        if (!flag)
                        {
                            DIFactory.Current.Services.GetService<ICallableToolKit>().ImportComicError(ImportComicError.Password, "密码错误",e.Items, i,e.Passwords);
                            return;
                        }
                        await Task.Run(() => {
                            flag = CompressHelper.CheckPassword(file.Path, ref options);
                        });
                        string comicId = LocalComic.RandomId();
                         
                        await Task.Run(async () => {
                            object res = await DeCompressAsync(file.Path, Config.ComicsPath, comicId,  cancelTokenSource.Token, options);
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoadingProgressBar.IsIndeterminate = true;
                                LoadingProgressText.Visibility = Visibility.Collapsed;
                                LoadingControlText.Text = AppResourcesHelper.GetString("Shadow.String.ImportLoading");
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
                    else if (item is StorageFolder folder)
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
                        LoadingControlText.Text = AppResourcesHelper.GetString("Shadow.String.ImportLoading");
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
                DIFactory.Current.Services.GetService<ICallableToolKit>().ImportComicCompleted();
            }
            catch (TaskCanceledException)
            {
                Log.ForContext<NavigationPage>().Information("中断导入");
                DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingControl.IsLoading = false;
                });
            }
        }
        public async Task<object> DeCompressAsync(string zip, string destinationDirectory,
            string comicId, CancellationToken token, ReaderOptions readerOptions = null)
        {
            ICallableToolKit caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            Logger.Information("进入解压流程");
            string path = Path.Combine(destinationDirectory, comicId);
            DateTime start;
            ShadowEntry root = new ShadowEntry()
            {
                Name = zip.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last(),
            };
            string md5 = EncryptingHelper.CreateMd5(zip);
            string sha1 = EncryptingHelper.CreateSha1(zip);
            CacheZip cacheZip = DBHelper.Db.Queryable<CacheZip>().First(x => x.Sha1 == sha1 && x.Md5 == md5);
            cacheZip ??= CacheZip.Create(md5, sha1);
            if (cacheZip.ComicId != null)
            {
                comicId = cacheZip.ComicId;
                path = Path.Combine(destinationDirectory, comicId);
                // 缓存文件未被删除
                if (Directory.Exists(cacheZip.CachePath))
                {
                    Logger.Information("{Zip}文件存在缓存记录,直接载入漫画{cid}", zip, cacheZip.ComicId);
                    return cacheZip;
                }
            }
            if (token.IsCancellationRequested) throw new TaskCanceledException();
            using (FileStream fStream = File.OpenRead(zip))
            using (NonDisposingStream stream = NonDisposingStream.Create(fStream))
            {
                if (token.IsCancellationRequested) throw new TaskCanceledException();
                using IArchive archive = ArchiveFactory.Open(stream, readerOptions);
                if (token.IsCancellationRequested) throw new TaskCanceledException();
                IEnumerable<IArchiveEntry> total = archive.Entries.Where(entry => !entry.IsDirectory && entry.Key.IsPic()).OrderBy(x => x.Key);
                if (token.IsCancellationRequested) throw new TaskCanceledException();
                int totalCount = total.Count();
                MemoryStream ms = new MemoryStream();
                if (total.FirstOrDefault() is IArchiveEntry img)
                {
                    using (Stream entryStream = img.OpenEntryStream())
                    {
                        await entryStream.CopyToAsync(ms);
                        // ms.Seek(0, SeekOrigin.Begin);
                    }
                    byte[] bytes = ms.ToArray();
                    CacheImg.CreateImage(destinationDirectory, bytes, comicId);
                    caller.ImportComicThumb(new MemoryStream(bytes));
                }
                Logger.Information("开始解压:{Zip}", zip);
                start = DateTime.Now;
                int i = 0;
                path.CreateDirectory();
                foreach (IArchiveEntry entry in total)
                {
                    if (token.IsCancellationRequested) throw new TaskCanceledException();
                    entry.WriteToDirectory(path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    i++;
                    double result = (double)i / (double)totalCount;
                    caller.ImportComicProgress(Math.Round(result * 100, 2));
                    ShadowEntry.LoadEntry(entry, root);
                }
                root.LoadChildren();
                DateTime stop = DateTime.Now;
                cacheZip.ComicId = comicId;
                cacheZip.CachePath = path;
                cacheZip.Name = Path.GetFileNameWithoutExtension(zip).Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                if (DBHelper.Db.Queryable<CacheZip>().Any(x => x.Id == cacheZip.Id))
                {
                    cacheZip.Update();
                }
                else
                {
                    cacheZip.Add();
                }
                Logger.Information("解压:{Zip} 页数:{Pages} 耗时: {Time} s", zip, totalCount, (stop - start).TotalSeconds);
            }
            return root;
        }
        private void Caller_NavigationToolKit_NavigateTo(object sender, NavigateToEventArgs e)
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
                var passwords = new string[items.Count];
                DIFactory.Current.Services.GetService<ICallableToolKit>().ImportComic(items, passwords, 0);
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
                e.DragUIOverride.Caption = AppResourcesHelper.GetString("Shadow.String.Import");
                OverBorder.Visibility = Visibility.Visible;
                OverBorder.Width = Root.ActualWidth - 30;
                OverBorder.Height = Root.ActualHeight - 30;
                ImportText.Text = AppResourcesHelper.GetString("Shadow.String.ImportText");
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
