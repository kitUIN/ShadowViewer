using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI.Windowing;
using ShadowViewer.Core.Args;
using ShadowViewer.Core.Enums;
using ShadowViewer.Core.Services;
using Windows.Foundation;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Serilog;

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
    public event EventHandler? DebugEvent;

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

    /// <inheritdoc />
    public void ThemeChanged()
    {
        ThemeChangedEvent?.Invoke(this, EventArgs.Empty);
        Logger.Debug("触发事件{EventName}",nameof(ThemeChangedEvent));
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