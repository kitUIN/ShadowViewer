using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using ShadowViewer.Sdk.Args;
using ShadowViewer.Sdk.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ShadowViewer.Services;

/// <summary>
/// 文件和文件夹选择服务实现类
/// </summary>
internal class FilePickerService(ILogger logger) : IFilePickerService
{
    /// <summary>
    /// 日志
    /// </summary>
    private ILogger Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<StorageFile?> PickSingleFileAsync(
        IList<string>? fileTypeFilter = null,
        PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary,
        PickerViewMode viewMode = PickerViewMode.List,
        string? settingsIdentifier = null)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = suggestedStartLocation,
            ViewMode = viewMode
        };

        if (!string.IsNullOrEmpty(settingsIdentifier))
        {
            picker.SettingsIdentifier = settingsIdentifier;
        }

        if (fileTypeFilter == null || fileTypeFilter.Count == 0)
        {
            picker.FileTypeFilter.Add("*");
        }
        else
        {
            foreach (var fileType in fileTypeFilter)
            {
                picker.FileTypeFilter.Add(fileType);
            }
        }

        var resultSource = new TaskCompletionSource<IStorageItem?>();
        WeakReferenceMessenger.Default.Send(new ShowSinglePickerArgs(picker, resultSource));
        var result = await resultSource.Task;

        Logger.Debug("Pick single file: {FileName}", result?.Path ?? "Cancelled");
        return result as StorageFile;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StorageFile>> PickMultipleFilesAsync(
        IList<string>? fileTypeFilter = null,
        PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary,
        PickerViewMode viewMode = PickerViewMode.List,
        string? settingsIdentifier = null)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = suggestedStartLocation,
            ViewMode = viewMode
        };

        if (!string.IsNullOrEmpty(settingsIdentifier))
        {
            picker.SettingsIdentifier = settingsIdentifier;
        }

        if (fileTypeFilter == null || fileTypeFilter.Count == 0)
        {
            picker.FileTypeFilter.Add("*");
        }
        else
        {
            foreach (var fileType in fileTypeFilter)
            {
                picker.FileTypeFilter.Add(fileType);
            }
        }

        var resultSource = new TaskCompletionSource<IReadOnlyList<IStorageItem>?>();
        WeakReferenceMessenger.Default.Send(new ShowMultiPickerArgs(picker, resultSource));
        var result = await resultSource.Task;

        var files = result?.Cast<StorageFile>().ToList() ?? new List<StorageFile>();
        Logger.Debug("Pick multiple files: {Count} file(s)", files.Count);
        return files;
    }

    /// <inheritdoc />
    public async Task<StorageFolder?> PickFolderAsync(
        PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary,
        PickerViewMode viewMode = PickerViewMode.List,
        string? settingsIdentifier = null)
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = suggestedStartLocation,
            ViewMode = viewMode
        };

        if (!string.IsNullOrEmpty(settingsIdentifier))
        {
            picker.SettingsIdentifier = settingsIdentifier;
        }

        picker.FileTypeFilter.Add("*");

        var resultSource = new TaskCompletionSource<IStorageItem?>();
        WeakReferenceMessenger.Default.Send(new ShowSinglePickerArgs(picker, resultSource));
        var result = await resultSource.Task;

        Logger.Debug("Pick folder: {FolderPath}", result?.Path ?? "Cancelled");
        return result as StorageFolder;
    }

    /// <inheritdoc />
    public async Task<StorageFile?> PickSaveFileAsync(
        IDictionary<string, IList<string>> fileTypeChoices,
        string? suggestedFileName = null,
        PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary,
        string? settingsIdentifier = null)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = suggestedStartLocation
        };

        if (!string.IsNullOrEmpty(settingsIdentifier))
        {
            picker.SettingsIdentifier = settingsIdentifier;
        }

        if (!string.IsNullOrEmpty(suggestedFileName))
        {
            picker.SuggestedFileName = suggestedFileName;
        }

        foreach (var choice in fileTypeChoices)
        {
            picker.FileTypeChoices.Add(choice.Key, choice.Value);
        }

        var resultSource = new TaskCompletionSource<IStorageItem?>();
        WeakReferenceMessenger.Default.Send(new ShowSinglePickerArgs(picker, resultSource));
        var result = await resultSource.Task;

        Logger.Debug("Save file: {FileName}", result?.Path ?? "Cancelled");
        return result as StorageFile;
    }
}
