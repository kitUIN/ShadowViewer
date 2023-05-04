// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;

namespace ShadowViewer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private ShadowPath parameter;
        private HomeViewModel viewModel;
        private bool isLoaded = false;
        private Window window;
        public HomePage()
        {
            this.InitializeComponent();
        }
        public void Refresh()
        {
            this.Frame.Navigate(this.GetType(), parameter);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if(e.Parameter is ShadowPath parameter)
            {
                this.parameter = parameter;
            }else if(e.Parameter == null)
            {
                this.parameter = new ShadowPath("/");
            }
            viewModel = new HomeViewModel(this.parameter);
        }
        private void ShowMenu(Point position, UIElement sender, bool isComicBook, bool isSingle, bool isFolder)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions()
            {
                ShowMode = FlyoutShowMode.Standard,
                Position = position
            };
            ShadowCommandRename.IsEnabled = isComicBook & isSingle;
            ShadowCommandDelete.IsEnabled = isComicBook;
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
        private async void ShadowCommandAddFromFolder_Click(object sender, RoutedEventArgs e)
        {

            var folder = await FileHelper.SelectFolderAsync(this, "AddNewComic");
            if (folder != null)
            {

            }
        }
        private async void ShadowCommandAddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            await XamlHelper.CreateFolderDialog(XamlRoot, parameter.paths.Last()).ShowAsync();
        }

        private async void ShadowCommandRename_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            await CreateRenameDialog(I18nHelper.GetString("ShadowCommandRenameToolTip.Content"),XamlRoot, comic.Name, comic.Img).ShowAsync();
        }

        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            var comics =  ContentGridView.SelectedItems;
            foreach(LocalComic comic in comics)
            {
                comic.RemoveInDB();
                viewModel.LocalComics.Remove(comic);
                WindowHelper.ColseWindowFromTitle(comic.Name);
            }
            MessageHelper.SendFilesReload();
        }

        

        private void ShadowCommandMove_Click(object sender, RoutedEventArgs e)
        {

        }
        private  void ShadowCommandAddTag_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            NavigateToStatus(comic, true);
        }
        private void ShadowCommandStatus_Click(object sender, RoutedEventArgs e)
        {
            HomeCommandBarFlyout.Hide();
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            NavigateToStatus(comic, false);
        }
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
                if (item is LocalComic comic)
                {
                    //parameter = new PathMessage(comic.Id, parameter.ParentPath + "/" + comic.Name);
                    //Refresh();
                }
            }
        }

        private void HomeCommandBarFlyout_Closed(object sender, object e)
        {
            if (!isLoaded && window!=null)
            {
                isLoaded = true;
                window.Activate();
            }
        }

        /// <summary>
        /// ������/����ͼ��Ի���
        /// </summary>
        /// <returns></returns>
        private ContentDialog CreateRenameDialog(string title, XamlRoot xamlRoot, string oldname, string oldimg)
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
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal,
            };
            Button selectImg = new Button()
            {
                Margin = new Thickness(10, 0, 0, 0),
                Content = new SymbolIcon(Symbol.Folder),
            };
            var imgBox = XamlHelper.CreateOneLineTextBox(I18nHelper.GetString("Dialog/CreateFolder/Img"), "", oldimg, 163);
            selectImg.Click += async (s, e) =>
            {
                Button button = s as Button;
                var file = await FileHelper.SelectFileAsync(dialog, ".png", ".jpg", ".jpeg");
                if (file != null)
                {
                    ((TextBox)imgBox.Children[1]).Text = file.Path;
                }
            };
            stackPanel.Children.Add(imgBox);
            stackPanel.Children.Add(selectImg);
            var nameBox = XamlHelper.CreateOneLineTextBox(I18nHelper.GetString("Dialog/CreateFolder/Name"),
                I18nHelper.GetString("Dialog/CreateFolder/Title"), oldname, 222);
            grid.Children.Add(nameBox);
            grid.Children.Add(stackPanel);
            dialog.Content = grid;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var img = ((TextBox)imgBox.Children[1]).Text;
                var name = ((TextBox)nameBox.Children[1]).Text;
                if (img != oldimg)
                {
                    ComicDB.Update("Img", "Name", img, oldname);
                }
                if (name != oldname)
                {
                    ComicDB.Update("Name", "Name", name, oldname);
                }
                MessageHelper.SendFilesReload();
                if(img != oldimg || name != oldname)
                {
                    if(ComicDB.GetFirst("Name", name) is LocalComic comic)
                    {
                        NavigateToStatus(comic, false, oldname, true, true);
                    }
                }
            };
            return dialog;
        }
    }
}
