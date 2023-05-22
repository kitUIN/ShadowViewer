using System.Linq;
using Windows.UI.Core;

namespace ShadowViewer.Pages
{
    public sealed partial class BookShelfPage : Page
    {
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
        /// 右键菜单
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
        /// 右键菜单-新建漫画从文件夹导入
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddFromFolder_Click(object sender, RoutedEventArgs e)
        {

            StorageFolder folder = await FileHelper.SelectFolderAsync(this, "AddNewComic");
            if (folder != null)
            { 
                LoadingControl.IsLoading = true;
                LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                await ViewModel.ImportComicsAsync(folder);
                LoadingControl.IsLoading = false;
                 
            }
        }
        /// <summary>
        /// 右键菜单-新建漫画从压缩文件导入
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddFromZip_Click(object sender, RoutedEventArgs e)
        {
            StorageFile storageFile = await FileHelper.SelectFileAsync(this, ".zip",".rar",".7z");
            if (storageFile != null)
            {
                try
                {
                    List<Task> backgrounds = new List<Task>();
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                    if (storageFile.IsZip())
                    {
                        LoadingControl.IsLoading = true;
                        LocalComic comic = await ComicHelper.ImportComicsFromZip(storageFile.Path, Config.TempPath);
                        backgrounds.Add(Task.Run(() => ComicHelper.EntryToComic(Config.ComicsPath, comic, storageFile.Path)));
                        ViewModel.LocalComics.Add(comic);
                    } 
                    LoadingControl.IsLoading = false;
                    await Task.WhenAll(backgrounds);
                    ViewModel.RefreshLocalComic();
                }
                catch(Exception ex)
                {
                    Log.Error("右键菜单-新建漫画从压缩文件导入报错:{Ex}", ex);
                }
            }
        }
        
        /// <summary>
        /// 右键菜单-新建文件夹
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
        /// 右键菜单-移动到
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
        /// 右键菜单-添加标签
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
        /// 右键菜单-查看属性
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
        /// 右键菜单-刷新
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandRefresh_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshLocalComic();
        }
        /// <summary>
        /// 右键选中GridViewItem
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
        /// 双击文件夹或者漫画
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoubleTappedRoutedEventArgs"/> instance containing the event data.</param>
        private async void ContentGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is LocalComic comic)
            {
                if(comic.IsFolder)
                {
                    Frame.Navigate(this.GetType(), new Uri(ViewModel.OriginPath, comic.Id));
                }
                else
                {
                    if (comic.IsTemp)
                    {
                        List<Task> backgrounds = new List<Task>();
                        if (!ComicHelper.Entrys.ContainsKey(comic.Link))
                        {
                            LocalComic temp = await ComicHelper.ImportComicsFromZip(comic.Link, Config.TempPath);
                            backgrounds.Add(Task.Run(() => ComicHelper.EntryToComic(Config.ComicsPath, comic, comic.Link)));
                        } 
                        Frame.Navigate(typeof(PicPage), ComicHelper.Entrys[comic.Link], new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                        await Task.WhenAll(backgrounds);
                    }
                    else
                    {
                        Frame.Navigate(typeof(PicPage), comic, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                }
            }
        }
        /// <summary>
        /// 重命名对话框
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
        /// 新建文件夹对话框
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
        /// 在树形结构上双击
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoubleTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void TreeViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            MoveToPath(MoveTreeView.SelectedItem as ShadowPath);
        }
        /// <summary>
        /// 移动到 对话框的按钮响应
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void MoveTeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            MoveToPath(MoveTreeView.SelectedItem as ShadowPath);
        }
        /// <summary>
        /// 移动到别的文件夹
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
        /// 接收拖动
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
                else { return; } 
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
        }

        /// <summary>
        /// 拖动开始步骤 添加到GridView选中
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
        /// 禁止传递右键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SmokeGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        /// <summary>
        /// 排序点击响应
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
        /// 排序框,菜单框,工具栏详细信息初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortButton_Loaded(object sender, RoutedEventArgs e)
        {
            SortText.Text = I18nHelper.GetString("Xaml/MenuFlyoutItem/RZ/Text");
            MenuButton.Visibility = Config.IsBookShelfMenuShow ? Visibility.Visible : Visibility.Collapsed;
            Visibility detail = Config.IsBookShelfDetailShow ? Visibility.Visible : Visibility.Collapsed;
            MenuText.Visibility = FilterText.Visibility = SettingsText.Visibility =  detail;
        }
        
        /// <summary>
        /// 删除漫画二次确定
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
        /// 删除响应(右键删除,Detele键删除)
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
        /// 执行删除漫画操作
        /// </summary>
        private void DeleteComics()
        {
            foreach (LocalComic comic in ContentGridView.SelectedItems.ToList())
            {
                if (Config.IsDeleteFilesWithComicDelete && !comic.IsTemp && !comic.IsFolder && comic.IsFromZip)
                {
                    comic.Link.DeleteDirectory();
                }
                ViewModel.LocalComics.Remove(comic);
            }
        }
        /// <summary>
        /// 复选框-记住选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Remember_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            Config.IsRememberDeleteFilesWithComicDelete = (bool)box.IsChecked;
        }
        /// <summary>
        /// 复选框-一起删除缓存文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteFiles_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            Config.IsDeleteFilesWithComicDelete = (bool)box.IsChecked;
        }
        /// <summary>
        /// 按键响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Key == VirtualKey.A && WindowHelper.GetWindowForXamlRoot(XamlRoot).CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
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
        /// 菜单按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenu(sender as UIElement);
        }
        /// <summary>
        /// 书架设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BookShelfSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

}
