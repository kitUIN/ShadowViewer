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
            ShadowCommandAddTag.IsEnabled = isComicBook;
            ShadowCommandStatus.IsEnabled = isComicBook & isSingle;
            ShadowCommandReImg.IsEnabled = isComicBook & isSingle;
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
            await XamlHelper.CreateRenameDialog(I18nHelper.GetString("ShadowCommandRenameToolTip.Content"),XamlRoot, comic.Name, comic.Img).ShowAsync();
        }

        private void ShadowCommandDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private  void ShadowCommandAddTag_Click(object sender, RoutedEventArgs e)
        {
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            var newWindow = new FileWindow();
            WindowHelper.TrackWindow(newWindow);
            newWindow.Navigate(typeof(StatusPage), comic);
            isLoaded = false;
        }

        private void ShadowCommandMove_Click(object sender, RoutedEventArgs e)
        {

        }
        private async void ShadowCommandReImg_Click(object sender, RoutedEventArgs e)
        {
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            await XamlHelper.CreateRenameDialog(I18nHelper.GetString("ShadowCommandReImgToolTip.Content"),XamlRoot, comic.Name, comic.Img).ShowAsync();
        }
        private async void ShadowCommandStatus_Click(object sender, RoutedEventArgs e)
        {
            LocalComic comic = ContentGridView.SelectedItems[0] as LocalComic;
            await XamlHelper.CreateStatusDialog(XamlRoot, comic).ShowAsync();
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
                if (item is LocalComic comic)
                {
                    //parameter = new PathMessage(comic.Id, parameter.ParentPath + "/" + comic.Name);
                    //Refresh();
                }
            }
        }

        private void HomeCommandBarFlyout_Closed(object sender, object e)
        {
            if (!isLoaded)
            {
                isLoaded = true;
                WindowHelper.ActiveWindows.Last().Activate();
            }
        }
    }
}
