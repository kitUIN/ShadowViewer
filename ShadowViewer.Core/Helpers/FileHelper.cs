
namespace ShadowViewer.Helpers
{
    public static class FileHelper
    {
        public static string[] pngs = { ".png", ".jpg", ".jpeg", ".bmp" };
        public static string[] zips = { ".zip", ".rar", ".7z" };
        
        public static bool IsPic(this StorageFile file)
        {
            return pngs.Contains(file.FileType);
        }
        public static bool IsZip(this StorageFile file)
        {
            return zips.Contains(file.FileType);
        }
        public static async Task<StorageFile> IsFileExist(this string uri)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(uri);
                return file;
            } 
            catch (Exception exception) when (exception is System.Runtime.InteropServices.COMException || exception is FileNotFoundException)
            {
                 return null;
            }
        }
        public static void RarCompress(string rar,string destinationDirectory)
        {
            ReaderOptions options = new ReaderOptions();
            options.ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
            using (Stream stream = File.OpenRead(rar))
            {
                var reader = ReaderFactory.Open(stream, options);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    { 
                        reader.WriteEntryToDirectory(destinationDirectory, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }
            }
        }
        public static void ZipCompress(string zip, string destinationDirectory)
        {
            ReaderOptions options = new ReaderOptions();
            options.ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
            var archive = ArchiveFactory.Open(zip, options);
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(destinationDirectory, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
            }
        } 

        public static async Task CreateFileAsync(StorageFolder localFolder, string path)
        {
            try
            {
                await localFolder.CreateFileAsync(path, CreationCollisionOption.FailIfExists);
                Log.ForContext<StorageFolder>().Debug("创建文件: {Folder}/{Path}", localFolder.Path, path);
            }
            catch (Exception)
            {

            }
        }
        public static async Task CreateFolderAsync(StorageFolder localFolder,string path)
        {
            try
            {
                await localFolder.CreateFolderAsync(path, CreationCollisionOption.FailIfExists);
                Log.ForContext<StorageFolder>().Debug("创建文件夹: {Folder}/{Path}", localFolder.Path, path);
            }
            catch (Exception)
            {

            }
            
            
        }
        public static async Task<StorageFolder> SelectFolderAsync(UIElement element, string accessToken = "")
        {
            var window = WindowHelper.GetWindowForElement(element);
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            FolderPicker openPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                if(accessToken != "")
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(accessToken, folder);
                }
                Log.ForContext<FolderPicker>().Debug("选择了文件夹:{Path}", folder.Path);
                return folder;
            }
            return null;
        }
        public static async Task<StorageFile> SelectFileAsync(UIElement element, params string[] filter)
        {
            var window = WindowHelper.GetWindowForElement(element);
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            FileOpenPicker openPicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            foreach ( var filterItem in filter) { openPicker.FileTypeFilter.Add(filterItem); }
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                Log.ForContext<FileOpenPicker>().Debug("选择了文件:{Path}", file.Path);
                return file;
            }
            return null;
        }
        /// <summary>
        /// 计算大小
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public static async Task<ulong> GetSizeInFiles(IReadOnlyList<StorageFile> files)
        {
            ulong res = 0;
            foreach (var item in files.Where(x => pngs.Contains(x.FileType)))
            {
                res += (await item.GetBasicPropertiesAsync()).Size;
            }
            return res;
        }
        /// <summary>
        /// 从文件中获取封面
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        public static string GetImgInFiles(IReadOnlyList<StorageFile> files)
        {
            
            var imgFile = files.OrderBy(x => x.Name).FirstOrDefault(x => pngs.Contains(x.FileType));
            return imgFile is null ? "" : imgFile.Path;
        }
    }
}
