using System;

namespace ShadowViewer.Pages
{
    public sealed partial class AttributesPage : Page
    {
        public AttributesViewModel ViewModel { get; set; }
        public AttributesPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is LocalComic comic)
            {
                ViewModel = new AttributesViewModel(comic);
            }
        }

        private async void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = XamlHelper.CreateOneTextBoxDialog(XamlRoot,
                I18nHelper.GetString("Shadow.String.Set"),
                I18nHelper.GetString("Xaml.TextBlock.Author.Text"),
      "", ViewModel.CurrentComic.Author,
      (s, e, t) =>
            {
                ViewModel.CurrentComic.Author = t;
            });
            await dialog.ShowAsync();
        }

        private async void FileNameButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = XamlHelper.CreateOneTextBoxDialog(XamlRoot,
                I18nHelper.GetString("Shadow.String.Set"),
                I18nHelper.GetString("Xaml.TextBlock.FileName.Text"),
      "", ViewModel.CurrentComic.Name,
      (s, e, t) =>
      {
          ViewModel.CurrentComic.Name = t;
      });
            await dialog.ShowAsync();
        }

        private async void GrouprButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = XamlHelper.CreateOneTextBoxDialog(XamlRoot,
                I18nHelper.GetString("Shadow.String.Set"),
                I18nHelper.GetString("Xaml.TextBlock.Group.Text"),
      "", ViewModel.CurrentComic.Group,
      (s, e, t) =>
      {
          ViewModel.CurrentComic.Group = t;
      });
            await dialog.ShowAsync();
        }

        private void TopBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.TextBlockMaxWidth = ((Border)sender).ActualWidth - 330;
            TagBorder.Width = InfoBorder.ActualWidth + 205;
        }

        private void Tag_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ShadowTag tag = (ShadowTag)button.Tag; 
            if (tag.IsIcon)
            {
                TagName.Text = "";
                YesIcon.Symbol = FluntIcon.FluentIconSymbol.TagFilled;
                YesText.Text = I18nHelper.GetString("Shadow.String.AddNew");
            }
            else
            {
                BackgroundColorPicker.SelectedColor = ((SolidColorBrush)button.Background).Color;
                ForegroundColorPicker.SelectedColor = ((SolidColorBrush)button.Foreground).Color;
                TagName.Text = ((TextBlock)((StackPanel)button.Content).Children[1]).Text;
                YesIcon.Symbol = FluntIcon.FluentIconSymbol.TagResetFilled;
                YesText.Text = I18nHelper.GetString("Shadow.String.Update");
            }
            RemoveTagButton.Visibility = (!tag.IsIcon).ToVisibility();
            YesToolTip.Content = YesText.Text;
            if (tag.IsEnable)
            {
                TagSelectFlyout.ShowAt(sender as FrameworkElement);
            }
        }
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddNewTag(new ShadowTag(TagName.Text, background: BackgroundColorPicker.SelectedColor, foreground: ForegroundColorPicker.SelectedColor));
        }

        private void TagName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (ShadowTag.Query().Any(x => x.Name == box.Text))
            {
                YesIcon.Symbol = FluntIcon.FluentIconSymbol.TagResetFilled;
                YesText.Text = I18nHelper.GetString("Shadow.String.Update");
            }
            else
            {
                YesIcon.Symbol = FluntIcon.FluentIconSymbol.TagFilled;
                YesText.Text = I18nHelper.GetString("Shadow.String.AddNew");
            }
        }

        private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveTag(TagName.Text);
        }
    }
}
