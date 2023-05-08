﻿using Microsoft.UI.Xaml.Controls;

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
        /// <summary>
        /// 从文件中获取封面
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public static string GetImgInFiles(List<StorageFile> files)
        {
            files.Sort((x, y) => x.Name.CompareTo(y.Name));
            var imgFile = files.FirstOrDefault(x => pngs.Contains(x.FileType));
            return imgFile is null ? null : imgFile.Path;
        }
        /// <summary>
        /// 计算大小
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public static async Task<ulong> GetSizeInFiles(List<StorageFile> files)
        {
            ulong res = 0;
            foreach (var item in files)
            {
                if (pngs.Contains(item.FileType))
                {
                    res += (await item.GetBasicPropertiesAsync()).Size;
                }
            } 
            return res;
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
            var img = GetImgInFiles(oneFiles);
            var size = await GetSizeInFiles(oneFiles);
            // 无封面情况,从内部取
            
            // 最多2层漫画
            if (first.Count > 0)
            {
                foreach(var item in first)
                {
                    List<StorageFile> twoFiles = (await item.GetFilesAsync()).ToList();
                    if (img is null)
                    {
                        img = GetImgInFiles(twoFiles);
                    }
                    size += await GetSizeInFiles(twoFiles);
                }
            }
            if (img is null)
            {   
                img = "";
            }
            ComicDB.Add(new LocalComic(folder.DisplayName, "", parent, "0%", time, time, folder.Path, "Local", "", img, (long)size, false));

        }
    }
}
