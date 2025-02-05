using Microsoft.UI.Windowing;
using Windows.Foundation;

namespace ShadowViewer.Services;

/// <summary>
/// 触发器服务类
/// </summary>
internal partial class CallableService(ILogger logger) : ICallableService
{
    private ILogger Logger { get; } = logger;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<NavigateToEventArgs>? NavigateToEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler? RefreshBookEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<ImportComicEventArgs>? ImportComicEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<ImportComicErrorEventArgs>? ImportComicErrorEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<ImportComicThumbEventArgs>? ImportComicThumbEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<ImportComicProgressEventArgs>? ImportComicProgressEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler? ImportComicCompletedEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler? DebugEvent;

    /// <inheritdoc />
    public event EventHandler? ThemeChangedEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<TopGridEventArg>? TopGridEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler<ImportPluginEventArg>? ImportPluginEvent; 

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event TypedEventHandler<AppWindow, AppWindowChangedEventArgs>? OverlappedChangedEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ImportComic(IReadOnlyList<IStorageItem> items, string[] passwords, int index)
    {
        ImportComicEvent?.Invoke(this, new ImportComicEventArgs(items, passwords, index));
        Logger.Debug("触发事件ImportComicEvent");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ImportComicThumb(MemoryStream stream)
    {
        ImportComicThumbEvent?.Invoke(this, new ImportComicThumbEventArgs(stream));
        Logger.Debug("触发事件ImportComicThumbEvent");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ImportComicError(ImportComicError error, string message, IReadOnlyList<IStorageItem> items, int index,
        string[] password)
    {
        ImportComicErrorEvent?.Invoke(this, new ImportComicErrorEventArgs(error, message, items, index, password));
        Logger.Debug("触发事件ImportComicErrorEvent");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ImportComicProgress(double progress)
    {
        ImportComicProgressEvent?.Invoke(this, new ImportComicProgressEventArgs(progress));
        Logger.Debug("触发事件ImportComicProgressEvent");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NavigateTo(Type page, object? parameter,bool force=false)
    {
        var args = new NavigateToEventArgs(page, parameter, force);
        NavigateToEvent?.Invoke(this, args);
        Logger.Debug("触发事件NavigateTo,{Args}", args.ToString());
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void RefreshBook()
    {
        RefreshBookEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件RefreshBook");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ImportComicCompleted()
    {
        ImportComicCompletedEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件ImportComicCompletedEvent");
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Debug()
    {
        DebugEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件{EventName}", nameof(DebugEvent));
    }

    /// <inheritdoc />
    public void ThemeChanged()
    {
        ThemeChangedEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件{EventName}",nameof(ThemeChangedEvent));
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void TopGrid(object sender, UIElement element, TopGridMode mode)
    {
        Logger.Debug("{Sender}触发事件{Event}", sender.GetType().FullName,
            nameof(TopGridEvent));
        TopGridEvent?.Invoke(sender, new TopGridEventArg(element, mode));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ImportPlugin(object sender, IReadOnlyList<IStorageItem> items)
    {
        ImportPluginEvent?.Invoke(sender, new ImportPluginEventArg(items));
        Logger.Debug("触发事件{Event}", nameof(ImportPluginEvent));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ChangeOverlapped(AppWindow sender, AppWindowChangedEventArgs args)
    {
        OverlappedChangedEvent?.Invoke(sender, args);
        Logger.Debug("触发事件{Event}", nameof(OverlappedChangedEvent));
    }
}