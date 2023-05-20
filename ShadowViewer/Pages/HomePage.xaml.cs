namespace ShadowViewer.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomeViewModel ViewModel { get; set; }
        public HomePage()
        {
            this.InitializeComponent(); 
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = new HomeViewModel(e.Parameter as Uri);
        }
        /// <summary>
        /// 显示右键菜单
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="isComicBook">if set to <c>true</c> [is comic book].</param>
        /// <param name="isSingle">if set to <c>true</c> [is single].</param>
        /// <param name="isFolder">if set to <c>true</c> [is folder].</param>
        private void ShowMenu(Point position, UIElement sender, bool isComicBook, bool isSingle, bool isFolder)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions()
            {
                ShowMode = FlyoutShowMode.Standard,
                Position = position
            };
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
            bool isComicBook = false;
            bool isSingle = false;
            bool isFolder = false;
            if (ContentGridView.SelectedItems.Count > 0)
            {
                isComicBook = true;
            }
            if (ContentGridView.SelectedItems.Count == 1)
            {
                isSingle = true;
            }
            ShowMenu(e.GetPosition(sender as UIElement), sender as UIElement, isComicBook, isSingle, isFolder);
        }
        
        
        /// <summary>
        /// 右键菜单-新建漫画从文件夹导入
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddFromFolder_Click(object sender, RoutedEventArgs e)
        {

            var folder = await FileHelper.SelectFolderAsync(this, "AddNewComic");
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
            var storageFile = await FileHelper.SelectFileAsync(this, ".zip",".rar",".7z");
            if (storageFile != null)
            {
                try
                {
                    List<Task> backgrounds = new List<Task>();
                    LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
                    if (storageFile.IsZip())
                    {
                        LoadingControl.IsLoading = true;
                        LocalComic comic = await ComicHelper.ImportComicsFromZip(storageFile.Path, App.Config.TempPath);
                        backgrounds.Add(Task.Run(() => ComicHelper.EntryToComic(App.Config.ComicsPath, comic, storageFile.Path))); ;
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
            foreach (LocalComic comic in ContentGridView.SelectedItems.ToList())
            {
                ViewModel.LocalComics.Remove(comic);
            }
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
        private void ContentGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
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
                        Frame.Navigate(typeof(PicPage), ComicHelper.Entrys[comic.Link]);
                    }
                    else
                    {

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
                ViewModel.LocalComics.Add(ComicHelper.CreateFolder(name, "", parent));
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

        private void SmokeGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid.ActualHeight > 50)
            {
                destinationElement.Height = grid.ActualHeight - 50;
            }
        }

        private void SmokeGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            SortText.Text = ((MenuFlyoutItem)sender).Text;
            ViewModel.Sorts = EnumHelper.GetEnum<ShadowSorts>(((MenuFlyoutItem)sender).Tag.ToString());
            ViewModel.RefreshLocalComic();
        }

        private void SortButton_Loaded(object sender, RoutedEventArgs e)
        {
            SortText.Text = I18nHelper.GetString("Xaml/MenuFlyoutItem/RZ/Text");
        }

    }
}
