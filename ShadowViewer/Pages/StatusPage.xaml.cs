


namespace ShadowViewer.Pages
{
    public sealed partial class StatusPage : Page
    {
        private StatusViewModel viewModel;
 
        public StatusPage()
        {
            this.InitializeComponent();
            this.RequestedTheme = ThemeHelper.RootTheme;
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            if (e.Parameter is List<object> args)
            {
                bool isTag = false;
                if (args.Count >= 2 && args[1] is bool flag)
                {
                    isTag = flag;
                }
                if (args.Count >= 1 && args[0] is LocalComic comic)
                {
                    viewModel = new StatusViewModel(comic, isTag, this.Frame, TagIdBox);
                }
            }
            TagIdBox.Text = "0";
            TagNameBox.Text = I18nHelper.GetString("String.Default.Tag");
        }
        /// <summary>
        /// 点击图片
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TappedRoutedEventArgs"/> instance containing the event data.</param>
        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var file = await FileHelper.SelectFileAsync(sender as Button, ".png", ".jpg", ".jpeg", ".bmp");
            if (file != null && file.DecodePath() != viewModel.Comic.Img)
            {
                viewModel.Comic.Img = file.DecodePath();
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
            var box = sender as TextBox;
            Uri uri = null;
            bool flag = false;
            string title = I18nHelper.GetString("Shadow.Error.Title");
            string message = "";
            if (e.Key == VirtualKey.Enter)
            {
                try
                {
                    uri = new Uri(box.Text);
                    StorageFile file = null;
                    if (box.Text.StartsWith("ms-appx"))
                    {
                        file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                    }
                    else
                    {
                        file = await StorageFile.GetFileFromPathAsync(uri.DecodeUri());
                    }
                    if (file != null)
                    {
                        if (!(file.FileType == ".jpg" ||
                            file.FileType == ".jpeg" ||
                            file.FileType == ".bmp" ||
                            file.FileType == ".png"))
                        {
                            flag = true;
                            message = I18nHelper.GetString("Error.Message.NotImage");
                        }
                    }
                    else
                    {
                        flag = true;
                        message = I18nHelper.GetString("Error.Message.NoFile");
                    }
                }
                catch (UriFormatException)
                {
                    flag = true;
                    message = I18nHelper.GetString("Error.Message.UriFormatException");
                }
                catch (System.Runtime.InteropServices.COMException )
                {
                    flag = true;
                    message = I18nHelper.GetString("Error.Message.NoFile");
                }
                catch(System.IO.FileNotFoundException)
                {
                    flag = true;
                    message = I18nHelper.GetString("Error.Message.NoFile");
                }
                if(flag)
                {
                    box.Text = viewModel.Comic.Img;
                    await XamlHelper.CreateMessageDialog(this.XamlRoot, title, message).ShowAsync();
                    return;
                }
                viewModel.Comic.Img = box.Text;
                MessageHelper.SendFilesReload();
            }
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
                var old = viewModel.Comic.Name;
                try 
                { 
                    viewModel.Comic.Name = box.Text;
                    WindowHelper.GetWindowForElement(this).Title = viewModel.Comic.Name;
                    MessageHelper.SendFilesReload();
                    viewModel.Reload();
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex)
                {
                    string message = ex.Message;
                    if (ex.SqliteErrorCode == 19)
                    {
                        message = I18nHelper.GetString("Shadow.Error.SQLite.Unique");
                    }
                    box.Text = old;
                    _ = XamlHelper.CreateMessageDialog(XamlRoot, I18nHelper.GetString("Shadow.Error.Title"), message).ShowAsync();
                } 
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
                viewModel.Comic.Author = box.Text;
                MessageHelper.SendFilesReload();
                viewModel.Reload();
            }
        }
        public SolidColorBrush ToBrush(Color color)
        {
            return new SolidColorBrush(color);
        }
        /// <summary>
        /// 点击添加标签
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AddNewTag_Click(object sender, RoutedEventArgs e)
        {
            if (TagIdBox.Text == "" || TagNameBox.Text == "") { return; }
            var tag = new ShadowTag(TagIdBox.Text, TagNameBox.Text,
                ForegroundColorPicker.SelectedColor, BackgroundColorPicker.SelectedColor);
            if (!TagsHelper.ShadowTags.Any(x => x.tag == tag.tag))
            {
                TagDB.Add(tag);
                TagsHelper.ShadowTags.Add(tag);
            }
            viewModel.Comic.AnotherTags.Add(tag.tag);
            viewModel.LoadTags();
        }
        /// <summary>
        /// 标签ID变化
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TagIdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Contains(","))
            {
                ((TextBox)sender).Undo();
                return;
            } 
            if (TagsHelper.ShadowTags.FirstOrDefault(x => x.tag == ((TextBox)sender).Text) is ShadowTag shadowTag)
            {
                if(TagNameBox.Text != shadowTag.name)
                {
                    TagNameBox.Text = shadowTag.name;
                }
                ForegroundColorPicker.SelectedColor = shadowTag.foreground.Color;
                BackgroundColorPicker.SelectedColor = shadowTag.background.Color;
                ForegroundColorPicker.IsEnabled = false;
                BackgroundColorPicker.IsEnabled = false;
            }
            else
            {
                ForegroundColorPicker.IsEnabled = true;
                BackgroundColorPicker.IsEnabled = true;
            }
        }
        /// <summary>
        /// 标签名称选择响应
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="AutoSuggestBoxSuggestionChosenEventArgs"/> instance containing the event data.</param>
        private void TagNameBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var shadowTag = args.SelectedItem as ShadowTag;
            sender.Text = shadowTag.name;
            TagIdBox.Text = shadowTag.tag;
            ForegroundColorPicker.SelectedColor = shadowTag.foreground.Color;
            BackgroundColorPicker.SelectedColor = shadowTag.background.Color;
        }
        /// <summary>
        /// 标签名称变化
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="AutoSuggestBoxTextChangedEventArgs"/> instance containing the event data.</param>
        private void TagNameBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var res = TagsHelper.ShadowTags.FindAll(x => x.name.Contains(sender.Text));
                sender.ItemsSource = res;
            }
            
        }
        
    }
}
