﻿using System.Diagnostics;
using ShadowViewer.Plugin.Local.Enums;
using ShadowViewer.Plugins;
using ShadowViewer.Plugin.Local.Models;
using SqlSugar;

namespace ShadowViewer.Plugin.Local;

[PluginMetaData("Local",
    "本地阅读器",
    "本地阅读适配器",
    "kitUIN", "0.1.0",
    "https://github.com/kitUIN/ShadowViewer/",
    "/",
    20230808)]
public class LocalPlugin : PluginBase
{
    public LocalPlugin(ICallableService callableService, ISqlSugarClient sqlSugarClient,
        CompressService compressServices, IPluginService pluginService) : base(callableService, sqlSugarClient,
        compressServices, pluginService)
    {
    }

    public override PluginMetaData MetaData { get; } = typeof(LocalPlugin).GetPluginMetaData();
    public override LocalTag AffiliationTag { get; }
    public override Type SettingsPage { get; }
    public override bool CanSwitch { get; } = false;
    public override bool CanDelete { get; } = false;

    protected override void PluginEnabled()
    {
        Caller.CurrentEpisodeIndexChangedEvent += LoadLocalImage;
        Caller.PicturesLoadStartingEvent += LoadImageFormLocalComic;
    }

    protected override void PluginDisabled()
    {
        Caller.CurrentEpisodeIndexChangedEvent -= LoadLocalImage;
        Caller.PicturesLoadStartingEvent -= LoadImageFormLocalComic;
    }

    /// <summary>
    /// Episode变化响应
    /// </summary>
    private void LoadLocalImage(object sender, CurrentEpisodeIndexChangedEventArg arg)
    {
        if (sender is not PicViewModel viewModel) return;
        if (arg.OldValue == arg.NewValue) return;
        if (viewModel.Affiliation != MetaData.Id) return;
        viewModel.Images.Clear();
        var index = 0;
        if (viewModel.Episodes.Count > 0 && viewModel.Episodes[arg.NewValue] is ShadowEpisode episode)
            foreach (var item in Db.Queryable<LocalPicture>().Where(x => x.EpisodeId == episode.Source.Id)
                         .OrderBy(x => x.Name)
                         .ToList())
                viewModel.Images.Add(new ShadowPicture(++index, item.Img));
    }

    /// <summary>
    /// 从本地漫画加载Episode
    /// </summary>
    private void LoadImageFormLocalComic(object sender, PicViewArg arg)
    {
        if (sender is not PicViewModel viewModel) return;
        if (arg.Affiliation != MetaData.Id || arg.Parameter is not LocalComic comic) return;

        var orders = new List<int>();
        Db.Queryable<LocalEpisode>().Where(x => x.ComicId == comic.Id).OrderBy(x => x.Order).ForEach(x =>
        {
            orders.Add(x.Order);
            viewModel.Episodes.Add(new ShadowEpisode(x));
        });
        if (viewModel.CurrentEpisodeIndex == -1 && orders.Count > 0)
            viewModel.CurrentEpisodeIndex = orders[0];
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override IEnumerable<IShadowSearchItem> SearchTextChanged(AutoSuggestBox sender,
        AutoSuggestBoxTextChangedEventArgs args)
    {
        var res = new List<IShadowSearchItem>();
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !string.IsNullOrEmpty(sender.Text))
            res.AddRange(Db.Queryable<LocalComic>().Where(x => x.Name.Contains(sender.Text)).ToList().Select(item =>
                new LocalSearchItem(item.Name, MetaData.Id, item.Id, LocalSearchMode.SearchComic)));
        return res;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void SearchSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is LocalSearchItem item)
        {
            sender.Text = item.Title;
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        
    }
}