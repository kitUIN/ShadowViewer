using ShadowViewer.Core.Enums;

namespace ShadowViewer.ViewModels
{
    public partial class BookShelfViewModel: ObservableRecipient, IRecipient<FilesMessage>
    {
        private bool isEmpty = true;
        private int folderTotalCounts;
        public LocalComic ConnectComic { get; set; }
        public string Path { get; private set; } = "local";
        public Uri OriginPath { get; private set; }
        public ShadowSorts Sorts { get; set; } = ShadowSorts.RZ;
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        private static ILogger Logger { get; } = Log.ForContext<BookShelfPage>();

        public bool IsEmpty
        {
            get => isEmpty;
            set => SetProperty(ref isEmpty, value, propertyName: nameof(IsEmpty));
        }
        public int FolderTotalCounts
        {
            get => folderTotalCounts;
            set => SetProperty(ref folderTotalCounts, value, propertyName: nameof(FolderTotalCounts));
        }
        public BookShelfViewModel(Uri parameter)
        {
            IsActive = true;
            LocalComics.CollectionChanged += LocalComics_CollectionChanged;
            OriginPath = parameter;
            Path = parameter.AbsolutePath.Split(new char[] { '/', }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? parameter.Host;
            Logger.Information("导航到{path},Path={p}", OriginPath, Path);
            RefreshLocalComic();
        }
        
        private void LocalComics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(LocalComic item in e.OldItems) 
                {
                    item.Remove();
                }
            }
            else if(e.Action== NotifyCollectionChangedAction.Add)
            {
                foreach (LocalComic item in e.NewItems)
                {
                    if (item.Id is null) continue;
                    if (!DBHelper.Db.Queryable<LocalComic>().Any(x => x.Id == item.Id ))
                    {
                        item.Add();
                    }
                }
            }
            IsEmpty = LocalComics.Count == 0;
            FolderTotalCounts = LocalComics.Count;
        }
         
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            var comics = DBHelper.Db.Queryable<LocalComic>().Where(x => x.Parent == Path).ToList();
            switch (Sorts)
            {
                case ShadowSorts.AZ:
                    comics.Sort(ComicHelper.AZSort); break;
                case ShadowSorts.ZA:
                    comics.Sort(ComicHelper.ZASort); break;
                case ShadowSorts.CA:
                    comics.Sort(ComicHelper.CASort); break;
                case ShadowSorts.CZ:
                    comics.Sort(ComicHelper.CZSort); break;
                case ShadowSorts.RA:
                    comics.Sort(ComicHelper.RASort); break;
                case ShadowSorts.RZ:
                    comics.Sort(ComicHelper.RZSort); break;
                case ShadowSorts.PA:
                    comics.Sort(ComicHelper.PASort); break;
                case ShadowSorts.PZ:
                    comics.Sort(ComicHelper.PZSort); break;
            } 
            foreach (LocalComic item in comics)
            {
                LocalComics.Add(item);
                if(ConnectComic is LocalComic && item.Id == ConnectComic.Id)
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
