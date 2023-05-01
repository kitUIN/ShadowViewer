namespace ShadowViewer.Controls
{
    public sealed partial class StatusBlock : UserControl
    {
        DependencyProperty TextProperty = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(StatusBlock),
                new PropertyMetadata(null, new PropertyChangedCallback(OnTextSoureChanged)));

        DependencyProperty TitleProperty = DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(StatusBlock),
                new PropertyMetadata(null, new PropertyChangedCallback(OnTitleSoureChanged)));
        public StatusBlock()
        {
            this.InitializeComponent();
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        private static void OnTextSoureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusBlock control = (StatusBlock)d;
            control.Text = (string)e.NewValue;
        }
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        private static void OnTitleSoureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusBlock control = (StatusBlock)d;
            control.Title = (string)e.NewValue;
        }
    }
}
