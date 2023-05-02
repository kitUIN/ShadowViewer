using Microsoft.UI.Text;

namespace ShadowViewer.Helpers
{
    public static class XamlHelper
    {
        public static FontIcon CreateFontIcon(string glyph)
        {
            return new FontIcon()
            {
                Glyph = glyph,
            };
        }
        public static ImageIcon CreateImageIcon(Uri uri)
        {
            return new ImageIcon()
            {
                Source = new BitmapImage(uri)
            };
        }
        public static ImageIcon CreateImageIcon(string uriString)
        {
            return CreateImageIcon(new Uri(uriString));
        }
        public static BitmapIcon CreateBitmapIcon(Uri uri)
        {
            return new BitmapIcon()
            {
                UriSource = uri,
            };
        }
        public static BitmapIcon CreateBitmapIcon(string uriString)
        {
            return CreateBitmapIcon(new Uri(uriString));
        }

        /// <summary>
        /// 创建一个横着的带Header的TextBox
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="placeholder">The placeholder.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        public static StackPanel CreateOneLineTextBox(string header, string placeholder,string text, int width)
        {
            StackPanel grid = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            TextBlock headerBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0),
                Text = header,
                FontSize = 16,
            };
            TextBox txt = new TextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = width,
                Text = text,
                PlaceholderText = placeholder,
            };
            grid.Children.Add(headerBlock);
            grid.Children.Add(txt);
            return grid;
        }
        /// <summary>
        /// 新建文件夹对话框
        /// </summary>
        /// <returns></returns>
        public static ContentDialog CreateFolderDialog(XamlRoot xamlRoot, string parent)
        {
            ContentDialog dialog = CreateContentDialog(xamlRoot);
            dialog.Title = I18nHelper.GetString("Dialog/CreateFolder/Title");
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
            var imgBox = CreateOneLineTextBox(I18nHelper.GetString("Dialog/CreateFolder/Img"), "","", 163);
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
            var nameBox = CreateOneLineTextBox(I18nHelper.GetString("Dialog/CreateFolder/Name"),
                I18nHelper.GetString("Dialog/CreateFolder/Title"),"", 222);
            grid.Children.Add(nameBox);
            grid.Children.Add(stackPanel);
            dialog.Content = grid;
            dialog.PrimaryButtonClick += (s, e) =>
            {
                var img = ((TextBox)imgBox.Children[1]).Text;
                var name = ((TextBox)nameBox.Children[1]).Text;
                DBHelper.AddComic(name, img, parent);
                MessageHelper.SendFilesReload();

            };
            return dialog;
        }
        
        /// <summary>
        /// 创建一个基础的ContentDialog
        /// </summary>
        /// <param name="xamlRoot">The xaml root.</param>
        /// <returns></returns>
        public static ContentDialog CreateContentDialog(XamlRoot xamlRoot)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = xamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.DefaultButton = ContentDialogButton.Primary;
            return dialog;
        }
        public static ContentDialog CreateMessageDialog(XamlRoot xamlRoot,string title,string message)
        {
            ContentDialog dialog = CreateContentDialog(xamlRoot);
            dialog.Title = title;
            dialog.Content = message;
            dialog.PrimaryButtonText = I18nHelper.GetString("Dialog/ConfirmButton");
            dialog.CloseButtonText = I18nHelper.GetString("Dialog/CloseButton");
            return dialog;
        }
        public static StackPanel CreateOneLineTextBlock(string title, string value)
        {
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            TextBlock headerBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 100,
                Text = title,
            };
            TextBlock text = new TextBlock
            {
                IsTextSelectionEnabled = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Text = value,
            };
            panel.Children.Add(headerBlock);
            panel.Children.Add(text);
            return panel;
        }
        /// <summary>
        /// 添加标签对话框
        /// </summary>
        /// <param name="xamlRoot">The xaml root.</param>
        /// <param name="comic">The comic.</param>
        /// <returns></returns>
        public static ContentDialog CreateTagsDialog(XamlRoot xamlRoot, LocalComic comic)
        {
            ContentDialog dialog = CreateContentDialog(xamlRoot);
            dialog.Title = I18nHelper.GetString("ShadowCommandAddTag.Label");
            dialog.PrimaryButtonText = I18nHelper.GetString("Dialog/ConfirmButton");
            dialog.CloseButtonText = I18nHelper.GetString("Dialog/CloseButton");
            ScrollViewer scrollViewer = new ScrollViewer();
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            
             
            scrollViewer.Content = panel;
            dialog.Content = scrollViewer;
            return dialog;
        }

        /// <summary>
        /// 查看属性对话框
        /// </summary>
        /// <param name="xamlRoot">The xaml root.</param>
        /// <param name="comic">The comic.</param>
        /// <returns></returns>
        public static ContentDialog CreateStatusDialog(XamlRoot xamlRoot, LocalComic comic)
        {
            ContentDialog dialog = CreateContentDialog(xamlRoot);
            dialog.Title = I18nHelper.GetString("ShadowCommandStatusToolTip/Content");
            dialog.PrimaryButtonText = I18nHelper.GetString("Dialog/ConfirmButton");
            dialog.CloseButtonText = I18nHelper.GetString("Dialog/CloseButton");
            
            StackPanel grid = new StackPanel()
            {
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Vertical,
            };
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog/CreateFolder/Name"), comic.Name));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.Author"), comic.Author));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.Type"), I18nHelper.GetString(comic.IsFolder ? "Dialog.Status.Folder" : "Dialog.Status.File")));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.Path"), LocalComic.GetPath(comic.Name,comic.Parent)));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.CreateTime"), comic.CreateTime));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.LastReadTime"), comic.LastReadTime));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.Size"), comic.SizeString));
            grid.Children.Add(CreateOneLineTextBlock(I18nHelper.GetString("Dialog.Status.Link"), comic.Link));
             
            dialog.Content = grid;
            return dialog;
        }
        private static Border CreateTag(string title, Brush background)
        { 
            return new Border
            {
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(7,4,7,4),
                Background = background,
                Child = new TextBlock { Text = title, FontSize=13, FontWeight = FontWeights.Bold },
            };
        }
    }
}
