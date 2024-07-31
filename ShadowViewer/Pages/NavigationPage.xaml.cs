namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : Page
    {
        public static ILogger Logger { get; } = Log.ForContext<NavigationPage>();
        private static CancellationTokenSource _cancelTokenSource = new();
        private NavigationViewModel ViewModel { get; } = DiFactory.Services.Resolve<NavigationViewModel>();
        private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
        private INotifyService NotifyService { get; } = DiFactory.Services.Resolve<INotifyService>();
        private PluginLoader PluginService { get; } = DiFactory.Services.Resolve<PluginLoader>();

        public NavigationPage()
        {
            this.InitializeComponent();
            Caller.ImportComicEvent += Caller_ImportComicEvent;
            Caller.ImportComicProgressEvent += Caller_ImportComicProgressEvent;
            Caller.ImportComicErrorEvent += Caller_ImportComicErrorEvent;
            Caller.ImportComicThumbEvent += Caller_ImportComicThumbEvent;
            Caller.ImportComicCompletedEvent += Caller_ImportComicCompletedEvent;
            Caller.NavigateToEvent += Caller_NavigationToolKit_NavigateTo;
            PluginEventService.PluginLoaded += CallerOnPluginEnabledEvent;
            PluginEventService.PluginDisabled += CallerOnPluginEnabledEvent;
            NotifyService.TipPopupEvent += NotifyService_TipPopupEvent;
            Caller.TopGridEvent += Caller_TopGridEvent;
            ViewModel.ReloadItems();
        }

        private void NotifyService_TipPopupEvent(object? sender, TipPopupEventArgs e)
        {
            TipContainer.Show(e.Text, e.Level, e.DisplaySeconds);
        }

        

        /// <summary>
        /// ���������¼�
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
                }
            }
            catch (Exception ex)
            {
                Logger.Error("���������¼�����:{E}", ex);
            }
        }

        /// <summary>
        /// ���û���ò��ʱ������ർ����
        /// </summary>
        private void CallerOnPluginEnabledEvent(object? sender, PluginEventArgs e)
        {
            ViewModel.ReloadItems();
        }

        /// <summary>
        /// �������
        /// </summary>
        private void Caller_ImportComicCompletedEvent(object sender, EventArgs e)
        {
            Caller.RefreshBook();
            //LoadingControl.IsLoading = false;
        }

        /// <summary>
        /// ���������ͼ
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
        /// ����ʧ��
        /// </summary>
        private async void Caller_ImportComicErrorEvent(object sender, ImportComicErrorEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                if (args.Error != ImportComicError.Password) return;
                var dialog = XamlHelper.CreateOneLineTextBoxDialog(args.Message, XamlRoot);
                dialog.PrimaryButtonClick += (s, e) =>
                {
                    // ���¿�ʼ
                    var password = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                    args.Password[args.Index] = password == "" ? null : password;
                    Caller.ImportComic(args.Items, args.Password, args.Index);
                };
                dialog.CloseButtonClick += (s, e) =>
                {
                    // ��������
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
        /// �������
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
        /// ��ʼ����
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
                        //LoadingControl.IsLoading = true;
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
                            Caller.ImportComicError(ImportComicError.Password, "�������", e.Items, i, e.Passwords);
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
                                            Log.Error("��Ч�ļ���{F},����", folder.Path);
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
                            //DispatcherQueue.TryEnqueue(() => LoadingControl.IsLoading = false);
                            continue;
                        }

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            //LoadingControl.IsLoading = true;
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
                                Log.Warning("������Ч�ļ���:{F},����\n{ex}", folder.Path, ex);
                            }
                        }, _cancelTokenSource.Token);
                    }
                    else
                    {
                        Log.Warning("������Ч�ļ�:{F},����", item.Path);
                    }
                }

                Caller.ImportComicCompleted();
            }
            catch (TaskCanceledException)
            {
                Log.ForContext<NavigationPage>().Information("�жϵ���");
                //DispatcherQueue.TryEnqueue(() => { LoadingControl.IsLoading = false; });
            }
        }

        /// <summary>
        /// �����¼�
        /// </summary>
        private void Caller_NavigationToolKit_NavigateTo(object sender, NavigateToEventArgs e)
        {
            if (Type.Equals(ContentFrame.CurrentSourcePageType,e.Page)&&!e.Force) return;
            ContentFrame.Navigate(e.Page,e.Parameter);
        }

        /// <summary>
        /// �����������
        /// </summary>
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type? page = null;
            object? parameter = null;
            NavigationTransitionInfo? info = null;
            if (args.IsSettingsInvoked)
            {
                page = typeof(SettingsPage);
                parameter = new Uri("shadow://settings/");
            }
            else if (args.InvokedItemContainer != null && 
                     args.InvokedItemContainer.Tag is IShadowNavigationItem item &&
                     ViewModel.NavigationViewItemInvokedHandler(item) is { } navigation
                     )
            {
                parameter = navigation.Parameter;
                page = navigation.PageType;
                info = navigation.TransitionInfo;
            }
            info ??= args.RecommendedNavigationTransitionInfo;
            var preNavPageType = ContentFrame.CurrentSourcePageType;
            if (page is not null && !Type.Equals(preNavPageType, page))
            {
                ContentFrame.Navigate(page, parameter, info);
            }
        }

        /// <summary>
        /// ��ʼ�����
        /// </summary>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// �ⲿ�ļ����������Ӧ
        /// </summary>
        private async void Root_Drop(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
            if (e.DataView.Contains(StandardDataFormats.StorageItems) 
                //&& !LoadingControl.IsLoading
                )
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
        /// �ⲿ�ļ��϶�������ʾ
        /// </summary>
        private void Root_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems) 
                //&& !LoadingControl.IsLoading
                )
            {
                e.AcceptedOperation = DataPackageOperation.Link;
                e.DragUIOverride.Caption = ResourcesHelper.GetString(ResourceKey.Import);
                OverBorder.Visibility = Visibility.Visible;
                //OverBorder.Width = Root.ActualWidth - 30;
                //OverBorder.Height = Root.ActualHeight - 30;
            }
        }

        /// <summary>
        /// �ⲿ�ļ��϶��뿪
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
        /// ȡ������
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancelTokenSource.Cancel();
        }

        /// <summary>
        /// ���������˰�ť ���
        /// </summary>
        public void AppTitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ContentFrame.CanGoBack) return;
            ContentFrame.GoBack();
        }
        /// <summary>
        /// ��������尴ť ���
        /// </summary>
        public void AppTitleBar_OnPaneButtonClick(object sender, RoutedEventArgs e)
        {
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }
    }
}