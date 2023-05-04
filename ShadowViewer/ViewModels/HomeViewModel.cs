namespace ShadowViewer.ViewModels
{
    public partial class HomeViewModel: ObservableRecipient, IRecipient<FilesMessage>
    {
        public string Path { get; private set; } = "local";
        [ObservableProperty]
        private List<string> paths = new List<string> { "local" };
        public string OriginPath { get; private set; } = "shadow://local";
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        public HomeViewModel()
        {
            IsActive = true;
        }
        public void Navigate(object parameter)
        {
            if(parameter is string path)
            {
                OriginPath = path;
                Paths = path.Replace("shadow://", "").Split("/").ToList();
                if(Paths.Count == 0)
                {
                    Paths.Add("local");
                }
                Path = Paths.Last();
            }
            Log.ForContext<HomePage>().Information("导航到{patj}", OriginPath);
            RefreshLocalComic();
        }
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            foreach(LocalComic item in ComicDB.Get("Parent", Path))
            {
                LocalComics.Add(item);
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
