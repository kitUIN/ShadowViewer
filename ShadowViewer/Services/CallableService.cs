using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using Windows.Foundation;
using ShadowViewer.Sdk.Args;
using ShadowViewer.Sdk.Enums;
using ShadowViewer.Sdk.Services;

namespace ShadowViewer.Services;

/// <summary>
/// 触发器服务类
/// </summary>
internal class CallableService(ILogger logger) : ICallableService
{
    /// <summary>
    /// Logger
    /// </summary>
    private ILogger Logger { get; } = logger;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event EventHandler? DebugEvent;

    /// <inheritdoc />
    public event EventHandler<TopLevelControlEventArgs>? TopLevelControlEvent;

    /// <inheritdoc />
    public event EventHandler? AppLoadedEvent;

    /// <inheritdoc />
    public event EventHandler? ThemeChangedEvent;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event TypedEventHandler<AppWindow, AppWindowChangedEventArgs>? OverlappedChangedEvent;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Debug()
    {
        DebugEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件{EventName}", nameof(DebugEvent));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ThemeChanged()
    {
        ThemeChangedEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件{EventName}", nameof(ThemeChangedEvent));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void AppLoaded()
    {
        AppLoadedEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件{EventName}", nameof(AppLoadedEvent));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ChangeOverlapped(AppWindow sender, AppWindowChangedEventArgs args)
    {
        OverlappedChangedEvent?.Invoke(sender, args);
        Logger.Debug("触发事件{Event}", nameof(OverlappedChangedEvent));
    }

    /// <inheritdoc />
    public void CreateTopLevelControl(UIElement control)
    {
        TopLevelControlEvent?.Invoke(this, 
            new TopLevelControlEventArgs(control, TopLevelControlMode.Add));
        Logger.Debug("触发事件{Event}", nameof(TopLevelControlEvent));
    }

    /// <inheritdoc />
    public void RemoveTopLevelControl(UIElement control)
    {
        TopLevelControlEvent?.Invoke(this,
            new TopLevelControlEventArgs(control, TopLevelControlMode.Remove));
        Logger.Debug("触发事件{Event}", nameof(TopLevelControlEvent));
    }
}