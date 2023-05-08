namespace ShadowViewer.Helpers
{
    public static class ComicHelper
    {
        public static string[] pngs = { ".png", ".jpg", ".jpeg", ".bmp" };

        public static string GetPath(string name, string parent)
        {
            List<string> strings = new List<string>();
            while (parent != "local")
            {
                if (ComicDB.GetFirst("Name", parent) is LocalComic local)
                {
                    strings.Add(parent);
                    parent = local.Parent;
                }
                else
                {
                    break;
                }
            }
            strings.Add("local");
            strings.Reverse();
            return "shadow://" + string.Join("/", strings) + "/" + name;
        }
        public static LocalComic CreateFolder(string name, string author, string img, string parent)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return new LocalComic(name, author, parent, "", time, time, name, "Local", "", img, 0, true);
        }
        /// <summary>
        /// 检测一层
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns>Counts=0为无下级目录,null为无图片,Counts>0为有下级目录</returns>
        public static List<StorageFolder> CheckEffectiveFolderAsync(List<IStorageItem> items)
        {
            
            List<StorageFolder> folders = new List<StorageFolder>();
            bool anyPng = false;
            foreach (var item in items)
            {
                if (item is StorageFile file && pngs.Contains(file.FileType))
                {
                    anyPng = true;
                }
                else if (item is StorageFolder folder1)
                { 
                    folders.Add(folder1);
                }
            }
            if (!anyPng)
            {
                return null;
            }
            return folders;
        }
        public static async Task  ImportComicsAsync(StorageFolder folder, string parent)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            List<IStorageItem> item = (await folder.GetItemsAsync()).ToList();
            var first = CheckEffectiveFolderAsync(item);
            item.Sort((x, y) => x.Name.CompareTo(y.Name));
            var imgItem = item.FirstOrDefault(x => x is StorageFile file && pngs.Contains(file.FileType));
            string img = imgItem is null ? "" : imgItem.Path;
            // 多层漫画无封面,从里面取
            if (imgItem is null && first.Count > 0)
            {
                StorageFolder imgFolder = item.FirstOrDefault(x => x is StorageFolder) as StorageFolder;
                List<IStorageItem> item2 = (await imgFolder.GetItemsAsync()).ToList();
                item2.Sort((x, y) => x.Name.CompareTo(y.Name));
                var imgItem2 = item.FirstOrDefault(x => x is StorageFile file && pngs.Contains(file.FileType));
                img = imgItem2 is null ? "" : imgItem2.Path;
            }
            ComicDB.Add(new LocalComic(folder.DisplayName, "", parent, "0%", time, time, folder.Path, "Local", "", img, 0, false));
            
        }
    }
}
