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
        }
        public void Navigate(Uri parameter)
        {
            OriginPath = parameter.AbsoluteUri;
            List<string> urls = parameter.AbsolutePath.Split('/').ToList();
            Path = urls.Count > 0? urls.Last() :parameter.Host;
            Log.ForContext<HomePage>().Information("导航到{patj}", OriginPath);
            RefreshLocalComic();
        }
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            foreach(LocalComic item in ComicDB.Get("Parent", Path))
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
