using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Core.Enums;
using SharpCompress.Readers;
using SqlSugar;
using System.Linq;
using System.Threading;
using Windows.Storage;

namespace ShadowViewer.Pages
{
    public sealed partial class BookShelfPage : Page
    {
        private static CancellationTokenSource cancelTokenSource;
        private BookShelfViewModel ViewModel { get; set; }
        public BookShelfPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = new BookShelfViewModel(e.Parameter as Uri);
        }
        /// <summary>
        /// 显示悬浮菜单
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="isComicBook">if set to <c>true</c> [is comic book].</param>
        /// <param name="isSingle">if set to <c>true</c> [is single].</param>
        private void ShowMenu(UIElement sender, Point? position = null)
        {
            bool isComicBook = ContentGridView.SelectedItems.Count > 0;
            bool isSingle = ContentGridView.SelectedItems.Count == 1;
            bool isFolder = isSingle && ((LocalComic)ContentGridView.SelectedItem).IsFolder;
            FlyoutShowOptions myOption = new FlyoutShowOptions()
            {
                ShowMode = FlyoutShowMode.Standard,
                Position = position,
            };
            ShadowCommandRename.IsEnabled = isComicBook & isSingle;
            ShadowCommandDelete.IsEnabled = isComicBook;
            ShadowCommandMove.IsEnabled = isComicBook;
            ShadowCommandAdd.IsEnabled = isFolder;
            ShadowCommandStatus.IsEnabled = isComicBook & isSingle;
            HomeCommandBarFlyout.ShowAt(sender, myOption);
        }
        /// <summary>
        /// 悬浮菜单-从文件夹导入漫画
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddFromFolder_Click(object sender, RoutedEventArgs e)
        {

            StorageFolder folder = await FileHelper.SelectFolderAsync(this, "AddNewComic");
            if (folder != null)
            {
                cancelTokenSource = new CancellationTokenSource();
                LoadingControl.IsLoading = true;
                bool again = false;
                await Task.Run(() => DispatcherQueue.EnqueueAsync(async () => again = !Config.IsImportAgain && await ComicHelper.ImportAgainDialog(XamlRoot, path: folder.Path)));
                if (again)
                {
                    LoadingControl.IsLoading = false;
                    return;
                }
                LoadingProgressBar.IsIndeterminate = true;
                LoadingProgressText.Visibility = Visibility.Collapsed;
                LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                LoadingFileName.Text = folder.Name;
                await Task.Run(async () =>
                {
                    try
                    {
                        await ComicHelper.ImportComicsFromFolder(folder, ViewModel.Path);
                    }
                    catch (Exception)
                    {
                        Log.Warning("导入无效文件夹:{F},忽略", folder.Path);
                    }
                }, cancelTokenSource.Token);
                ViewModel.RefreshLocalComic();
                LoadingControl.IsLoading = false;
            }
        }
        /// <summary>
        /// 悬浮菜单-从压缩包导入漫画
        /// </summary>
        private async void ShadowCommandAddFromZip_Click(object sender, RoutedEventArgs e)
        {
            StorageFile storageFile = await FileHelper.SelectFileAsync(this, ".zip", ".rar", ".7z");
            if (storageFile != null)
            {
                if (storageFile.IsZip())
                {
                    cancelTokenSource = new CancellationTokenSource();
                    LoadingControl.IsLoading = true;
                    bool again = false;
                    await Task.Run(() => DispatcherQueue.EnqueueAsync(async () => again = await ComicHelper.ImportAgainDialog(XamlRoot, zip: storageFile.Path)), cancelTokenSource.Token);
                    if (again)
                    {
                        LoadingControl.IsLoading = false;
                        return;
                    }
                    ZipThumb.Source = null;
                    LoadingProgressBar.IsIndeterminate = true;
                    LoadingProgressBar.Value = 0;
                    LoadingProgressText.Visibility = Visibility.Visible;
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportDecompress");
                    LoadingFileName.Text = storageFile.Name;
                    ReaderOptions options = null;
                    bool skip = false;
                    bool flag = false;
                    await Task.Run(() =>
                    {
                        flag = CompressHelper.CheckPassword(storageFile.Path, ref options);
                    }, cancelTokenSource.Token);
                    while (!flag)
                    {
                        ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog(I18nHelper.GetString("Shadow.String.ZipPasswordTitle"), XamlRoot, "", I18nHelper.GetString("Shadow.String.ZipPasswordTitle"), I18nHelper.GetString("Shadow.String.ZipPasswordTitle"));
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
                        await Task.Run(() =>
                        {
                            flag = CompressHelper.CheckPassword(storageFile.Path, ref options);
                        }, cancelTokenSource.Token);
                    }
                    if (skip) return;
                    string comicId = LocalComic.RandomId();
                    await Task.Run(async () =>
                    {
                        object res = await CompressHelper.DeCompressAsync(storageFile.Path, Config.ComicsPath, comicId,
                        imgAction: new Progress<MemoryStream>(
                            (MemoryStream ms) => DispatcherQueue.EnqueueAsync(async () =>
                            {
                                BitmapImage bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(ms.AsRandomAccessStream());
                                ZipThumb.Source = bitmapImage;
                            })),
                        progress: new Progress<double>((value) => DispatcherQueue.TryEnqueue(() => LoadingProgressBar.Value = value)),
                        () =>
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoadingProgressBar.IsIndeterminate = false;
                            });
                        }, cancelTokenSource.Token, options);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            LoadingProgressBar.IsIndeterminate = true;
                            LoadingProgressText.Visibility = Visibility.Collapsed;
                            LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                        });
                        if (res is CacheZip cache)
                        {
                            StorageFolder folder = await cache.CachePath.ToStorageFolder();
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await ComicHelper.ImportComicsFromFolder(folder, ViewModel.Path, cache.ComicId, cache.Name);
                                }
                                catch (Exception)
                                {
                                    Log.Warning("导入无效文件夹:{F},忽略", folder.Path);
                                }
                            }, cancelTokenSource.Token);
                        }
                        else if (res is ShadowEntry root)
                        {
                            string path = Path.Combine(Config.ComicsPath, comicId);
                            string fileName = Path.GetFileNameWithoutExtension(storageFile.Path).Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                            LocalComic comic = LocalComic.Create(fileName, path, img: ComicHelper.LoadImgFromEntry(root, path),
                                parent: "local", size: root.Size, id: comicId);
                            comic.Add();
                            await Task.Run(() => ShadowEntry.ToLocalComic(root, path, comic.Id), cancelTokenSource.Token);
                        }
                    });
                }
                else
                {
                    Log.Warning("导入无效文件:{F},忽略", storageFile.Path);
                }
                ViewModel.RefreshLocalComic();
                LoadingControl.IsLoading = false;
            }
        }

        /// <summary>
        /// 右键菜单-创建文件夹
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            await CreateFolderDialog(XamlRoot, ViewModel.Path).ShowAsync();
        }
        /// <summary>
        /// 右键菜单-重命名
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandRename_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            await CreateRenameDialog(I18nHelper.GetString("Xaml.ToolTip.Rename.Content"), XamlRoot, comic).ShowAsync();
        }
        /// <summary>
        /// 右键菜单-删除
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            Delete();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShadowCommandMove_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            MoveTreeView.ItemsSource = new List<ShadowPath> {
                new ShadowPath(ContentGridView.SelectedItems.Cast<LocalComic>().ToList().Select(c => c.Id))
            };
            MoveTeachingTip.IsOpen = true;
        }
        /// <summary>
        /// �Ҽ��˵�-�鿴����
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandStatus_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            ViewModel.ConnectComic = ContentGridView.SelectedItems[0] as LocalComic;
            Frame.Navigate(typeof(AttributesPage), ViewModel.ConnectComic);
        }

        /// <summary>
        /// 右键菜单-刷新
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandRefresh_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshLocalComic();
        }
        /// <summary>
        /// 触控/鼠标-漫画项右键<br />
        /// 选中&显示悬浮菜单
        /// </summary>
        private void ContentGridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
            {
                GridViewItem container = (GridViewItem)ContentGridView.ContainerFromItem(element.DataContext);
                if (container != null && !container.IsSelected)
                {
                    container.IsSelected = true;
                }
                ShowMenu(sender as UIElement, e.GetPosition(sender as UIElement));
            }
        }
        /// <summary>
        /// 弹出框-重命名
        /// </summary>
        private ContentDialog CreateRenameDialog(string title, XamlRoot xamlRoot, LocalComic comic)
        {
            ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog(title, xamlRoot, comic.Name);
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)dialog.Content).Children[0]).Children[1]).Text;
                comic.Name = name;
                ViewModel.RefreshLocalComic();
            };
            return dialog;
        }
        /// <summary>
        /// 弹出框-新建文件夹
        /// </summary>
        /// <returns></returns>
        public ContentDialog CreateFolderDialog(XamlRoot xamlRoot, string parent)
        {
            ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog(I18nHelper.GetString("Shadow.String.CreateFolder.Title"), xamlRoot, "");
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                ViewModel.LocalComics.Add(ComicHelper.CreateFolder(name, parent));
                ViewModel.RefreshLocalComic();
            };
            return dialog;
        }
        /// <summary>
        /// 路径树-双击
        /// </summary>
        private void TreeViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            MoveToPath(MoveTreeView.SelectedItem as ShadowPath);
        }
        /// <summary>
        /// 路径树-确定移动
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void MoveTeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            MoveToPath(MoveTreeView.SelectedItem as ShadowPath);
        }
        /// <summary>
        /// 移动到路径树
        /// </summary>
        /// <param name="path">The path.</param>
        private void MoveToPath(ShadowPath path)
        {
            if (path == null) { return; }
            foreach (LocalComic comic in ContentGridView.SelectedItems.Cast<LocalComic>().ToList())
            {
                if (comic.Id != path.Id && path.IsFolder)
                {
                    comic.Parent = path.Id;
                }
            }
            LocalComic.Query().Where(x => x.Parent == path.Id).ToList().ForEach(x => path.SetSize(x.Size));
            MoveTeachingTip.IsOpen = false;
            ViewModel.RefreshLocalComic();
        }

        /// <summary>
        /// 拖动响应
        /// </summary>
        private void GridViewItem_Drop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement frame && frame.Tag is LocalComic comic && comic.IsFolder )
            {
                foreach (LocalComic item in ContentGridView.SelectedItems.Cast<LocalComic>().ToList())
                {
                    if (!item.IsFolder)
                    {
                        item.Parent = comic.Id;
                    }
                }
                comic.Size = 0;
                LocalComic.Query().Where(x => x.Parent == comic.Id).ToList().ForEach(x => comic.Size+=x.Size);
                ViewModel.RefreshLocalComic();
            }
        }
        /// <summary>
        /// 拖动悬浮显示
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void GridViewItem_DragOverCustomized(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement frame)
            {
                if (frame.Tag is LocalComic comic && comic.IsFolder)
                {
                    e.DragUIOverride.Caption = I18nHelper.GetString("Xaml.Command.Move.Label") + comic.Name;
                    e.AcceptedOperation = comic.IsFolder ? DataPackageOperation.Move : DataPackageOperation.None;
                }
                else return;
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
        }

        /// <summary>
        /// 拖动初始化
        /// </summary>
        private void ContentGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            foreach (LocalComic item in e.Items)
            {
                GridViewItem container = (GridViewItem)ContentGridView.ContainerFromItem(item);
                if (container != null && !container.IsSelected)
                {
                    container.IsSelected = true;
                }
            }
        }
        /// <summary>
        /// ��ֹ�����Ҽ��¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmokeGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        /// <summary>
        /// 修改排序
        /// </summary>
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string text = ((MenuFlyoutItem)sender).Tag.ToString();
            foreach (MenuFlyoutItem item in SortFlyout.Items.Cast<MenuFlyoutItem>())
            {
                item.Text = (item.Tag.ToString() == text ? "⁜ " : "    ") + I18nHelper.GetString($"Xaml.MenuFlyoutItem.{item.Tag.ToString()}.Text");
            }
            ViewModel.Sorts = EnumHelper.GetEnum<ShadowSorts>(((MenuFlyoutItem)sender).Tag.ToString());
            ViewModel.RefreshLocalComic();
        }
        /// <summary>
        /// 控件初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Controls_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (MenuFlyoutItem item in SortFlyout.Items)
            {
                item.Text = (item.Tag.ToString() == "RZ" ? "⁜ " : "    ") + I18nHelper.GetString($"Xaml.MenuFlyoutItem.{item.Tag.ToString()}.Text");
            }
            SelectionPanel.Visibility = Visibility.Collapsed;
            ShelfInfo.Visibility = Config.IsBookShelfInfoBar.ToVisibility();
            SortDetail.Visibility = AddDetail.Visibility = FilterDetail.Visibility = RefreshDetail.Visibility = Config.IsTopBarDetail.ToVisibility();
            StyleSegmented.SelectedIndex = Config.BookStyleDetail ? 1 : 0;
            ShadowCommandAddNewFolder.IsEnabled = ViewModel.Path == "local";
        }
        /// <summary>
        /// 删除二次确定框
        /// </summary>
        public async void DeleteMessageDialog()
        {
            void Remember_Checked(object sender, RoutedEventArgs e)
            {
                Config.IsRememberDeleteFilesWithComicDelete = (sender as CheckBox)?.IsChecked ?? false;
            }
            void DeleteFiles_Checked(object sender, RoutedEventArgs e)
            {
                Config.IsDeleteFilesWithComicDelete = (sender as CheckBox)?.IsChecked ?? false;
            }
            ContentDialog dialog = XamlHelper.CreateContentDialog(XamlRoot);
            StackPanel stackPanel = new StackPanel();
            dialog.Title = I18nHelper.GetString("Shadow.String.IsDelete");
            CheckBox deleteFiles = new CheckBox()
            {
                Content = I18nHelper.GetString("Shadow.String.DeleteFiles"),
                IsChecked = Config.IsDeleteFilesWithComicDelete,
            };
            deleteFiles.Checked += DeleteFiles_Checked;
            deleteFiles.Unchecked += DeleteFiles_Checked;
            CheckBox remember = new CheckBox()
            {
                Content = I18nHelper.GetString("Shadow.String.Remember"),
                IsChecked = Config.IsRememberDeleteFilesWithComicDelete,
            };
            remember.Checked += Remember_Checked;
            remember.Unchecked += Remember_Checked;
            stackPanel.Children.Add(deleteFiles);
            stackPanel.Children.Add(remember);
            dialog.IsPrimaryButtonEnabled = true;
            dialog.PrimaryButtonText = I18nHelper.GetString("Shadow.String.Confirm");
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.CloseButtonText = I18nHelper.GetString("Shadow.String.Canel");
            dialog.Content = stackPanel;
            dialog.PrimaryButtonClick += (ContentDialog s, ContentDialogButtonClickEventArgs e) =>
            {
                DeleteComics();
            };
            dialog.Focus(FocusState.Programmatic);
            await dialog.ShowAsync();
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void Delete()
        {
            if (ContentGridView.SelectedItems.ToList().Cast<LocalComic>().All(x => x.IsFolder))
            {
                DeleteComics();
            }
            else
            {
                if (Config.IsRememberDeleteFilesWithComicDelete)
                {
                    DeleteComics();
                }
                else
                {
                    DeleteMessageDialog();
                }
            }

        }
        /// <summary>
        /// 删除选中的漫画
        /// </summary>
        private void DeleteComics()
        {
            foreach (LocalComic comic in ContentGridView.SelectedItems.ToList().Cast<LocalComic>())
            {
                if (Config.IsDeleteFilesWithComicDelete && !comic.IsFolder && DBHelper.Db.Queryable<CacheZip>().Any(x => x.ComicId == comic.Id))
                {
                    comic.Link.DeleteDirectory();
                }
                ViewModel.LocalComics.Remove(comic);
            }
        }

        /// <summary>
        /// 检测按键
        /// </summary>
        private void GridViewOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Key == VirtualKey.A &&
                WindowHelper.GetWindowForXamlRoot(XamlRoot)
                .CoreWindow.GetKeyState(VirtualKey.Shift)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                foreach (LocalComic comic in view.ItemsSource as ObservableCollection<LocalComic>)
                {
                    view.SelectedItems.Add(comic);
                }
            }
            else if (e.Key == VirtualKey.Delete)
            {
                Delete();
            }
        }
        /// <summary>
        /// 右键菜单-设置按钮
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            this.Frame.Navigate(typeof(BookShelfSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 选中响应更改信息栏
        /// </summary>
        private void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentGridView.SelectedItems.Count > 0)
            {
                SelectionPanel.Visibility = Visibility.Visible;
                long size = 0;
                foreach (LocalComic item in ContentGridView.SelectedItems)
                {
                    size += item.Size;
                }
                SelectionValue.Text = ContentGridView.SelectedItems.Count.ToString();
                SizeValue.Text = ComicHelper.ShowSize(size);
            }
            else
            {
                SelectionPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 取消导入
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
        }
        /// <summary>
        /// 简约与详细视图切换<br />
        /// SelectedIndex:<br />
        /// 0 - 简略<br />
        /// 1 - 详细
        /// </summary>
        private void Segmented_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Segmented se = sender as Segmented;
            if (ContentGridView is null) return;
            Config.BookStyleDetail = se.SelectedIndex == 1;
            ContentGridView.ItemTemplate =
                this.Resources[(Config.BookStyleDetail ? "Detail" : "Simple") + "LocalComicItem"] as DataTemplate;
        }
        /// <summary>
        /// 触控-下拉刷新
        /// </summary>
        private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using Deferral RefreshCompletionDeferral = args.GetDeferral();
            this.ViewModel.RefreshLocalComic();
        }
        /// <summary>
        /// 触控/鼠标-点击漫画项
        /// </summary>
        private void ContentGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is LocalComic comic)
            {
                comic.LastReadTime = DateTime.Now;
                if (comic.IsFolder)
                {
                    Frame.Navigate(GetType(), new Uri(ViewModel.OriginPath, comic.Id));
                }
                else
                {
                    Frame.Navigate(typeof(PicPage), comic, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

    }

}
