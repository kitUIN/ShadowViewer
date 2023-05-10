using ShadowViewer.DataBases;

namespace ShadowViewer.ViewModels
{
    public partial class HomeViewModel: ObservableRecipient, IRecipient<FilesMessage>
    { 
        public LocalComic ConnectComic { get; set; }
        public string Path { get; private set; } = "local";
         
        public string OriginPath { get; private set; } = "shadow://local";
 
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        public HomeViewModel()
        {
            IsActive = true;
            LocalComics.CollectionChanged += LocalComics_CollectionChanged;
        }

        private void LocalComics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(LocalComic item in e.OldItems) 
                {
                    item.RemoveInDB();
                }
            }else if(e.Action== NotifyCollectionChangedAction.Add)
            {
                foreach (LocalComic item in e.NewItems)
                {
                    if (!ComicDB.Contains("id", item.Id))
                    {
                        ComicDB.Add(item);
                    }
                    
                }
            }
        }

        public void Navigate(Uri parameter)
        {
            OriginPath = parameter.AbsoluteUri;
            List<string> urls = parameter.AbsolutePath.Split('/').ToList();
            urls.RemoveAll(x => x == "");
            Path = urls.Count > 0 ? urls.Last() : parameter.Host;
            Log.ForContext<HomePage>().Information("导航到{path}", OriginPath);
            RefreshLocalComic();
        }
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            foreach (LocalComic item in ComicDB.Get("Parent", Path))
            {
                LocalComics.Add(item);
                if(ConnectComic is LocalComic && item.Id== ConnectComic.Id)
                {
                    ConnectComic = item;
                }
            } 
        }

        public void Receive(FilesMessage message)
        {
            if (message.objects.Length >= 1 && message.objects[0] is string method)
            {
                if(method == "Reload")
                {
                    RefreshLocalComic();
                }
            } 
        }
    }
}
