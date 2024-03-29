using CommunityToolkit.WinUI;
using SharpCompress.Readers;
using System.Threading;
using SqlSugar;
using System.Diagnostics;
using ShadowViewer.Args;
using ShadowViewer.Responders;
using ShadowViewer.Services;

namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : Page
    {
        public static ILogger Logger { get; } = Log.ForContext<NavigationPage>();
        private static CancellationTokenSource _cancelTokenSource = new();
        private NavigationViewModel ViewModel { get; }
        private ICallableService Caller { get; }
        private IPluginService PluginService { get; }

        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel = DiFactory.Services.Resolve<NavigationViewModel>();
            Caller = DiFactory.Services.Resolve<ICallableService>();
            PluginService = DiFactory.Services.Resolve<IPluginService>();
            Caller.ImportComicEvent += Caller_ImportComicEvent;
            Caller.ImportComicProgressEvent += Caller_ImportComicProgressEvent;
            Caller.ImportComicErrorEvent += Caller_ImportComicErrorEvent;
            Caller.ImportComicThumbEvent += Caller_ImportComicThumbEvent;
            Caller.ImportComicCompletedEvent += Caller_ImportComicCompletedEvent;
            Caller.NavigateToEvent += Caller_NavigationToolKit_NavigateTo;
            Caller.PluginEnabledEvent += CallerOnPluginEnabledEvent;
            Caller.PluginDisabledEvent += CallerOnPluginEnabledEvent;
            Caller.TopGridEvent += Caller_TopGridEvent;
            Caller.ImportPluginEvent += CallerOnImportPluginEvent;
            Caller.NavigationViewBackRequestedEvent += Caller_NavigationViewBackRequestedEvent;
            Caller.NavigationViewPaneEvent += Caller_NavigationViewPaneEvent;
        }

        private void Caller_NavigationViewPaneEvent(object? sender, EventArgs e)
        {
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }

        private void Caller_NavigationViewBackRequestedEvent(object? sender, EventArgs e)
        {
            if (!ContentFrame.CanGoBack) return;
            ContentFrame.GoBack();
        }

        private async void CallerOnImportPluginEvent(object sender, ImportPluginEventArg e)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                var path = ConfigHelper.GetString("PluginsPath");
                foreach (var item in e.Items)
                {
                    if (item is StorageFile file)
                    {
                        var f = file.DisplayName.Split(".")[2];
                        var pluginPath = Path.Combine(path, f);
                        var compressToolKit = DiFactory.Services.Resolve<CompressService>();
                        await Task.Run(async () =>
                        {
                            //compressToolKit.DeCompress(file.Path, pluginPath);
/*                            await DispatcherQueue.EnqueueAsync(async () =>
                            {
                                await PluginService.ImportAsync();
                            });*/
                        });
                    }
                }
            });
            
        }

        /// <summary>
        /// 顶部窗体事件
        /// </summary>
        private async void Caller_TopGridEvent(object sender, TopGridEventArg e)
        {
            try
            {
                switch (e.Mode)
                {
                    case TopGridMode.ContentDialog:
                        if (e.Element is ContentDialog dialog)
                        {
                            dialog.XamlRoot = XamlRoot;
                        
                                await dialog.ShowAsync();

                        }

                        break;
                    case TopGridMode.Dialog:
                        TopGrid.Children.Clear();
                        if (e.Element != null)
                        {
                            TopGrid.Children.Add(e.Element);
                        }

                        break;
                    case TopGridMode.Tip:
                        if (e.Element is TipPopup popup)
                        {
                            TipContainer.Visibility = Visibility.Visible;
                            TipContainer.Children.Add(popup);
                            popup.Visibility = Visibility.Visible;
                            await Task.Delay(TimeSpan.FromSeconds(popup.DisplaySeconds));
                            popup.Visibility = Visibility.Collapsed;
                            TipContainer.Children.Remove(popup);
                            if (TipContainer.Children.Count == 0)
                            {
                                TipContainer.Visibility = Visibility.Collapsed;
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("顶部窗体事件报错:{E}", ex);
            }
        }

        /// <summary>
        /// 启用或禁用插件时更新左侧导航栏
        /// </summary>
        private void CallerOnPluginEnabledEvent(object? sender, PluginEventArg e)
        {
            ViewModel.ReloadItems();
        }

        /// <summary>
        /// 导入完成
        /// </summary>
        private void Caller_ImportComicCompletedEvent(object sender, EventArgs e)
        {
            Caller.RefreshBook();
            LoadingControl.IsLoading = false;
        }

        /// <summary>
        /// 导入的缩略图
        /// </summary>
        private async void Caller_ImportComicThumbEvent(object sender, ImportComicThumbEventArgs e)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(e.Thumb.AsRandomAccessStream());
                ZipThumb.Source = bitmapImage;
            });
        }

        /// <summary>
        /// 导入失败
        /// </summary>
        private async void Caller_ImportComicErrorEvent(object sender, ImportComicErrorEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                if (args.Error != ImportComicError.Password) return;
                var dialog = XamlHelper.CreateOneLineTextBoxDialog(args.Message, XamlRoot);
                dialog.PrimaryButtonClick += (s, e) =>
                {
                    // 重新开始
                    var password = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                    args.Password[args.Index] = password == "" ? null : password;
                    Caller.ImportComic(args.Items, args.Password, args.Index);
                };
                dialog.CloseButtonClick += (s, e) =>
                {
                    // 跳过本个
                    if (args.Items.Count > args.Index + 1)
                    {
                        Caller.ImportComic(args.Items, args.Password, args.Index + 1);
                    }
                    else
                    {
                        Caller.ImportComicCompleted();
                    }
                };
                await dialog.ShowAsync();
            });
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
                for (var i = e.Index; i < e.Items.Count; i++)
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
                        LoadingControlText.Text = ResourcesHelper.GetString(ResourceKey.IsDecompressing);
                        LoadingFileName.Text = file.Name;
                        var options = new ReaderOptions
                        {
                            Password = e.Passwords[i],
                        };
                        var flag = CompressService.CheckPassword(file.Path, ref options);
                        if (!flag)
                        {
                            Caller.ImportComicError(ImportComicError.Password, "密码错误", e.Items, i, e.Passwords);
                            return;
                        }

                        await Task.Run(() => { flag = CompressService.CheckPassword(file.Path, ref options); });
                        var comicId = LocalComic.RandomId();

                        await Task.Run(async () =>
                        {
                            var res = await DiFactory.Services.Resolve<CompressService>()
                                .DeCompressAsync(file.Path, Config.ComicsPath, comicId, _cancelTokenSource.Token,
                                    options);
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoadingProgressBar.IsIndeterminate = true;
                                LoadingProgressText.Visibility = Visibility.Collapsed;
                                LoadingControlText.Text = ResourcesHelper.GetString(ResourceKey.IsImporting);
                            });
                            switch (res)
                            {
                                case CacheZip cache:
                                {
                                    StorageFolder folder = await cache.CachePath.ToStorageFolder();
                                    await Task.Run(() =>
                                    {
                                        try
                                        {
                                            ComicHelper.ImportComicsFromFolder(folder, "local", cache.ComicId,
                                                cache.Name);
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
                                    var fileName = Path.GetFileNameWithoutExtension(file.Path)
                                        ?.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                    var comic = LocalComic.Create(fileName, path,
                                        img: ComicHelper.LoadImgFromEntry(root, path, comicId),
                                        parent: "local", size: root.Size, id: comicId);
                                    var db = DiFactory.Services.Resolve<ISqlSugarClient>();
                                    db.Insertable(comic).ExecuteCommand();
                                    await Task.Run(() => ShadowEntry.ToLocalComic(root, path, comic.Id),
                                        _cancelTokenSource.Token);
                                    break;
                                }
                            }
                        }, _cancelTokenSource.Token);
                    }
                    else if (item is StorageFolder folder)
                    {
                        var again = false;
                        await Task.Run(() => DispatcherQueue.TryEnqueue(async () =>
                            again = await ComicHelper.CheckImportAgain(XamlRoot, path: folder.Path)));
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
                            LoadingControlText.Text = ResourcesHelper.GetString(ResourceKey.IsImporting);
                            LoadingFileName.Text = folder.Name;
                        });

                        await Task.Run(() =>
                        {
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

                Caller.ImportComicCompleted();
            }
            catch (TaskCanceledException)
            {
                Log.ForContext<NavigationPage>().Information("中断导入");
                DispatcherQueue.TryEnqueue(() => { LoadingControl.IsLoading = false; });
            }
        }

        /// <summary>
        /// 导航事件
        /// </summary>
        private void Caller_NavigationToolKit_NavigateTo(object sender, NavigateToEventArgs e)
        {
            if (Type.Equals(ContentFrame.CurrentSourcePageType,e.Page)&&!e.Force) return;
            ContentFrame.Navigate(e.Page,e.Parameter);
        }

        /// <summary>
        /// 左侧点击导航栏
        /// </summary>
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type? page = null;
            object? parameter = null;
            if (args.IsSettingsInvoked)
            {
                page = typeof(SettingsPage);
                parameter = new Uri("shadow://settings/");
            }
            else if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag is IShadowNavigationItem item)
            {
                foreach (var p in DiFactory.Services.ResolveMany<INavigationResponder>())
                {
                    p.NavigationViewItemInvokedHandler(item, ref page, ref parameter);
                    if (page != null) break;
                }
            }

            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (page is not null && !Type.Equals(preNavPageType, page))
            {
                ContentFrame.Navigate(page, parameter, args.RecommendedNavigationTransitionInfo);
            }
        }

        /// <summary>
        /// 初始化插件
        /// </summary>
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
                var groups = items.GroupBy(x =>
                    x is StorageFile file && file.IsZip() && file.Name.StartsWith("ShadowViewer.Plugin"));
                var list1 = groups.FirstOrDefault(x => x.Key)?.ToList();
                if (list1 is not null && list1.Count > 0)
                {
                    // Caller.ImportPlugin(this, list1);
                }
                var list2 = groups.FirstOrDefault(x => !x.Key)?.ToList();
                if (list2 is null || list2.Count == 0) return;
                var passwords = new string[list2.Count];
                Caller.ImportComic(list2, passwords, 0);
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
                e.DragUIOverride.Caption = ResourcesHelper.GetString(ResourceKey.Import);
                OverBorder.Visibility = Visibility.Visible;
                OverBorder.Width = Root.ActualWidth - 30;
                OverBorder.Height = Root.ActualHeight - 30;
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