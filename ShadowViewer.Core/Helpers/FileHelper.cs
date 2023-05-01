﻿namespace ShadowViewer.Helpers
{
    public class FileHelper
    {
        public static async Task CreateFileAsync(StorageFolder localFolder, string path)
        {
            Log.ForContext<FileHelper>().Debug("创建文件: {Folder}/{Path}", localFolder.Path, path);
            await localFolder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
        }
        public static async Task CreateFolderAsync(StorageFolder localFolder,string path)
        {
            Log.ForContext<FileHelper>().Debug("创建文件夹: {Folder}/{Path}", localFolder.Path, path);
            await localFolder.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
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
                Log.ForContext<FileHelper>().Debug("选择了文件夹:{Path}", folder.Path);
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
                Log.ForContext<FileHelper>().Debug("选择了文件:{Path}", file.Path);
                return file;
            }
            return null;
        }
    }
}
