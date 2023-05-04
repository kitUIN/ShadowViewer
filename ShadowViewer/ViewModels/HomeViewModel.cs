namespace ShadowViewer.ViewModels
{
    public class HomeViewModel: ObservableRecipient, IRecipient<FilesMessage>
    {
        public ShadowPath Path { get; set; }
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        public HomeViewModel()
        {
            IsActive = true;
        }
        public void Navigate(object parameter)
        {
            Path = new ShadowPath(parameter is string ? parameter as string : "local");
            RefreshLocalComic();
        }
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            foreach(LocalComic item in ComicDB.Get("Parent", Path.paths.Last()))
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
