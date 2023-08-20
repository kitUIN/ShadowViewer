using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Media;
using Serilog;
using ShadowViewer.Enums;
using ShadowViewer.Helpers;
using ShadowViewer.Interfaces;
using ShadowViewer.Models;
using ShadowViewer.Plugin.Core.Enums;
using ShadowViewer.Plugin.Core.Helpers;
using SqlSugar;

namespace ShadowViewer.ViewModels;

public partial class AttributesViewModel : ObservableObject
{
    /// <summary>
    /// 最大文本宽度
    /// </summary>
    [ObservableProperty] private double textBlockMaxWidth;

    /// <summary>
    /// 当前漫画
    /// </summary>
    public LocalComic CurrentComic { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public ObservableCollection<LocalTag> Tags = new();

    /// <summary>
    /// 话
    /// </summary>
    public ObservableCollection<LocalEpisode> Episodes = new();

    /// <summary>
    /// 是否有话
    /// </summary>
    public bool IsHaveEpisodes => Episodes.Count != 0;

    private readonly PluginService pluginService;
    private ISqlSugarClient Db { get; }
    private ILogger Logger { get; }

    public void Init(string comicId)
    {
        CurrentComic = Db.Queryable<LocalComic>().First(x => x.Id == comicId);
        ReLoadTags();
        ReLoadEps();
    }

    public AttributesViewModel(PluginService pluginService, ISqlSugarClient sqlSugarClient, ILogger logger)
    {
        this.pluginService = pluginService;
        Db = sqlSugarClient;
        Logger = logger;
    }

    /// <summary>
    /// 重新加载-话
    /// </summary>
    public void ReLoadEps()
    {
        Episodes.Clear();
        foreach (var item in Db.Queryable<LocalEpisode>().Where(x => x.ComicId == CurrentComic.Id).ToList())
            Episodes.Add(item);
    }

    /// <summary>
    /// 重新加载-标签
    /// </summary>
    public void ReLoadTags()
    {
        Tags.Clear();
        if (pluginService.GetAffiliationTag(CurrentComic.Affiliation) is { } shadow)
        {
            shadow.IsEnable = false;
            shadow.Icon = "\uE23F";
            shadow.ToolTip = ResourcesHelper.GetString(ResourceKey.Affiliation) + ": " + shadow.Name;
            Tags.Add(shadow);
        }

        if (CurrentComic.Tags != null)
            foreach (var item in CurrentComic.Tags)
            {
                item.Icon = "\uEEDB";
                item.ToolTip = ResourcesHelper.GetString(ResourceKey.Tag) + ": " + item.Name;
                Tags.Add(item);
            }


        Tags.Add(new LocalTag
        {
            Icon = "\uE008",
            // Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseMediumLowBrush"],
            Foreground = new SolidColorBrush((ThemeHelper.IsDarkTheme() ? "#FFFFFFFF" : "#FF000000").ToColor()),
            IsEnable = true,
            Name = ResourcesHelper.GetString(ResourceKey.AddTag),
            ToolTip = ResourcesHelper.GetString(ResourceKey.AddTag)
        });
    }

    /// <summary>
    /// 添加-标签
    /// </summary>
    public void AddNewTag(LocalTag tag)
    {
        if (Db.Queryable<LocalTag>().First(x => x.Id == tag.Id) is LocalTag localTag)
        {
            tag.ComicId = localTag.ComicId;
            tag.Icon = "\uEEDB";
            tag.ToolTip = ResourcesHelper.GetString(ResourceKey.Tag) + ": " + localTag.Name;
            Db.Updateable(tag).ExecuteCommand();
            if (Tags.FirstOrDefault(x => x.Id == tag.Id) is LocalTag lo) Tags[Tags.IndexOf(lo)] = tag;
        }
        else
        {
            tag.Id = LocalTag.RandomId();
            tag.ComicId = CurrentComic.Id;
            tag.Icon = "\uEEDB";
            tag.ToolTip = ResourcesHelper.GetString(ResourceKey.Tag) + ": " + tag.Name;
            Db.Insertable(tag).ExecuteCommand();
            Tags.Insert(Math.Max(0, Tags.Count - 1), tag);
        }
    }

    /// <summary>
    /// 删除-标签
    /// </summary>
    public void RemoveTag(string id)
    {
        if (Tags.FirstOrDefault(x => x.Id == id) is LocalTag tag)
        {
            Tags.Remove(tag);
            Db.Deleteable(tag).ExecuteCommand();
        }
    }

    /// <summary>
    /// 是否是最后一个标签
    /// </summary>
    public bool IsLastTag(LocalTag tag)
    {
        return Tags.IndexOf(tag) == Tags.Count - 1;
    }
}