using CommunityToolkit.WinUI;
using ShadowViewer.Utils.Args;
using SharpCompress.Readers;
using System.Threading;

namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : Page
    {
        public static ILogger Logger { get; } = Log.ForContext<NavigationPage>();
        private static CancellationTokenSource _cancelTokenSource;
        private NavigationViewModel ViewModel { get; }
        private ICallableToolKit Caller { get; }
        private IPluginsToolKit PluginsToolKit { get; }
        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel = DIFactory.Current.Services.GetService<NavigationViewModel>();
            Caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            PluginsToolKit = DIFactory.Current.Services.GetService<IPluginsToolKit>();
            Caller.ImportComicEvent += Caller_ImportComicEvent;
            Caller.ImportComicProgressEvent += Caller_ImportComicProgressEvent;
            Caller.ImportComicErrorEvent += Caller_ImportComicErrorEvent;
            Caller.ImportComicThumbEvent += Caller_ImportComicThumbEvent;
            Caller.ImportComicCompletedEvent += Caller_ImportComicCompletedEvent;
            Caller.NavigateToEvent += Caller_NavigationToolKit_NavigateTo;
            Caller.PluginEnabledEvent += CallerOnPluginEnabledEvent;
            Caller.PluginDisabledEvent += CallerOnPluginEnabledEvent;
            NavView.SelectedItem = NavView.MenuItems[0];
        }
        /// <summary>
        /// 启用或禁用插件时更新左侧导航栏
        /// </summary>
        private void CallerOnPluginEnabledEvent(object sender, PluginEventArg e)
        {
            ViewModel.LoadPluginItems(PluginItem);
        }

        /// <summary>
        /// 导入完成
        /// </summary>
        private void Caller_ImportComicCompletedEvent(object sender, EventArgs e)
        {
            DIFactory.Current.Services.GetService<ICallableToolKit>().RefreshBook();
            LoadingControl.IsLoading = false;
        }
        /// <summary>
        /// 导入的缩略图
        /// </summary>
        private async void Caller_ImportComicThumbEvent(object sender, ImportComicThumbEventArgs e)
        {
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(e.Thumb.AsRandomAccessStream());
            ZipThumb.Source = bitmapImage;
        }
        /// <summary>
        /// 导入失败
        /// </summary>
        private async void Caller_ImportComicErrorEvent(object sender, ImportComicErrorEventArgs args)
        {
            var caller = DIFactory.Current.Services.GetService<ICallableToolKit>();
            if (args.Error != ImportComicError.Password) return;
            var dialog = XamlHelper.CreateOneLineTextBoxDialog(args.Message, XamlRoot);
            dialog.PrimaryButtonClick += (s, e) =>
            {
                // 重新开始
                var password = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                args.Password[args.Index] = password == "" ? null : password;
                caller.ImportComic(args.Items, args.Password, args.Index);
            };
            dialog.CloseButtonClick += (s, e) =>
            {
                // 跳过本个
                if (args.Items.Count > args.Index + 1)
                {
                    caller.ImportComic(args.Items, args.Password, args.Index + 1);
                }
                else
                {
                    caller.ImportComicCompleted();
                }
            };
            await dialog.ShowAsync();
        }
        /// <summary>
        /// 导入进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Caller_ImportComicProgressEvent(object sender, ImportComicProgressEventArgs e)
        {
            if (LoadingProgressBar.IsIndeterminate)
            {
                LoadingProgressBar.IsIndeterminate = false;
            }
            LoadingProgressBar.Value = e.Progress;
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
                for (var i = e.Index; i < e.Items.Count;i++)
                {
                    var item = e.Items[i];
                    _cancelTokenSource = new CancellationTokenSource();
                    if (item is StorageFile file && file.IsZip())
                    {
                        ZipThumb.Source = null;
                        LoadingControl.IsLoading = true;
                        LoadingProgressBar.IsIndeterminate = true;
                        LoadingProgressBar.Value = 0;
                        LoadingProgressText.Visibility = LoadingProgressBar.Visibility = Visibility.Visible;
                        LoadingControlText.Text = ResourcesHelper.GetString("Shadow.String.ImportDecompress");
                        LoadingFileName.Text = file.Name;
                        var options = new ReaderOptions
                        {
                            Password = e.Passwords[i], 
                        };
                        var flag = CompressToolKit.CheckPassword(file.Path, ref options);
                        if (!flag)
                        {
                            DIFactory.Current.Services.GetService<ICallableToolKit>().ImportComicError(ImportComicError.Password, "密码错误",e.Items, i,e.Passwords);
                            return;
                        }
                        await Task.Run(() => {
                            flag = CompressToolKit.CheckPassword(file.Path, ref options);
                        });
                        var comicId = LocalComic.RandomId();
                         
                        await Task.Run(async () => {
                            var res = await DIFactory.Current.Services.GetService<CompressToolKit>().DeCompressAsync(file.Path, Config.ComicsPath, comicId,  _cancelTokenSource.Token, options);
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoadingProgressBar.IsIndeterminate = true;
                                LoadingProgressText.Visibility = Visibility.Collapsed;
                                LoadingControlText.Text = ResourcesHelper.GetString("Shadow.String.ImportLoading");
                            });
                            switch (res)
                            {
                                case CacheZip cache:
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
                                    }, _cancelTokenSource.Token);
                                    break;
                                }
                                case ShadowEntry root:
                                {
                                    var path = Path.Combine(Config.ComicsPath, comicId);
                                    var fileName = Path.GetFileNameWithoutExtension(file.Path)?.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                    var comic = LocalComic.Create(fileName, path, img: ComicHelper.LoadImgFromEntry(root, path, comicId),
                                        parent: "local", size: root.Size, id: comicId);
                                    comic.Add();
                                    await Task.Run(() => ShadowEntry.ToLocalComic(root, path, comic.Id), _cancelTokenSource.Token);
                                    break;
                                }
                            }
                        }, _cancelTokenSource.Token);
                    }
                    else if (item is StorageFolder folder)
                    {
                        var again = false;
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
                            LoadingControlText.Text = ResourcesHelper.GetString("Shadow.String.ImportLoading");
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
                        }, _cancelTokenSource.Token);
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
                var comic = DBHelper.Db.Queryable<LocalComic>().First(x => x.Id == e.Id);
                if (comic.IsFolder)
                {
                    DispatcherQueue.EnqueueAsync(() =>
                    {
                        ContentFrame.Navigate(typeof(BookShelfPage), e.Url);
                        NavView.SelectedItem = NavView.MenuItems.Cast<FrameworkElement>()
                        .FirstOrDefault(x => x.Tag.ToString() +"Page" == nameof(BookShelfPage));
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
            Type page = null;
            object parameter = null;
            if (args.IsSettingsInvoked)
            {
                page = typeof(SettingsPage);
                parameter = new Uri("shadow://settings/");
            }
            else if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is string navItemTag)
            {
                switch (navItemTag)
                {
                    case "BookShelf":
                        page = typeof(BookShelfPage);
                        parameter = new Uri("shadow://local/");
                        break;
                    case "Download":
                        page = typeof(DownloadPage);
                        break;
                    case "User":
                        break;
                    case "Plugins":
                        page = typeof(PluginPage);
                        break;
                    default:
                    {
                        foreach (var p in PluginsToolKit.EnabledPlugins)
                        {
                            p.NavigationViewItemInvokedHandler(navItemTag, out page,out parameter);
                            if (page != null) break;
                        }
                        break;
                    }
                }
            }
            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (page is not null && !Type.Equals(preNavPageType, page))
            {
                ContentFrame.Navigate(page, parameter, args.RecommendedNavigationTransitionInfo);
            }
        }
        /// <summary>
        /// 后退按钮
        /// </summary>
        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (!ContentFrame.CanGoBack) return;
            ContentFrame.GoBack();
        }
         

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        /// <summary>
        /// 外部文件拖入进行响应
        /// </summary>
        private async void Root_Drop(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && !LoadingControl.IsLoading)
            {
                var items = await e.DataView.GetStorageItemsAsync();
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
                e.DragUIOverride.Caption = ResourcesHelper.GetString("Shadow.String.Import");
                OverBorder.Visibility = Visibility.Visible;
                OverBorder.Width = Root.ActualWidth - 30;
                OverBorder.Height = Root.ActualHeight - 30;
                ImportText.Text = ResourcesHelper.GetString("Shadow.String.ImportText");
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
            _cancelTokenSource.Cancel();
        }
    }

}
