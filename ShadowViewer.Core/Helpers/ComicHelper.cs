namespace ShadowViewer.Helpers
{
    public static class ComicHelper
    { 
        public static LocalComic CreateFolder(string name,string img, string parent)
        {
            string id = Guid.NewGuid().ToString("N");
            while (ComicDB.Contains(nameof(id), id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (img == "") { img = "ms-appx:///Assets/Default/folder.png"; }
            if (name == "") { name = id; }
            var comic =  new LocalComic(id, name, time, time, id, img: img, parent: parent, isFolder: true, percent:"");
            return comic;
        }
        public static LocalComic CreateComic(string name, string img, string parent, string link, string affiliation = "Local",long size=0,string id=null)
        {
            if (id == null)
            {
                id = Guid.NewGuid().ToString("N");
                while (ComicDB.Contains(nameof(id), id))
                {
                    id = Guid.NewGuid().ToString("N");
                }
            }
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var comic = new LocalComic(id, name, time, time, link, img: img, size: size,
                affiliation: affiliation, parent: parent);
            return comic;
        } 
        /// <summary>
        /// 从文件夹导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        public static async Task<LocalComic> ImportComicsAsync(StorageFolder folder, string parent, Action valueAction, Action CloseAction)
        {
            var first = await folder.GetFoldersAsync();
            List<StorageFile> oneFiles = (await folder.GetFilesAsync()).ToList();
            // 一层的漫画
            var img = FileHelper.GetImgInFiles(oneFiles);
            var size = await FileHelper.GetSizeInFiles(oneFiles);
            // 无封面情况,从内部取

            // 最多2层漫画
            if (first.Count > 0)
            {
                foreach (var item in first)
                {
                    List<StorageFile> twoFiles = (await item.GetFilesAsync()).ToList();
                    if (img is null)
                    {
                        img = FileHelper.GetImgInFiles(twoFiles);
                    }
                    size += await FileHelper.GetSizeInFiles(twoFiles);
                }
            }
            if (img is null)
            {
                img = "";
            }
            return CreateComic(folder.DisplayName, img, parent, folder.Path, size: (long)size);
        }
        /// <summary>
        /// 字母顺序A-Z
        /// </summary>
        public static int AZSort(LocalComic x, LocalComic y) => x.Name.CompareTo(y.Name);
        /// <summary>
        /// 字母顺序Z-A
        /// </summary>
        public static int ZASort(LocalComic x, LocalComic y) => y.Name.CompareTo(x.Name);
        /// <summary>
        /// 阅读时间早-晚
        /// </summary>
        public static int RASort(LocalComic x, LocalComic y) => x.LastReadTime.CompareTo(y.LastReadTime);
        /// <summary>
        /// 阅读时间晚-早(默认)
        /// </summary>
        public static int RZSort(LocalComic x, LocalComic y) => y.LastReadTime.CompareTo(x.LastReadTime);
        /// <summary>
        /// 创建时间早-晚
        /// </summary>
        public static int CASort(LocalComic x, LocalComic y) => x.CreateTime.CompareTo(y.CreateTime);
        /// <summary>
        /// 创建时间晚-早
        /// </summary>
        public static int CZSort(LocalComic x, LocalComic y) => y.CreateTime.CompareTo(x.CreateTime);
        /// <summary>
        /// 阅读进度小-大
        /// </summary>
        public static int PASort(LocalComic x, LocalComic y) => x.Percent.CompareTo(y.Percent);
        /// <summary>
        /// 阅读进度大-小
        /// </summary>
        public static int PZSort(LocalComic x, LocalComic y) => y.Percent.CompareTo(x.Percent);
        
    }
}
