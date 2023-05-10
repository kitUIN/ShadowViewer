namespace ShadowViewer.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomeViewModel ViewModel { get;} = new HomeViewModel();

        public HomePage()
        {
            this.InitializeComponent(); 
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Navigate(e.Parameter as Uri);
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
        /// 右键菜单-新建漫画导入
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddFromFolder_Click(object sender, RoutedEventArgs e)
        {

            var folder = await FileHelper.SelectFolderAsync(this, "AddNewComic");
            if (folder != null)
            {
                await ComicHelper.ImportComicsAsync(folder, ViewModel.Path);
                MessageHelper.SendFilesReload();
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
            await CreateRenameDialog(I18nHelper.GetString("ShadowCommandRenameToolTip.Content"),XamlRoot, comic).ShowAsync();
        }
        /// <summary>
        /// 右键菜单-删除
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide(); 
            foreach(LocalComic comic in ContentGridView.SelectedItems)
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
            var black  = new List<string>();
            foreach(LocalComic comic in ContentGridView.SelectedItems)
            {
                black.Add(comic.Name);
            }
            MoveTreeView.ItemsSource = new List<ShadowPath>{ UriHelper.PathTreeInit(black) };
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
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            
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
        
        
        private void ShadowCommandRefresh_Click(object sender, RoutedEventArgs e)
        {
            MessageHelper.SendFilesReload();
        }
        /// <summary>
        /// 右键选中GridViewItem
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RightTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void ContentGridView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                var item = element.DataContext;
                if (item != null)
                {
                    var container = (GridViewItem)ContentGridView.ContainerFromItem(item);
                    if (container != null && !container.IsSelected)
                    {
                        ContentGridView.SelectedItems.Clear();
                        container.IsSelected = true;
                    }
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
            if (e.OriginalSource is FrameworkElement element)
            {
                var item = element.DataContext;
                if (item is LocalComic comic && comic.IsFolder)
                {
                    Frame.Navigate(this.GetType(), new Uri(ViewModel.OriginPath + "/" + comic.Id));
                }
            }
        }
 
        
        /// <summary>
        /// 创建一个原始的对话框
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="xamlRoot">The xaml root.</param>
        /// <param name="oldName">The old name.</param>
        /// <returns></returns>
        private ContentDialog CreateRawDialog(string title, XamlRoot xamlRoot, string oldName)
        {
            ContentDialog dialog = XamlHelper.CreateContentDialog(xamlRoot);
            dialog.Title = title;
            dialog.PrimaryButtonText = I18nHelper.GetString("Dialog/ConfirmButton");
            dialog.CloseButtonText = I18nHelper.GetString("Dialog/CloseButton");
            StackPanel grid = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Vertical,
            };
            Button selectImg = new Button()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Content = new SymbolIcon(Symbol.Folder),
            };
            var nameBox = XamlHelper.CreateOneLineTextBox(I18nHelper.GetString("Dialog/CreateFolder/Name"),
                I18nHelper.GetString("Dialog/CreateFolder/Title"), oldName, 222);
            ((TextBox)nameBox.Children[1]).TextChanged += (s, e) =>
            {
                var sender = s as TextBox;
                dialog.IsPrimaryButtonEnabled = !ComicDB.Contains("Name", sender.Text);
            };
            grid.Children.Add(nameBox);
            dialog.Content = grid;
            return dialog;
        }
        /// <summary>
        /// 重命名对话框
        /// </summary>
        /// <returns></returns>
        private ContentDialog CreateRenameDialog(string title, XamlRoot xamlRoot, LocalComic comic)
        {
            ContentDialog dialog = CreateRawDialog(title,xamlRoot,comic.Name);
            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonClick +=  (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)dialog.Content).Children[0]).Children[1]).Text;
                var oldname = comic.Name;
                comic.Name = name;
                MessageHelper.SendFilesReload();
                MessageHelper.SendStatusReload();
            };
            return dialog;
        }
        /// <summary>
        /// 新建文件夹对话框
        /// </summary>
        /// <returns></returns>
        public ContentDialog CreateFolderDialog(XamlRoot xamlRoot, string parent)
        {
            ContentDialog dialog = CreateRawDialog(I18nHelper.GetString("Dialog/CreateFolder/Title"), xamlRoot, "");
            dialog.IsPrimaryButtonEnabled = true;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)dialog.Content).Children[0]).Children[1]).Text;
                ComicHelper.CreateFolder(name,"", parent);
                MessageHelper.SendFilesReload();
            };
            return dialog;
           
        }
        
        /// <summary>
        /// 移动到 对话框的按钮响应
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void MoveTeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            if(MoveTreeView.SelectedItem is ShadowPath path)
            {
                MoveToPath(path.Name);
            }
        }
        /// <summary>
        /// 移动到别的文件夹
        /// </summary>
        /// <param name="path">The path.</param>
        private void MoveToPath(string path)
        {
            foreach (LocalComic comic in ContentGridView.SelectedItems)
            {
                comic.Parent = path;
            }
            MoveTeachingTip.IsOpen = false;
            MessageHelper.SendFilesReload();
            MessageHelper.SendStatusReload();
        }
        /// <summary>
        /// 在树形结构上双击
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoubleTappedRoutedEventArgs"/> instance containing the event data.</param>
        private void TreeViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (MoveTreeView.SelectedItem is ShadowPath path)
            {
                MoveToPath(path.Name);
            }
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
                string parent;
                if(frame.Tag is LocalComic comic && comic.IsFolder)
                {
                    parent = comic.Name;
                }
                else if(frame.Tag is string name)
                {
                    parent = name;
                }
                else
                {
                    return;
                }
                foreach (LocalComic item in ContentGridView.SelectedItems)
                {
                    item.Parent = parent;
                }
                MessageHelper.SendFilesReload();
                MessageHelper.SendStatusReloadDB();
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
                string parent;
                if (frame.Tag is LocalComic comic && comic.IsFolder)
                {
                    parent = comic.Name;
                    e.AcceptedOperation = comic.IsFolder ? DataPackageOperation.Move : DataPackageOperation.None;
                    e.DragUIOverride.IsCaptionVisible = true;
                }
                else
                {
                    return;
                }
                e.DragUIOverride.Caption = I18nHelper.GetString("ShadowCommandMove.Label") + parent;
                e.DragUIOverride.IsGlyphVisible = true;
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
        /// <summary>
        /// 外部文件拖入进行响应
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private async void Root_Drop(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var item = await e.DataView.GetStorageItemsAsync();
                if (item.Count == 1 && item[0] is StorageFolder folder)
                {
                    await ComicHelper.ImportComicsAsync(folder, ViewModel.Path);
                }
                MessageHelper.SendFilesReload();
            }
        }
        /// <summary>
        /// 外部文件拖动悬浮显示
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void Root_DragOver(object sender, DragEventArgs e)
        {

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Link;
                e.DragUIOverride.Caption = I18nHelper.GetString("String.Import");
                OverBorder.Visibility = Visibility.Visible;
                OverBorder.Width = Root.ActualWidth - 20;
                OverBorder.Height = Root.ActualHeight - 20;
                ImportText.Text = "将文件拖到这并导入为漫画";
            }
        }
        /// <summary>
        /// 外部文件拖动离开
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void Root_DragLeave(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
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
    }
}
