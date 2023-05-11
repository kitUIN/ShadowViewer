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
            dialog.IsPrimaryButtonEnabled = false;
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
        public static InfoBar CreateSimpleInfoBar(InfoBarSeverity severity, string title, string message)
        {
            return new InfoBar()
            {
                IsIconVisible = true,
                IsOpen = true,
                IsClosable = true,
                Title  = title,
                Message = message,
                Severity = severity,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment= HorizontalAlignment.Right,
            };
        }
    }
}
