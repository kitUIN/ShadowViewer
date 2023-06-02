using CommunityToolkit.WinUI;
using SharpCompress.Readers;
using SqlSugar;
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
        /// 显示右键菜单
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="isComicBook">if set to <c>true</c> [is comic book].</param>
        /// <param name="isSingle">if set to <c>true</c> [is single].</param>
        private void ShowMenu(UIElement sender, Point position = default)
        {
            bool isComicBook = ContentGridView.SelectedItems.Count > 0;
            bool isSingle = ContentGridView.SelectedItems.Count == 1;
            FlyoutShowOptions myOption = new FlyoutShowOptions()
            {
                ShowMode = FlyoutShowMode.Standard,
                
            };
            if (position != default) myOption.Position = position;
            ShadowCommandRename.IsEnabled = isComicBook & isSingle;
            ShadowCommandDelete.IsEnabled = isComicBook;
            ShadowCommandMove.IsEnabled = isComicBook;
            ShadowCommandAddTag.IsEnabled = isComicBook & isSingle;
            ShadowCommandNewFolder.IsEnabled = ViewModel.Path == "local";
            ShadowCommandStatus.IsEnabled = isComicBook & isSingle;
            HomeCommandBarFlyout.ShowAt(sender, myOption);
        } 
        /// <summary>
        /// �Ҽ��˵�
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RightTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void Root_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!Config.IsBookShelfMenuShow)
            { 
                ShowMenu(sender as UIElement, e.GetPosition(sender as UIElement));
            }
        }
        /// <summary>
        /// 右键菜单-从文件夹导入漫画
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
                await Task.Run(() => DispatcherQueue.EnqueueAsync(async () => again = !Config.IsImportAgain&& await ComicHelper.ImportAgainDialog(XamlRoot, path: folder.Path)));
                if (again)
                {
                    LoadingControl.IsLoading = false;
                    return;
                }
                LoadingProgressBar.IsIndeterminate = true;
                LoadingProgressText.Visibility = Visibility.Collapsed;
                LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                LoadingFileName.Text = folder.Name;
                await Task.Run(async () => await ComicHelper.ImportComicsFromFolder(folder, ViewModel.Path), cancelTokenSource.Token);
                ViewModel.RefreshLocalComic();
                LoadingControl.IsLoading = false;
            }
        }
        /// <summary>
        /// 右键菜单-从压缩包导入漫画
        /// </summary>
        private async void ShadowCommandAddFromZip_Click(object sender, RoutedEventArgs e)
        {
            StorageFile storageFile = await FileHelper.SelectFileAsync(this, ".zip",".rar",".7z");
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
                    await Task.Run(() => {
                        flag = CompressHelper.CheckPassword(storageFile.Path, options);
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
                        await Task.Run(() => {
                            flag = CompressHelper.CheckPassword(storageFile.Path, options);
                        }, cancelTokenSource.Token);
                    }
                    if (skip) return;
                    string comicId = LocalComic.RandomId();
                    await Task.Run(async () => {
                        object res = await CompressHelper.DeCompressAsync(storageFile.Path, Config.ComicsPath, comicId,
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
                            LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                        });
                        if (res is CacheZip cache)
                        {
                            StorageFolder folder = await cache.CachePath.ToStorageFolder();
                            await Task.Run(async () => await ComicHelper.ImportComicsFromFolder(folder, ViewModel.Path, cache.ComicId,
                                cache.Name), cancelTokenSource.Token);
                        }
                        else if (res is ShadowEntry root)
                        {
                            string path = Path.Combine(Config.ComicsPath, comicId);
                            string fileName = Path.GetFileNameWithoutExtension(storageFile.Path).Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                            LocalComic comic = LocalComic.Create(fileName, path, img: ComicHelper.LoadImgFromEntry(root, path),
                                parent: "local", size: root.Size, id: comicId);
                            comic.Add();
                            await Task.Run(() =>  ShadowEntry.ToLocalComic(root, path, comic.Id), cancelTokenSource.Token);
                        }
                    });
                }
                ViewModel.RefreshLocalComic();
                LoadingControl.IsLoading = false;
            }
        }
        
        /// <summary>
        /// �Ҽ��˵�-�½��ļ���
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            await CreateFolderDialog(XamlRoot, ViewModel.Path).ShowAsync();
        }
        /// <summary>
        /// �Ҽ��˵�-������
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
        /// �Ҽ��˵�-ɾ��
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            Delete();
        }


        /// <summary>
        /// �Ҽ��˵�-�ƶ���
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandMove_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            MoveTreeView.ItemsSource = new List<ShadowPath> { 
                new ShadowPath(ContentGridView.SelectedItems.Cast<LocalComic>().Select(c => c.Id)) 
            };
            MoveTeachingTip.IsOpen = true;
        }
        /// <summary>
        /// �Ҽ��˵�-���ӱ�ǩ
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandAddTag_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            ConnectedAnimation animation = null;
            ViewModel.ConnectComic = ContentGridView.SelectedItems[0] as LocalComic;
            animation = ContentGridView.PrepareConnectedAnimation("forwardComicStatusAnimation", ViewModel.ConnectComic, "connectedElement");
            SmokeFrame.Navigate(typeof(TagsPage), ViewModel.ConnectComic);
            SmokeGrid.Visibility = Visibility.Visible;
            animation.TryStart(destinationElement);
        }
        /// <summary>
        /// �Ҽ��˵�-�鿴����
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandStatus_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            ConnectedAnimation animation = null;
            ViewModel.ConnectComic = ContentGridView.SelectedItems[0] as LocalComic;
            animation = ContentGridView.PrepareConnectedAnimation("forwardComicStatusAnimation", ViewModel.ConnectComic, "connectedElement");
            SmokeFrame.Navigate(typeof(StatusPage), ViewModel.ConnectComic);
            SmokeGrid.Visibility = Visibility.Visible;
            animation.TryStart(destinationElement);
        }

        /// <summary>
        /// �Ҽ��˵�-ˢ��
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandRefresh_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshLocalComic();
        }
        /// <summary>
        /// �Ҽ�ѡ��GridViewItem
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RightTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void ContentGridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext != null)
            {
                var container = (GridViewItem)ContentGridView.ContainerFromItem(element.DataContext);
                if (container != null && !container.IsSelected)
                {
                    ContentGridView.SelectedItems.Clear();
                    container.IsSelected = true;
                }
            }
        }
        /// <summary>
        /// ˫���ļ��л�������
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoubleTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void ContentGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is LocalComic comic)
            {
                if(comic.IsFolder)
                {
                    Frame.Navigate(GetType(), new Uri(ViewModel.OriginPath, comic.Id));
                }
                else
                {
                    Frame.Navigate(typeof(PicPage), comic, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }
        /// <summary>
        /// �������Ի���
        /// </summary>
        /// <returns></returns>
        private ContentDialog CreateRenameDialog(string title, XamlRoot xamlRoot, LocalComic comic)
        {
            ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog(title, xamlRoot, comic.Name); 
            dialog.PrimaryButtonClick +=  (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)dialog.Content).Children[0]).Children[1]).Text;
                comic.Name = name;
                ViewModel.RefreshLocalComic();
            };
            return dialog;
        }
        /// <summary>
        /// �½��ļ��жԻ���
        /// </summary>
        /// <returns></returns>
        public ContentDialog CreateFolderDialog(XamlRoot xamlRoot, string parent)
        {
            ContentDialog dialog = XamlHelper.CreateOneLineTextBoxDialog(I18nHelper.GetString("Shadow.String.CreateFolder.Title"), xamlRoot, "");
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)s.Content).Children[0]).Children[1]).Text;
                ViewModel.LocalComics.Add(ComicHelper.CreateFolder(name,   parent));
                ViewModel.RefreshLocalComic();
            };
            return dialog;
        }
        /// <summary>
        /// �����νṹ��˫��
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoubleTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void TreeViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            MoveToPath(MoveTreeView.SelectedItem as ShadowPath);
        }
        /// <summary>
        /// �ƶ��� �Ի���İ�ť��Ӧ
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void MoveTeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            MoveToPath(MoveTreeView.SelectedItem as ShadowPath);
        }
        /// <summary>
        /// �ƶ�������ļ���
        /// </summary>
        /// <param name="path">The path.</param>
        private void MoveToPath(ShadowPath path)
        {
            if (path == null) { return; }
            foreach (LocalComic comic in ContentGridView.SelectedItems)
            {
                if(comic.Id != path.Id && path.IsFolder)
                {
                    comic.Parent = path.Id;
                }
            }
            MoveTeachingTip.IsOpen = false;
            ViewModel.RefreshLocalComic();
        }
        
        /// <summary>
        /// �����϶�
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void GridViewItem_Drop(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement frame  )
            { 
                if(frame.Tag is LocalComic comic && comic.IsFolder)
                {
                    foreach (LocalComic item in ContentGridView.SelectedItems)
                    {
                        if (!item.IsFolder)
                        {
                            item.Parent = comic.Id;
                        } 
                    }
                    ViewModel.RefreshLocalComic();
                }                 
            }
        }
        /// <summary>
        /// �϶�������ʾ
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
                else { return; } 
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
        }

        /// <summary>
        /// �϶���ʼ���� ���ӵ�GridViewѡ��
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragItemsStartingEventArgs"/> instance containing the event data.</param>
        private void ContentGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
             
            foreach(var item in e.Items)
            {
                if(!ContentGridView.SelectedItems.Contains(item))
                {
                    ContentGridView.SelectedItems.Add(item);
                }
            }
        }
        
        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsComicStatusAnimation", destinationElement);
            SmokeGrid.Children.Remove(destinationElement); 
            animation.Completed += Animation_Completed; 
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            } 
            await ContentGridView.TryStartConnectedAnimationAsync(animation, ViewModel.ConnectComic, "connectedElement");
        }
        private void Animation_Completed(ConnectedAnimation sender, object args)
        {
            SmokeGrid.Visibility = Visibility.Collapsed;
            SmokeGrid.Children.Add(destinationElement);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmokeGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid.ActualHeight > 50)
            {
                destinationElement.Height = grid.ActualHeight - 50;
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
        /// ��������Ӧ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            SortText.Text = ((MenuFlyoutItem)sender).Text;
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
            SortText.Text = I18nHelper.GetString("Xaml/MenuFlyoutItem/RZ/Text");
            MenuButton.Visibility = Config.IsBookShelfMenuShow.ToVisibility();
            MenuText.Visibility = FilterText.Visibility = SettingsText.Visibility = HistoryText.Visibility = Config.IsBookShelfDetailShow.ToVisibility();
            SelectionPanel.Visibility = Visibility.Collapsed;
            ShelfInfo.Visibility = Config.IsBookShelfInfoBar.ToVisibility();
        }
        
        /// <summary>
        /// ɾ����������ȷ��
        /// </summary>
        public async void DeleteMessageDialog()
        {
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
            dialog.PrimaryButtonClick += (s, e) =>
            {
                DeleteComics();
            };
            dialog.Focus(FocusState.Programmatic);
            await dialog.ShowAsync();
        }
        /// <summary>
        /// ɾ����Ӧ(�Ҽ�ɾ��,Detele��ɾ��)
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
        /// ִ��ɾ����������
        /// </summary>
        private void DeleteComics()
        {
            foreach (LocalComic comic in ContentGridView.SelectedItems.ToList())
            {
                if (Config.IsDeleteFilesWithComicDelete && !comic.IsFolder)
                {
                    comic.Link.DeleteDirectory();
                }
                ViewModel.LocalComics.Remove(comic);
            }
        }
        /// <summary>
        /// ��ѡ��-��סѡ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Remember_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            Config.IsRememberDeleteFilesWithComicDelete = (bool)box.IsChecked;
        }
        /// <summary>
        /// ��ѡ��-һ��ɾ�������ļ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteFiles_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            Config.IsDeleteFilesWithComicDelete = (bool)box.IsChecked;
        }
        /// <summary>
        /// ������Ӧ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Key == VirtualKey.A &&
                WindowHelper.GetWindowForXamlRoot(XamlRoot)
                .CoreWindow.GetKeyState(VirtualKey.Shift)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                foreach(LocalComic comic in view.ItemsSource as ObservableCollection<LocalComic>)
                {
                    view.SelectedItems.Add(comic);
                }
            }
            else if(e.Key == VirtualKey.Delete)
            {
                Delete();
            }
        }
        /// <summary>
        /// �˵���ť
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenu(sender as UIElement);
        }
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
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
            if(ContentGridView.SelectedItems.Count > 0 )
            {
                SelectionPanel.Visibility = Visibility.Visible;
                long size = 0;
                foreach( LocalComic item in ContentGridView.SelectedItems)
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
    }

}
