namespace ShadowViewer.Models
{
    public partial class LocalComic: ObservableObject
    {
        /// <summary>
        /// ID
        /// </summary>
        [ObservableProperty]
        private string id;
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
        /// 汉化组
        /// </summary>
        [ObservableProperty]
        private string group;
        /// <summary>
        /// 描述
        /// </summary>
        [ObservableProperty]
        private string description;
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
        /// 类型
        /// </summary>
        [ObservableProperty]
        private string affiliation;
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
        public LocalComic(string id, string name, string createTime, 
            string lastReadTime, string link,string description="",string group = "", string author="", string parent = "local",
            string percent="0%", string tags = "" , string affiliation = "Local", string img="", long size=0, bool isFolder=false)
        {
            this.id = id;
            this.name = name;
            this.author = author;
            this.group  = group;
            this.img = img;
            this.percent = percent;
            this.description = description;
            this.createTime = createTime;
            this.lastReadTime = lastReadTime;
            this.parent = parent;
            Tags = LoadTags(tags);
            this.affiliation = affiliation;
            this.link = link;
            this.size = size;
            this.sizeString = ShowSize(size);
            this.isFolder = isFolder;
            Tags.CollectionChanged += Tags_CollectionChanged;
            
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ComicDB.Update(nameof(Tags), nameof(Id), Tags.JoinToString(), Id);
        }
         
        partial void OnSizeChanged(long value)
        {
            SizeString = ShowSize(value);
        }
        partial void OnImgChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Img), nameof(Id), newValue, Id);
            }
        }
        partial void OnAuthorChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Author), nameof(Id), newValue, Id);
            }
        }
        partial void OnGroupChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Group), nameof(Id), newValue, Id);
            }
        }
        partial void OnNameChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Name), nameof(Id), newValue, Id); 
            }
        }
        partial void OnLinkChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Link), nameof(Id), newValue, Id);
            }
        }
        partial void OnParentChanged(string oldValue, string newValue)
        {
            if(oldValue != newValue && newValue != Name)
            {
                ComicDB.Update(nameof(Parent), nameof(Id), newValue, Id);
            }
        }
         
        private static ObservableCollection<string> LoadTags(string tags)
        {
            var res = new HashSet<string>();
            foreach (var tag in tags.Split(","))
            {
                if (tag != "")
                {
                    res.Add(tag);
                }
            }
            return new ObservableCollection<string>(res);
        }
         
        private string ShowSize(long size)
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
        
        public string Path 
        { 
            get
            {
                if (Parent == "local")
                {
                    return "shadow://local/" + Id;
                }
                else
                {
                    return "shadow://local/" + Parent + "/" + Id;
                }
            } 
        }
        

    }
}
