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
        public static LocalComic CreateComic(string name, string img, string parent, string link, string affiliation = "Local",long size=0)
        {
            string id = Guid.NewGuid().ToString("N");
            while (ComicDB.Contains(nameof(id), id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var comic = new LocalComic(id, name, time, time, link, img: img, size: size,
                affiliation: affiliation, parent: parent, isFolder: true);
            ComicDB.Add(comic);
            return comic;
        }

        /// <summary>
        /// 从文件夹导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        public static async Task  ImportComicsAsync(StorageFolder folder, string parent)
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
                foreach(var item in first)
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
             CreateComic(folder.DisplayName, img, parent, folder.Path,size: (long)size);
        }
    }
}
