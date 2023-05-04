


namespace ShadowViewer.Pages
{
    public sealed partial class StatusPage : Page
    {
        private StatusViewModel viewModel;
        private bool isTag = false;
        public StatusPage()
        {
            this.InitializeComponent();
            this.RequestedTheme = ThemeHelper.RootTheme;
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            if (e.Parameter is List<object> args)
            {
                if(args.Count >= 2 && args[1] is bool flag)
                {
                    isTag = flag;
                }
                if (args.Count >= 1 && args[0] is LocalComic comic)
                {
                    viewModel = new StatusViewModel(comic, isTag);
                }
            }
            Random random = new Random();
            int randomNumber = random.Next(0, 2000000);
            TagIdBox.Text = randomNumber.ToString();
            TagNameBox.Text = "Random";
        }
        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var file = await FileHelper.SelectFileAsync(sender as Button, ".png", ".jpg", ".jpeg", ".bmp");
            if (file != null && file.DecodePath() != viewModel.Comic.Img)
            {
                viewModel.Comic.Img = file.DecodePath();
                MessageHelper.SendFilesReload();
            }
        }

        private async void FileImg_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var box = sender as TextBox;
            Uri uri = null;
            bool flag = false;
            string title = "";
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
                            title = I18nHelper.GetString("Error.Title");
                            message = I18nHelper.GetString("Error.Message.NotImage");
                        }
                    }
                    else
                    {
                        flag = true;
                        title = I18nHelper.GetString("Error.Title");
                        message = I18nHelper.GetString("Error.Message.NoFile");
                    }
                }
                catch(UriFormatException)
                {
                    flag = true;
                    title = I18nHelper.GetString("Error.Title");
                    message = I18nHelper.GetString("Error.Message.UriFormatException");
                }
                catch(System.Runtime.InteropServices.COMException)
                {
                    flag = true;
                    title = I18nHelper.GetString("Error.Title");
                    message = I18nHelper.GetString("Error.Message.NoFile");
                }
                catch(System.IO.FileNotFoundException)
                {
                    flag = true;
                    title = I18nHelper.GetString("Error.Title");
                    message = I18nHelper.GetString("Error.Message.NoFile");
                }
                if(flag)
                {
                    box.Text = viewModel.Comic.Img;
                    await XamlHelper.CreateMessageDialog(this.XamlRoot, title,message).ShowAsync();
                    return;
                }
                viewModel.Comic.Img = box.Text;
                MessageHelper.SendFilesReload();
            }
        }

        private void FileName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (e.Key == VirtualKey.Enter)
            {
                viewModel.Comic.Name = box.Text;
                WindowHelper.GetWindowForElement(this).Title  = viewModel.Comic.Name;
                var comic = ComicDB.GetFirst("Name", box.Text);
                this.Frame.Navigate(typeof(StatusPage), new List<object> { comic, isTag });
                MessageHelper.SendFilesReload();
                WindowHelper.GetWindowForElement(this).Activate();
            }
        }
        public SolidColorBrush ToBrush(Color color)
        {
            return new SolidColorBrush(color);
        }
        private void AddNewTag_Click(object sender, RoutedEventArgs e)
        {
            if (TagIdBox.Text == "" || TagNameBox.Text =="") { return; }
            var tag = new ShadowTag(TagIdBox.Text, TagNameBox.Text,
                ForegroundColorPicker.SelectedColor, BackgroundColorPicker.SelectedColor);
            if (!TagsHelper.ShadowTags.Any(x => x.tag == tag.tag))
            {
                TagDB.Add(tag);
                TagsHelper.ShadowTags.Add(tag);
            }
            viewModel.Comic.AddAnotherTag(tag.tag);
            viewModel.Comic.UpdateAnotherTags();
            viewModel.LoadTags(isTag);
        }



         
        private void TagNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TagsHelper.ShadowTags.FirstOrDefault(x => x.name == TagNameBox.Text) is ShadowTag shadowTag)
            {
                TagIdBox.Text = shadowTag.tag;
                ForegroundColorPicker.SelectedColor = shadowTag.foreground.Color;
                BackgroundColorPicker.SelectedColor = shadowTag.background.Color;
            }
        }

        private void TagIdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Contains(","))
            {
                ((TextBox)sender).Undo();
            }
        }
         
    }
}
