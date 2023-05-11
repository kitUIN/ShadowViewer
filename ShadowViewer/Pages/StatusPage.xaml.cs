namespace ShadowViewer.Pages
{
    public sealed partial class StatusPage : Page
    {
        private StatusViewModel ViewModel { get; } = new StatusViewModel();
 
        public StatusPage()
        {
            this.InitializeComponent();
            this.RequestedTheme = ThemeHelper.RootTheme;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is LocalComic comic)
            {
                ViewModel.Navigate(comic, this.Frame);
                AuthorName.Visibility = comic.IsFolder? Visibility.Collapsed:Visibility.Visible;
                GroupName.Visibility = comic.IsFolder? Visibility.Collapsed:Visibility.Visible;
            }
        }
        /// <summary>
        /// 点击图片
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TappedRoutedEventArgs"/> instance containing the event data.</param>
        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var file = await FileHelper.SelectFileAsync(sender as Button, ".png", ".jpg", ".jpeg", ".bmp");
            if (file != null && file.DecodePath() != ViewModel.Comic.Img)
            {
                ViewModel.Comic.Img = file.DecodePath();
                MessageHelper.SendFilesReload();
            }
        }
        /// <summary>
        /// 图片文本框回车
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyRoutedEventArgs"/> instance containing the event data.</param>
        private async void FileImg_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) { return; }
            var box = sender as TextBox;
            Uri uri;
            bool flag = false;
            string title = I18nHelper.GetString("Shadow.Error.Title");
            string message = "";
            try
                {
                    uri = new Uri(box.Text);
                    StorageFile file = uri.Scheme == "ms-appx" ? await StorageFile.GetFileFromApplicationUriAsync(uri) : await StorageFile.GetFileFromPathAsync(uri.DecodeUri());
                    if (!(file.IsPic()))
                    {
                        flag = true;
                        message = I18nHelper.GetString("Error.Message.NotImage");
                    }
                }
                catch (UriFormatException)
                {
                    flag = true;
                    message = I18nHelper.GetString("Error.Message.UriFormatException");
                }
                catch (Exception exception) when (exception is System.Runtime.InteropServices.COMException || exception is FileNotFoundException)
                {
                    flag = true;
                    message = I18nHelper.GetString("Error.Message.NoFile");
                } 
                if(flag)
                {
                    box.Text = ViewModel.Comic.Img;
                    await XamlHelper.CreateMessageDialog(this.XamlRoot, title, message).ShowAsync();
                    return;
                }
                ViewModel.Comic.Img = box.Text;
                MessageHelper.SendFilesReload();
        }
        /// <summary>
        /// 文件名回车
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyRoutedEventArgs"/> instance containing the event data.</param>
        private void FileName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.Comic.Name = box.Text;
                MessageHelper.SendFilesReload();
            }
        }
        /// <summary>
        /// 作者名按下回车
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyRoutedEventArgs"/> instance containing the event data.</param>
        private void AuthorName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.Comic.Author = box.Text;
                MessageHelper.SendFilesReload();
            }
        }
        /// <summary>
        /// 汉化组按下回车
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyRoutedEventArgs"/> instance containing the event data.</param>
        private void GroupName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.Comic.Group = box.Text;
                MessageHelper.SendFilesReload();
            }
        }
        public SolidColorBrush ToBrush(Color color)
        {
            return new SolidColorBrush(color);
        }
         
        /// <summary>
        /// 控制图片名 文件名 作者 汉化组4个框
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var s = sender as StackPanel;
            FileImg.Width = s.ActualWidth - 170;
            FileName.Width = s.ActualWidth - 170;
            AuthorName.Width = s.ActualWidth - 170;
            GroupName.Width = s.ActualWidth - 170;
        }
    }
}
