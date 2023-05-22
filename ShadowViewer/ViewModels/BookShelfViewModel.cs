namespace ShadowViewer.ViewModels
{
    public partial class BookShelfViewModel: ObservableRecipient, IRecipient<FilesMessage>
    { 
        public LocalComic ConnectComic { get; set; }
        public string Path { get; private set; } = "local";
        public Uri OriginPath { get; private set; }
        public ShadowSorts Sorts { get; set; } = ShadowSorts.RZ;
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        private static ILogger Logger { get; } = Log.ForContext<BookShelfPage>();
        public BookShelfViewModel(Uri parameter)
        {
            IsActive = true;
            LocalComics.CollectionChanged += LocalComics_CollectionChanged;
            OriginPath = parameter;
            Path = parameter.AbsolutePath.Split(new char[] { '/', }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? parameter.Host;
            Logger.Information("导航到{path},Path={p}", OriginPath, Path);
            RefreshLocalComic();
        }
        /// <summary>
        /// 从文件夹导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        public async Task ImportComicsAsync(StorageFolder folder, string id = null)
        {
            ShadowFile file = await ShadowFile.Create(folder);
            List<ShadowFile> two = ShadowFile.GetDepthFiles(file, 2);
            ShadowFile img = null;
            if (two == null || two.Count == 0)
            {
                two = ShadowFile.GetDepthFiles(file, 1);
            } 
            foreach (ShadowFile file2 in two)
            {
                img = file2.Children.FirstOrDefault(x => x.Self is StorageFile f && f.IsPic());
                if (img != null) break;
            }
            LocalComic comic = ComicHelper.CreateComic(((StorageFolder)file.Self).DisplayName, img?.Self.Path ?? @"ms-appx:///Assets/Default/picbroken.png", Path, file.Self.Path, id: id, size: file.Size);
            LocalComics.Add(comic);
            ShadowFile.InitLocal(file, comic.Id);
            file.Dispose();
        } 
        private void LocalComics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(LocalComic item in e.OldItems) 
                {
                    DBHelper.Remove(item);
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
