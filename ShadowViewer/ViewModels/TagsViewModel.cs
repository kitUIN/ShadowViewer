﻿namespace ShadowViewer.ViewModels
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
                        IsRemoveable = true,
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
            var tag = item.Tag.ToString();
            Comic.Tags.Remove(tag);
            DBHelper.Remove(new ShadowTag() { Name = tag });
            TagsHelper.ShadowTags.RemoveAll(x => x.Name == tag);
        }
        public void AddNewTag(ShadowTag tag)
        {
            tag.Add();
            TagsHelper.ShadowTags.Add(tag);
            Comic.Tags.Add(tag.Name);
            LoadTags();
        }
    }
}