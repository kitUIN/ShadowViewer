using System;
using Serilog;
using ShadowViewer.Args;
using ShadowViewer.ViewModels; 

namespace ShadowViewer.Plugin.Local.Services;

public class PicViewService
{
    private ILogger Logger { get; }

    public PicViewService(ILogger logger)
    {
        Logger = logger;
    }
    /// <summary>
    /// 图片界面章节改变事件
    /// </summary>
    public event EventHandler<CurrentEpisodeIndexChangedEventArg> CurrentEpisodeIndexChangedEvent;
    /// <summary>
    /// 图片页面加载图片事件
    /// </summary>
    public event EventHandler<PicViewArg> PicturesLoadStartingEvent;
    /*public void CurrentEpisodeIndexChanged(PicViewModel sender, string affiliation, int oldValue, int newValue)
    {
        Logger.Debug("触发事件{Event}", nameof(CurrentEpisodeIndexChangedEvent));
        CurrentEpisodeIndexChangedEvent?.Invoke(sender,
            new CurrentEpisodeIndexChangedEventArg(affiliation, oldValue, newValue));
    }

    public void PicturesLoadStarting(PicViewModel sender, PicViewArg arg)
    {
        Logger.Debug("触发事件{Event}", nameof(PicturesLoadStartingEvent));
        PicturesLoadStartingEvent?.Invoke(sender, arg);
    }*/
}