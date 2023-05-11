using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Helpers;
using System.Linq;

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
        /// 从文件夹导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        private async Task ImportComicsAsync(StorageFolder folder, string parent,string id =null)
        {
            LoadingControl.IsLoading = true;
            LoadingControlText.Text = I18nHelper.GetString("Shadow.String.ImportLoading");
            ulong size = 0;
            var file = await ShadowFile.Create(folder, async (s) => {
                if (s is StorageFile file && file.IsPic())
                { size += (await file.GetBasicPropertiesAsync()).Size; }
            });
            string img = null;
            if (file.Depth > 2)
            {
                while (file.Depth > 2)
                {
                    file = file.Children.FirstOrDefault(x => x.Self is StorageFolder);
                }
            }
            img = file.Children.FirstOrDefault(x => x.Self is StorageFile f && f.IsPic())?.Self.Path ?? "";
            LoadingControl.IsLoading = false;
            ViewModel.LocalComics.Add(ComicHelper.CreateComic(((StorageFolder)file.Self).DisplayName, img, parent, file.Self.Path, id: id, size: (long)size));
        }
        /// <summary>
        /// 从zip导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        private async Task ImportComicsAsync(StorageFile storageFile, string parent)
        {
            LoadingControl.IsLoading = true;
            LoadingControlText.Text = I18nHelper.GetString("Shadow.String.Compress");
            string id = Guid.NewGuid().ToString("N");
            while (ComicDB.Contains(nameof(id), id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            StorageFolder comicsFolder = await StorageFolder.GetFolderFromPathAsync(App.Config.ComicsPath);
            var folder = await comicsFolder.CreateFolderAsync(id);
            string path = Path.Combine(App.Config.ComicsPath, id);
            if (storageFile.FileType == ".zip")
            {
                FileHelper.ZipCompress(storageFile.Path, path);
            }
            else if(storageFile.FileType == ".rar")
            {
                FileHelper.RarCompress(storageFile.Path, path);
            }
            
            await ImportComicsAsync(folder, parent, id);
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
                await ImportComicsAsync(folder, ViewModel.Path);
            }
        }
        /// <summary>
        /// 右键菜单-新建漫画从压缩文件导入
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ShadowCommandAddFromZip_Click(object sender, RoutedEventArgs e)
        {

            var file = await FileHelper.SelectFileAsync(this, ".zip",".rar",".7z");
            if (file != null)
            {
                await ImportComicsAsync(file,ViewModel.Path);
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
            await CreateRenameDialog(I18nHelper.GetString("ShadowCommandRenameToolTip.Content"), XamlRoot, comic).ShowAsync();
        }
        /// <summary>
        /// 右键菜单-删除
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide(); 
            foreach (LocalComic comic in ContentGridView.SelectedItems)
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
            if (e.OriginalSource is FrameworkElement element && element.DataContext is LocalComic comic && comic.IsFolder)
            {
                Frame.Navigate(this.GetType(), new Uri(ViewModel.OriginPath, comic.Id));
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
            var nameBox = XamlHelper.CreateOneLineTextBox(I18nHelper.GetString("Dialog/CreateFolder/Name"),
                I18nHelper.GetString("Dialog/CreateFolder/Title"), oldName, 222);
            grid.Children.Add(nameBox);
            dialog.Content = grid;
            dialog.IsPrimaryButtonEnabled = true;
            return dialog;
        }
        /// <summary>
        /// 重命名对话框
        /// </summary>
        /// <returns></returns>
        private ContentDialog CreateRenameDialog(string title, XamlRoot xamlRoot, LocalComic comic)
        {
            ContentDialog dialog = CreateRawDialog(title, xamlRoot, comic.Name); 
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
            ContentDialog dialog = CreateRawDialog(I18nHelper.GetString("Dialog/CreateFolder/Title"), xamlRoot, "");
            
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
                    e.DragUIOverride.Caption = I18nHelper.GetString("ShadowCommandMove.Label") + comic.Name;
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
                var items = await e.DataView.GetStorageItemsAsync();
                foreach (var item1 in items.Where(x => x is StorageFolder))
                {
                    await ImportComicsAsync(item1 as StorageFolder, ViewModel.Path);
                }
                foreach (var item2 in items.Where(x => x is StorageFile file && file.IsZip()))
                {
                     
                }
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
