namespace ShadowViewer.Helpers
{
    public static class FileHelper
    {
        public static string[] pngs = { ".png", ".jpg", ".jpeg", ".bmp" };
        public static bool IsPic(this StorageFile file)
        {
            return pngs.Contains(file.FileType);
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
    }
}
