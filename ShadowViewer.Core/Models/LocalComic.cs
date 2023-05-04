using ShadowViewer.DataBases;
using ShadowViewer.Helpers;

namespace ShadowViewer.Models
{
    public partial class LocalComic: ObservableObject
    {
        /// <summary>
        /// 名称
        /// </summary>
        [ObservableProperty]
        private string name;
        /// <summary>
        /// 作者
        /// </summary>
        [ObservableProperty]
        private string author;
        /// <summary>
        /// 缩略图地址
        /// </summary>
        [ObservableProperty]
        private string img;
        /// <summary>
        /// 阅读进度(0-100%)
        /// </summary>
        [ObservableProperty]
        private string percent;
        /// <summary>
        /// 创建时间
        /// </summary>
        [ObservableProperty]
        private string createTime;
        /// <summary>
        /// 最后阅读时间
        /// </summary>
        [ObservableProperty]
        private string lastReadTime;
        /// <summary>
        /// 父文件夹
        /// </summary>
        [ObservableProperty]
        private string parent;
        /// <summary>
        /// 标签
        /// </summary>
        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();
        /// <summary>
        /// 附加标签
        /// </summary>
        public ObservableCollection<string> AnotherTags { get; } = new ObservableCollection<string>();
        /// <summary>
        /// 链接对象
        /// </summary>
        [ObservableProperty]
        private string link;
        /// <summary>
        /// 文件大小
        /// </summary>
        [ObservableProperty]
        private long size;
        /// <summary>
        /// 文件大小(String)
        /// </summary>
        [ObservableProperty]
        private string sizeString;
        /// <summary>
        /// 是否是文件夹
        /// </summary>
        [ObservableProperty]
        private bool isFolder = false;
        public LocalComic(string name, string author, string parent, string percent, string createTime, string lastReadTime, string link, ObservableCollection<string> tags, ObservableCollection<string> anotherTags, string img, long size, bool isFolder)
        {
            this.name = name;
            this.author = author;
            this.img = img;
            this.percent = percent;
            this.createTime = createTime;
            this.lastReadTime = lastReadTime;
            this.parent = parent;
            this.Tags = tags;
            this.AnotherTags = anotherTags;
            this.link = link;
            this.size = size;
            this.sizeString = ShowSize(size);
            this.isFolder = isFolder;
        }
        public LocalComic(string name, string author, string parent, string percent, string createTime, string lastReadTime, string link, string tags, string anotherTags, string img, long size,bool isFolder)
        {
            this.name = name;
            this.author = author;
            this.img = img;
            this.percent = percent;
            this.createTime = createTime;
            this.lastReadTime = lastReadTime;
            this.parent = parent;
            LoadTags(tags);
            LoadAnotherTags(anotherTags);
            this.link = link;
            this.size = size;
            this.sizeString = ShowSize(size);
            this.isFolder = isFolder;
        }
        partial void OnSizeChanged(long value)
        {
            SizeString = ShowSize(value);
        }
        partial void OnImgChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update("Img", "Name", newValue, Name);
            }
        }
        partial void OnNameChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update("Link", "Name", newValue, oldValue);
                ComicDB.Update("Name", "Name", newValue, oldValue);
                ComicDB.Update("Parent", "Parent", newValue, oldValue);
            }
        }
        public void AddTag(string tag)
        {
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
            }
        }
        public void AddAnotherTag(string tag)
        {
            if (!AnotherTags.Contains(tag))
            {
                AnotherTags.Add(tag);
            }
        } 
        public void LoadTags(string tags)
        {
            foreach(var tag in tags.Split(","))
            {
                AddTag(tag);
            }
        }
        public void LoadAnotherTags(string tags)
        {
            foreach(var tag in tags.Split(","))
            {
                AddAnotherTag(tag);
            }
        }
        public string ShowSize(long size)
        {
            long KB = 1024;
            long MB = KB * 1024;
            long GB = MB * 1024;
            if (size / GB >= 1)
            {
                return $"{Math.Round(size / (float)GB, 2)} GB";
            }
            else if (size / MB >= 1)
            {
                return $"{Math.Round(size / (float)MB, 2)} MB";
            }
            else if (size / KB >= 1)
            {
                return $"{Math.Round(size / (float)KB, 2)} KB";
            }
            return $"{size} B";
        }
        public static LocalComic CreateFolder(string name, string author, string img, string parent)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return new LocalComic(name, author, parent, "", time, time, name, "Local", "", img, 0, true);
        }
        public static LocalComic ReadComicFromDB(SqliteDataReader reader)
        {
            return new LocalComic(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5),
                reader.GetString(6), reader.GetString(7), reader.GetString(8), reader.GetString(9),
                reader.GetInt64(10), reader.GetBoolean(11));
        }
        public static string GetPath(string name, string parent)
        {
            
            string path = "shadow://local/";
            
            if (parent == "")
            {
                return path + name;
            }
            List<string> strings = new List<string>();
            while (parent != "")
            {
                Log.Information(parent);
                if(ComicDB.GetFirst("Name", parent) is LocalComic local)
                {
                    strings.Add(local.Parent);
                }
            }
            strings.Reverse();
            return path + string.Join("/", strings) + name;
        }
        public void RemoveInDB()
        {
            ComicDB.Remove("name", Name);
        }
        public void UpdateAnotherTags()
        {
            ComicDB.Update("AnotherTags", "Name", AnotherTags.JoinToString(), Name);
        }
    }
}
