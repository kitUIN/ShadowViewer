namespace ShadowViewer.ViewModels
{
    internal partial class StatusViewModel: ObservableRecipient, IRecipient<StatusMessage>
    {
        public LocalComic Comic { get; set; }
        public ObservableCollection<TokenItem> Tags = new ObservableCollection<TokenItem>();
        public StatusViewModel(LocalComic comic)
        {
            Comic = comic;
            LoadTags();
        }
        public string GetComicType
        {
            get => I18nHelper.GetString(Comic.IsFolder ? "FolderStatus.Title" : "FileStatus.Title");
        }
        public string GetShadowPath
        {
            get => LocalComic.GetPath(Comic.Name, Comic.Parent);
        }
        private void LoadTags()
        {
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
        }

        public void Receive(StatusMessage message)
        {
            Log.Information("hello{222}", message.objects[0]);
        }
    }
}
