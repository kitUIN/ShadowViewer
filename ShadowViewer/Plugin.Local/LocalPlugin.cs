using ShadowViewer.Plugins;
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
    public new static readonly PluginMetaData MetaData = typeof(LocalPlugin).GetPluginMetaData();
    public override LocalTag AffiliationTag { get; }
    public override Type SettingsPage { get; }

    public override void NavigationViewItemInvokedHandler(object tag, ref Type page, ref object parameter)
    {
    }

    protected override void PluginEnabled()
    {
        Caller.CurrentEpisodeIndexChangedEvent += LoadLocalImage;
        Caller.PicturesLoadStartingEvent+=LoadImageFormLocalComic;
    }

    protected override void PluginDisabled()
    {
        Caller.CurrentEpisodeIndexChangedEvent -= LoadLocalImage;
        Caller.PicturesLoadStartingEvent-=LoadImageFormLocalComic;
    }

    /// <summary>
    /// 从本地漫画加载图片
    /// </summary>
    private void LoadLocalImage(object sender, CurrentEpisodeIndexChangedEventArg arg)
    {
        if (sender is not PicViewModel viewModel) return;
        if (arg.OldValue == arg.NewValue) return;
        if (viewModel.Affiliation != MetaData.Id) return;
        viewModel.Images.Clear();
        var index = 0;
        if (viewModel.Episodes.Count > 0 && viewModel.Episodes[arg.NewValue].Source is LocalEpisode episode)
            foreach (var item in Db.Queryable<LocalPicture>().Where(x => x.EpisodeId == episode.Id).OrderBy(x => x.Name)
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
            viewModel.Episodes.Add(new ShadowEpisode() { Source = x, Title = x.Name });
        });
        if (viewModel.CurrentEpisodeIndex == -1 && orders.Count > 0)
            viewModel.CurrentEpisodeIndex = orders[0];
    }
}