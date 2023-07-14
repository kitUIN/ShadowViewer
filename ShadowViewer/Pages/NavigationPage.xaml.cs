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
using ShadowViewer.Extensions;

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
        /// <summary>
        /// 导入完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Caller_ImportComicCompletedEvent(object sender, EventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                DIFactory.Current.Services.GetService<ICallableToolKit>().RefreshBook();
                LoadingControl.IsLoading = false;
            });
            
        }
        /// <summary>
        /// 导入的缩略图
        /// </summary>
        private void Caller_ImportComicThumbEvent(object sender, ImportComicThumbEventArgs e)
        {
            DispatcherQueue.EnqueueAsync(async () =>
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(e.Thumb.AsRandomAccessStream());
                ZipThumb.Source = bitmapImage;
            });
        }
        /// <summary>
        /// 导入失败
        /// </summary>
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
        /// <summary>
        /// 导入进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 开始导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Caller_ImportComicEvent(object sender, ImportComicEventArgs e)
        {
            try
            {
                for (int i = e.Index; i < e.Items.Count;i++)
                {
                    var item = e.Items[i];
                    cancelTokenSource = new CancellationTokenSource();
                    if (item is StorageFile file && file.IsZip())
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ZipThumb.Source = null;
                            LoadingControl.IsLoading = true;
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressBar.Value = 0;
                            LoadingProgressText.Visibility = LoadingProgressBar.Visibility = Visibility.Visible;
                            LoadingControlText.Text = AppResourcesHelper.GetString("Shadow.String.ImportDecompress");
                            LoadingFileName.Text = file.Name;
                        });
                        ReaderOptions options = new ReaderOptions();
                        options.Password = e.Passwords[i];
                        bool flag = CompressToolKit.CheckPassword(file.Path, ref options);
                        if (!flag)
                        {
                            DIFactory.Current.Services.GetService<ICallableToolKit>().ImportComicError(ImportComicError.Password, "密码错误",e.Items, i,e.Passwords);
                            return;
                        }
                        await Task.Run(() => {
                            flag = CompressToolKit.CheckPassword(file.Path, ref options);
                        });
                        string comicId = LocalComic.RandomId();
                         
                        await Task.Run(async () => {
                            object res = await DIFactory.Current.Services.GetService<CompressToolKit>().DeCompressAsync(file.Path, Config.ComicsPath, comicId,  cancelTokenSource.Token, options);
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoadingProgressBar.IsIndeterminate = true;
                                LoadingProgressText.Visibility = Visibility.Collapsed;
                                LoadingControlText.Text = AppResourcesHelper.GetString("Shadow.String.ImportLoading");
                            });
                            if (res is CacheZip cache)
                            {
                                StorageFolder folder = await cache.CachePath.ToStorageFolder();
                                await Task.Run(() => {
                                    try
                                    {
                                        ComicHelper.ImportComicsFromFolder(folder, "local", cache.ComicId, cache.Name);
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
                        await Task.Run(() => DispatcherQueue.EnqueueAsync(async () => again =  await ComicHelper.CheckImportAgain(XamlRoot, path: folder.Path)));
                        if (again)
                        {
                            DispatcherQueue.TryEnqueue(() => LoadingControl.IsLoading = false);
                            continue;
                        } 
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            LoadingControl.IsLoading = true;
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressText.Visibility = Visibility.Collapsed;
                            LoadingControlText.Text = AppResourcesHelper.GetString("Shadow.String.ImportLoading");
                            LoadingFileName.Text = folder.Name;
                        }); 
                       
                        await Task.Run(  () => {
                            try
                            {
                                ComicHelper.ImportComicsFromFolder(folder, "local");
                            }
                            catch (Exception ex)
                            {
                                Log.Warning("导入无效文件夹:{F},忽略\n{ex}", folder.Path, ex);
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
