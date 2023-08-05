

namespace ShadowViewer.Pages
{
    /// <summary>
    /// ��������ҳ
    /// </summary>
    public sealed partial class AttributesPage : Page
    {
        public AttributesViewModel ViewModel { get; set; }
        private string TagId { get; set; }
        public AttributesPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is LocalComic comic)
            {
                ViewModel = DiFactory.Current.Services.GetService<AttributesViewModel>();
                ViewModel.Init(comic.Id);
            }
        }
        /// <summary>
        /// ���ͼƬ
        /// </summary>
        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = await FileHelper.SelectPicFileAsync(XamlRoot);
            if (file != null)
            {
                ViewModel.CurrentComic.Img = file.DecodePath();
            }
        }
        /// <summary>
        /// �޸�����
        /// </summary>
        private async void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = XamlHelper.CreateOneTextBoxDialog(XamlRoot,
                CoreResourcesHelper.GetString(CoreResourceKey.Set),
                CoreResourcesHelper.GetString(CoreResourceKey.Author),
      "", ViewModel.CurrentComic.Author,
      (s, e, t) =>
            {
                ViewModel.CurrentComic.Author = t;
            });
            await dialog.ShowAsync();
        }
        /// <summary>
        /// �޸���������
        /// </summary>
        private async void FileNameButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = XamlHelper.CreateOneTextBoxDialog(XamlRoot,
                CoreResourcesHelper.GetString(CoreResourceKey.Set),
                CoreResourcesHelper.GetString(CoreResourceKey.FileName),
      "", ViewModel.CurrentComic.Name,
      (s, e, t) =>
      {
          ViewModel.CurrentComic.Name = t;
      });
            await dialog.ShowAsync();
        }
        /// <summary>
        /// �޸ĺ�����
        /// </summary>
        private async void GrouprButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = XamlHelper.CreateOneTextBoxDialog(XamlRoot,
                CoreResourcesHelper.GetString(CoreResourceKey.Set),
                CoreResourcesHelper.GetString(CoreResourceKey.Group),
      "", ViewModel.CurrentComic.Group,
      (s, e, t) =>
      {
          ViewModel.CurrentComic.Group = t;
      });
            await dialog.ShowAsync();
        }
        /// <summary>
        /// �����ǩ
        /// </summary>
        private void Tag_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LocalTag tag = (LocalTag)button.Tag;
            if (ViewModel.IsLastTag(tag))
            {
                TagName.Text = "";
                YesIcon.Symbol = FluntIcon.FluentIconSymbol.TagFilled;
                YesText.Text = CoreResourcesHelper.GetString(CoreResourceKey.AddNew);
                RemoveTagButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                BackgroundColorPicker.SelectedColor = ((SolidColorBrush)button.Background).Color;
                ForegroundColorPicker.SelectedColor = ((SolidColorBrush)button.Foreground).Color;
                TagName.Text = ((TextBlock)((StackPanel)button.Content).Children[1]).Text;
                YesIcon.Symbol = FluntIcon.FluentIconSymbol.TagResetFilled;
                YesText.Text = CoreResourcesHelper.GetString(CoreResourceKey.Update);
                RemoveTagButton.Visibility = Visibility.Visible;
            }
            YesToolTip.Content = YesText.Text;
            if (tag.IsEnable)
            {
                TagId = tag.Id;
                TagSelectFlyout.ShowAt(sender as FrameworkElement);
            }
        }
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TagName.Text)) return;
            ViewModel.AddNewTag(new LocalTag(TagName.Text, background: BackgroundColorPicker.SelectedColor, foreground: ForegroundColorPicker.SelectedColor) { Id= TagId });
            TagSelectFlyout.Hide();
        }
        /// <summary>
        /// ����-ɾ��
        /// </summary>
        private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveTag(TagId);
            TagSelectFlyout.Hide();
        }

        /// <summary>
        /// �ؼ���ʼ��
        /// </summary>
        private void TopBorder_Loaded(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// ��ת��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Episode_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void TagName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == VirtualKey.Enter)
            {
                Yes_Click(null, null);
            }
        }

        private void IDButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        /// <summary>
        /// ��С��Ӧ����
        /// </summary>
        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = ((FrameworkElement)sender).ActualWidth;
            InfoStackPanel1.Width = width - 200;
            ViewModel.TextBlockMaxWidth = width - 330;

        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(((FrameworkElement)sender).Tag.ToString());
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }
}