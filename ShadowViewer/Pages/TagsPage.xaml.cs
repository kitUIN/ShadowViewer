namespace ShadowViewer.Pages
{
    public sealed partial class TagsPage : Page
    {
        public TagsViewModel ViewModel { get; set; }
        public TagsPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is LocalComic comic)
            {
                ViewModel = new TagsViewModel(comic);
            }
        }

        private async void AddTag_Click(object sender, RoutedEventArgs e)
        {
            if(TagName.Text == "")
            {
                await XamlHelper.CreateMessageDialog(this.XamlRoot, I18nHelper.GetString("Shadow.Error.Title"),
                    I18nHelper.GetString("Shadow.Error.Message.NotEmpty")).ShowAsync();
                return;
            }
            if (TagsHelper.ShadowTags.Any(x => x.Name == TagName.Text))
            {
                ViewModel.AddNewTag(TagsHelper.ShadowTags.First(x => x.Name == TagName.Text));
                await XamlHelper.CreateMessageDialog(this.XamlRoot, I18nHelper.GetString("Shadow.Error.Title"),
                    I18nHelper.GetString("Shadow.Error.Message.TagExists")).ShowAsync();
                return;
            }
            ShadowTag tag = new ShadowTag(TagName.Text, ForegroundColorPicker.SelectedColor, BackgroundColorPicker.SelectedColor);
            ViewModel.AddNewTag(tag);
        }
    }
}
