namespace ShadowViewer.ViewModels
{
    internal partial class StatusViewModel: ObservableRecipient, IRecipient<StatusMessage>
    {
        public LocalComic Comic { get; set; }
        public ObservableCollection<TokenItem> Tags = new ObservableCollection<TokenItem>();
        public StatusViewModel(LocalComic comic,bool isTag)
        {
            Comic = comic;
            LoadTags(isTag);
        }
        public string GetComicType
        {
            get => I18nHelper.GetString(Comic.IsFolder ? "FolderStatus.Title" : "FileStatus.Title");
        }
        public string GetShadowPath
        {
            get => ComicHelper.GetPath(Comic.Name, Comic.Parent);
        }
        public void LoadTags(bool isTag)
        {
            Tags.Clear();
            foreach (var item in Comic.Tags)
            {
                if (TagsHelper.ShadowTags.FirstOrDefault(x => x.tag == item) is ShadowTag shadowTag)
                {
                    if (!Tags.Any(x => (string)x.Content == shadowTag.name))
                    {
                        Tags.Add(new TokenItem
                        {
                            Content = shadowTag.name,
                            Foreground = shadowTag.foreground,
                            Background = shadowTag.background,
                            IsRemoveable = false,
                        });
                    }
                }
            }
            foreach (var item in Comic.AnotherTags)
            {
                if (TagsHelper.ShadowTags.FirstOrDefault(x => x.tag == item) is ShadowTag shadowTag)
                {
                    if (!Tags.Any(x => (string)x.Content == shadowTag.name))
                    {
                        TokenItem tokenItem = new TokenItem
                        {
                            Content = shadowTag.name,
                            Foreground = shadowTag.foreground,
                            Background = shadowTag.background,
                            IsRemoveable = isTag,
                            Tag = shadowTag.tag,
                        };
                        tokenItem.Removing += ShowTagItem_Removing;
                        Tags.Add(tokenItem);
                    }
                }

            }
        }
        private void ShowTagItem_Removing(object sender, TokenItemRemovingEventArgs e)
        {
            var item = e.TokenItem;
            Comic.AnotherTags.Remove(item.Tag.ToString());
        }
        public void Receive(StatusMessage message)
        {
            Log.Information("hello{222}", message.objects[0]);
        }
    }
}
