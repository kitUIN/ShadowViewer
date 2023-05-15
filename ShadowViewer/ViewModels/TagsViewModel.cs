namespace ShadowViewer.ViewModels
{
    public class TagsViewModel
    {
        public LocalComic Comic { get; set; }
        public ObservableCollection<TokenItem> Tags = new ObservableCollection<TokenItem>();

        public TagsViewModel(LocalComic comic)
        {
            Comic = comic;
            LoadTags();
        }
        public void LoadTags()
        {
            Tags.Clear();
            if (TagsHelper.Affiliations[Comic.Affiliation] is ShadowTag shadow)
            {
                Tags.Add(new TokenItem
                {
                    Content = shadow.name,
                    Foreground = shadow.foreground,
                    Background = shadow.background,
                    IsRemoveable = false
                });
            }
            foreach (var item in Comic.Tags)
            {
                if (TagsHelper.ShadowTags.FirstOrDefault(x => x.name == item) is ShadowTag shadowTag)
                {
                    TokenItem tokenItem = new TokenItem
                    {
                        Content = shadowTag.name,
                        Foreground = shadowTag.foreground,
                        Background = shadowTag.background,
                        IsRemoveable = true,
                        Tag = shadowTag.name,
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
        public void AddNewTag(ShadowTag tag)
        {
            TagDB.Add(tag);
            TagsHelper.ShadowTags.Add(tag);
            Comic.Tags.Add(tag.name);
            LoadTags();
        }
    }
}
