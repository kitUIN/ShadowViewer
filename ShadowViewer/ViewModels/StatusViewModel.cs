using ShadowViewer.Models;

namespace ShadowViewer.ViewModels
{
    internal partial class StatusViewModel: ObservableRecipient, IRecipient<StatusMessage>
    {

        public LocalComic Comic { get; set; }
        private Frame frame;
        public ObservableCollection<TokenItem> Tags = new ObservableCollection<TokenItem>();
        public void Navigate(LocalComic comic, Frame frame)
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
            if (TagsHelper.ShadowTags.FirstOrDefault(x => x.tag == Comic.Affiliation) is ShadowTag shadow)
            {
                Tags.Add(new TokenItem
                {
                    Content = shadow.name,
                    Foreground = shadow.foreground,
                    Background = shadow.background,
                    IsRemoveable = false,
                    Tag = shadow.tag,
                });
            }
            foreach (var item in Comic.Tags)
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
                            IsRemoveable = false,
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
