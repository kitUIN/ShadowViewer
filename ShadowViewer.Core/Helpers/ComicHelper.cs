namespace ShadowViewer.Helpers
{
    public static class ComicHelper
    {
        
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
            return new LocalComic(name, author, "", parent, "", time, time, name, "Local", "", img, 0, true);
        }
         
        
        /// <summary>
        /// 从文件夹导入漫画
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="parent">The parent.</param>
        public static async Task  ImportComicsAsync(StorageFolder folder, string parent)
        { 
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
            ComicDB.Add(new LocalComic(folder.DisplayName, "","", parent, "0%", time, time, folder.Path, "Local", "", img, (long)size, false));

        }
    }
}
