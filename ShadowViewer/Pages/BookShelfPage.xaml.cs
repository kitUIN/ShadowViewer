using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Helpers;
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
        /// ��ʾ�Ҽ��˵�
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
        /// �Ҽ��˵�-�½��������ļ��е���
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
                await ComicHelper.ImportComicsFromFolder(folder, ViewModel.Path);
                ViewModel.RefreshLocalComic();
                LoadingControl.IsLoading = false;
            }
        }
        /// <summary>
        /// �Ҽ��˵�-�½�������ѹ���ļ�����
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
                    }
                    ViewModel.RefreshLocalComic();
                    LoadingControl.IsLoading = false;
                    await Task.WhenAll(backgrounds);
                    ViewModel.RefreshLocalComic();
                }
                catch(Exception ex)
                {
                    Log.Error("�Ҽ��˵�-�½�������ѹ���ļ����뱨��:{Ex}", ex);
                }
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
        private async void ContentGridView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is LocalComic comic)
            {
                if(comic.IsFolder)
                {
                    Frame.Navigate(GetType(), new Uri(ViewModel.OriginPath, comic.Id));
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
                if (Config.IsDeleteFilesWithComicDelete && !comic.IsTemp && !comic.IsFolder && comic.IsFromZip)
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
    }

}
