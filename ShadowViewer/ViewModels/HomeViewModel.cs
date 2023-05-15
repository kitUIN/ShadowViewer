namespace ShadowViewer.ViewModels
{
    public partial class HomeViewModel: ObservableRecipient, IRecipient<FilesMessage>
    { 
        public LocalComic ConnectComic { get; set; }
        public string Path { get; private set; } = "local";
        public Uri OriginPath { get; private set; }
        public ShadowSorts Sorts { get; set; } = ShadowSorts.RZ;
        public ObservableCollection<LocalComic> LocalComics { get; } = new ObservableCollection<LocalComic>();
        public HomeViewModel(Uri parameter)
        {
            IsActive = true;
            LocalComics.CollectionChanged += LocalComics_CollectionChanged;
            OriginPath = parameter;
            Path = parameter.AbsolutePath.Split(new char[] { '/', }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? parameter.Host;
            Log.ForContext<HomePage>().Information("导航到{path},Path={p}", OriginPath, Path);
            RefreshLocalComic();
        }
        /// <summary>
        /// 从文件夹导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        public async Task ImportComicsAsync(StorageFolder folder,   string id = null)
        {
            var file = await ShadowFile.Create(folder);
            string img = null;
            if (file.Depth > 2)
            {
                while (file.Depth > 2)
                {
                    file = file.Children.FirstOrDefault(x => x.Self is StorageFolder);
                }
            }
            img = file.Children.FirstOrDefault(x => x.Self is StorageFile f && f.IsPic())?.Self.Path ?? "";
            LocalComics.Add(ComicHelper.CreateComic(((StorageFolder)file.Self).DisplayName, img, Path, file.Self.Path, id: id, size: file.Size));
        }
        /// <summary>
        /// 导入前先解压
        /// </summary>
        /// <param name="storageFile">The storage file.</param>
        /// <returns></returns>
        public async Task<Tuple<StorageFolder, string>> ImportZipCompress(StorageFile storageFile)
        {
            string id = Guid.NewGuid().ToString("N");
            while (ComicDB.Contains(nameof(id), id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            string path = System.IO.Path.Combine(App.Config.ComicsPath, id, storageFile.DisplayName);
            var folder = await path.ToStorageFolder();
            await Task.Run(() => {  CompressHelper.DeCompress(storageFile.Path, path); });
            return new Tuple<StorageFolder, string>(folder, id);
        }
        private void LocalComics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(LocalComic item in e.OldItems) 
                {
                    item.RemoveInDB();
                }
            }
            else if(e.Action== NotifyCollectionChangedAction.Add)
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
         
        public void RefreshLocalComic()
        {
            LocalComics.Clear();
            var comics = ComicDB.Get("Parent", Path);
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
