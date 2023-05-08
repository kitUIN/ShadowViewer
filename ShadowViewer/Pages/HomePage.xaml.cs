using Microsoft.UI.Xaml.Controls.Primitives;
using ShadowViewer.DataBases;
using Windows.ApplicationModel.DataTransfer;

namespace ShadowViewer.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomeViewModel ViewModel { get;} = new HomeViewModel();
        private bool isLoaded = false;
        private Window window;
        public HomePage()
        {
            this.InitializeComponent(); 
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Navigate(e.Parameter);
        }
        /// <summary>
        /// ��ʾ�Ҽ��˵�
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
        /// �Ҽ��˵�-�½���������
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
            await CreateRenameDialog(I18nHelper.GetString("ShadowCommandRenameToolTip.Content"),XamlRoot, comic).ShowAsync();
        }
        /// <summary>
        /// �Ҽ��˵�-ɾ��
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide(); 
            foreach(LocalComic comic in ContentGridView.SelectedItems)
            {
                comic.RemoveInDB(); 
                WindowHelper.ColseWindowFromTitle(comic.Name);
            }
            MessageHelper.SendFilesReload();
        }


        /// <summary>
        /// �Ҽ��˵�-�ƶ���
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
        /// �Ҽ��˵�-��ӱ�ǩ
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandAddTag_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            NavigateToStatus(comic, true);
        }
        /// <summary>
        /// �Ҽ��˵�-�鿴����
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShadowCommandStatus_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            NavigateToStatus(comic, false);
        }
        /// <summary>
        /// ��ת������ҳ��
        /// </summary>
        /// <param name="comic">The comic.</param>
        /// <param name="isTag">if set to <c>true</c> [is tag].</param>
        /// <param name="name">The name.</param>
        /// <param name="isMessage">if set to <c>true</c> [is message].</param>
        /// <param name="isLoaded">if set to <c>true</c> [is loaded].</param>
        private void NavigateToStatus(LocalComic comic, bool isTag = false, string name = null, bool isMessage = false , bool isLoaded = false)
        {
            var newWindow = WindowHelper.GetWindowForTitle(name ?? comic.Name) as FileWindow;
            if (newWindow == null)
            {
                newWindow = new FileWindow();
                WindowHelper.TrackWindow(newWindow);
            }
            newWindow.Title = comic.Name;
            List<object> args = new List<object>
            {
                comic
            };
            if (!isMessage)
            {
                args.Add(isTag);
            }
            newWindow.Navigate(typeof(StatusPage), args, I18nHelper.GetString("FileAppTitle.Text"));
            this.isLoaded = isLoaded;
            window = newWindow;
        }
        
        private void ShadowCommandRefresh_Click(object sender, RoutedEventArgs e)
        {
            MessageHelper.SendFilesReload();
        }
        /// <summary>
        /// �Ҽ�ѡ��GridViewItem
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
        /// ˫���ļ��л�������
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
                    Frame.Navigate(this.GetType(), ViewModel.OriginPath + "/" + comic.Name);
                }
            }
        }
        /// <summary>
        /// �Ҽ��˵��ر�ʱ����
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void HomeCommandBarFlyout_Closed(object sender, object e)
        {
            if (!isLoaded && window!=null)
            {
                isLoaded = true;
                window.Activate();
            }
        }
        /// <summary>
        /// ����һ��ԭʼ�ĶԻ���
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
        /// �������Ի���
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
        /// �½��ļ��жԻ���
        /// </summary>
        /// <returns></returns>
        public ContentDialog CreateFolderDialog(XamlRoot xamlRoot, string parent)
        {
            ContentDialog dialog = CreateRawDialog(I18nHelper.GetString("Dialog/CreateFolder/Title"), xamlRoot, "");
            dialog.IsPrimaryButtonEnabled = true;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var name = ((TextBox)((StackPanel)((StackPanel)dialog.Content).Children[0]).Children[1]).Text;
                ComicDB.Add(name, "", parent);
                MessageHelper.SendFilesReload();
            };
            return dialog;
           
        }
        /// <summary>
        /// ��ر仯
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PathBar.Width = ((Grid)sender).ActualWidth - 200;
             
        }
        /// <summary>
        /// ��ַ�����
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="BreadcrumbBarItemClickedEventArgs"/> instance containing the event data.</param>
        private void PathBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            int index = ViewModel.Paths.IndexOf(args.Item as string);
            if (index != -1)
            {
                List<string> list = ViewModel.Paths.GetRange(0, index + 1);
                Frame.Navigate(this.GetType(), "shadow://"+ string.Join("/", list));
            }
            
        }
        /// <summary>
        /// �ƶ��� �Ի���İ�ť��Ӧ
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
        /// �ƶ�������ļ���
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
        /// �����νṹ��˫��
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
        /// �����϶�
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
        /// �϶�������ʾ
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
                    e.DragUIOverride.IsCaptionVisible = comic.IsFolder;
                }
                else if (frame.Tag is string name)
                {
                    parent = name;
                    e.AcceptedOperation =  DataPackageOperation.Move;
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
        /// �϶���ʼ���� ��ӵ�GridViewѡ��
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
        /// �ⲿ�ļ����������Ӧ
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
        /// �ⲿ�ļ��϶�������ʾ
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
                ImportText.Text = "���ļ��ϵ��Ⲣ����Ϊ����";
            }
        }
        /// <summary>
        /// �ⲿ�ļ��϶��뿪
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void Root_DragLeave(object sender, DragEventArgs e)
        {
            OverBorder.Visibility = Visibility.Collapsed;
        }
    }
}
