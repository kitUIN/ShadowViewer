using ShadowViewer.Models;

namespace ShadowViewer.ViewModels
{
    public class HomeViewModel: ObservableRecipient, IRecipient<FilesMessage>
    {
        public ShadowPath path;
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        public HomeViewModel(ShadowPath path)
        {
            this.path = path;
            IsActive = true;
            RefreshLocalComic();
        }
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            foreach(LocalComic item in DBHelper.GetFrom("Parent", path.paths.Last()))
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
