namespace ShadowViewer.ViewModels
{
    internal partial class StatusViewModel: ObservableRecipient, IRecipient<StatusMessage>
    {
        public LocalComic Comic { get; set; }
        private Frame frame;
        [ObservableProperty]
        private bool isTag;
        public ObservableCollection<TokenItem> Tags = new ObservableCollection<TokenItem>();
        public ObservableCollection<string> ExistID = new ObservableCollection<string>();
        public StatusViewModel(LocalComic comic, bool isTag, Frame frame)
        {
            Comic = comic;
            IsActive = true;
            IsTag = isTag;
            this.frame = frame;
            LoadTags();
        }
        public string GetComicType
        {
            get => I18nHelper.GetString(Comic.IsFolder ? "Shadow.FileType.ComicFolder" : "Shadow.FileType.Comic");
        }
        public string GetShadowPath
        {
            get => ComicHelper.GetPath(Comic.Name, Comic.Parent);
        }
        public void LoadTags()
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
                            IsRemoveable = IsTag,
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
        public void Reload()
        {
            Reload(Comic);
        }
        public void Reload(LocalComic comic)
        {
            frame.Navigate(typeof(StatusPage), new List<object> { comic, IsTag });
        }
        public void Receive(StatusMessage message)
        {
            if (message.objects.Length >= 1 && message.objects[0] is string method)
            {
                if (method == "Reload")
                {
                    Reload(); 
                }
                else if (method == "ReloadDB")
                {
                    Reload(ComicDB.GetFirst(nameof(Comic.Name), Comic.Name));
                }
            }
        }
    }
}
