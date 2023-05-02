
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
            if (e.Parameter is LocalComic comic)
            {
                viewModel = new StatusViewModel(comic);
                
            }
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
                MessageHelper.SendFilesReload();
                WindowHelper.GetWindowForElement(this).Title  = viewModel.Comic.Name;
            }
        }
    }
}
