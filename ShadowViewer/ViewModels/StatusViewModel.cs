namespace ShadowViewer.ViewModels
{
    public partial class StatusViewModel: ObservableRecipient, IRecipient<StatusMessage>
    {

        public LocalComic Comic { get; set; }
        private Frame frame;
        public ObservableCollection<TokenItem> Tags = new ObservableCollection<TokenItem>();
        public StatusViewModel(LocalComic comic, Frame frame)
        {
            Comic = comic;
            IsActive = true;
            this.frame = frame;
            LoadTags();
        }
        public string GetComicType
        {
            get => I18nHelper.GetString(Comic.IsFolder ? "Shadow.FileType.ComicFolder" : "Shadow.FileType.Comic");
        } 
        public void LoadTags()
        {
            Tags.Clear();
            if (TagsHelper.Affiliations[Comic.Affiliation] is ShadowTag shadow)
            {
                Tags.Add(new TokenItem
                {
                    Content = shadow.Name,
                    Foreground = shadow.Foreground,
                    Background = shadow.Background,
                    IsRemoveable = false
                });
            }
            foreach (var item in Comic.Tags)
            {
                if (TagsHelper.ShadowTags.FirstOrDefault(x => x.Name == item) is ShadowTag shadowTag)
                {
                    TokenItem tokenItem = new TokenItem
                    {
                        Content = shadowTag.Name,
                        Foreground = shadowTag.Foreground,
                        Background = shadowTag.Background,
                        IsRemoveable = false,
                        Tag = shadowTag.Name,
                    };
                    tokenItem.Removing += ShowTagItem_Removing;
                    Tags.Add(tokenItem);
                }
            }
        }
        private void ShowTagItem_Removing(object sender, TokenItemRemovingEventArgs e)
        {
            var item = e.TokenItem;
            Comic.Tags.Remove(item.Tag.ToString());
        }
        public void Reload()
        {
            Reload(Comic);
        }
        public void Reload(LocalComic comic)
        {
            frame.Navigate(typeof(StatusPage), comic);
        }
        public void Receive(StatusMessage message)
        {
           
        }
    }
}
