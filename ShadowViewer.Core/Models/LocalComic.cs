﻿using ShadowViewer.DataBases;
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
            Tags = tags;
            AnotherTags = anotherTags;
            this.link = link;
            this.size = size;
            this.sizeString = ShowSize(size);
            this.isFolder = isFolder;
            Tags.CollectionChanged += Tags_CollectionChanged;
            AnotherTags.CollectionChanged += AnotherTags_CollectionChanged;
        }

        private void AnotherTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ComicDB.Update(nameof(AnotherTags), nameof(Name), AnotherTags.JoinToString(), Name);
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ComicDB.Update(nameof(Tags), nameof(Name), Tags.JoinToString(), Name);
        }

        public LocalComic(string name, string author, string parent, string percent, string createTime, string lastReadTime, string link, string tags, string anotherTags, string img, long size,bool isFolder):
            this(name,author, parent, percent, createTime, lastReadTime, link, LoadTags(tags), LoadTags(anotherTags), img, size, isFolder)
        {
        }
        partial void OnSizeChanged(long value)
        {
            SizeString = ShowSize(value);
        }
        partial void OnImgChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Img), nameof(Name), newValue, Name);
            }
        }
        partial void OnNameChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                
                ComicDB.Update(nameof(Name), nameof(Name), newValue, oldValue);
                Parent = Name;
                if (IsFolder)
                {
                    Link = Name;
                }
            }
        }
        partial void OnLinkChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                ComicDB.Update(nameof(Link), nameof(Name), newValue, Name);
            }
        }
        partial void OnParentChanged(string oldValue, string newValue)
        {
            if(oldValue != newValue)
            {
                ComicDB.Update(nameof(Parent), nameof(Name), newValue, Name);
            }
        }
         
        private static ObservableCollection<string> LoadTags(string tags)
        {
            var res = new HashSet<string>();
            foreach (var tag in tags.Split(","))
            {
                res.Add(tag);
            }
            return new ObservableCollection<string>(res);
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
        
        
        

    }
}
