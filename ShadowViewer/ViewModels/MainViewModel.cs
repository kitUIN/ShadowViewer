using CommunityToolkit.Mvvm.Input;
using ShadowViewer.Responders;

namespace ShadowViewer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private ILogger Logger { get; }
    private ICallableService Caller { get; }
    private PluginService PluginService { get; }
    private ResponderService ResponderService { get; }

    public MainViewModel(ICallableService callableService, PluginService pluginService, ILogger logger,
        ResponderService responderService)
    {
        Logger = logger;
        Caller = callableService;
        PluginService = pluginService;
        ResponderService = responderService;
    }
    
    /// <summary>
    /// 历史记录
    /// </summary>
    public ObservableCollection<IHistory> Histories { get; } = new();

    /// <summary>
    /// 后退按钮
    /// </summary>
    public void AppTitleBar_BackButtonClick(object sender, RoutedEventArgs e)
    {
        Caller.NavigationViewBackRequested(sender);
    }
    /// <summary>
    /// 导航栏面板按钮
    /// </summary>
    public void AppTitleBar_OnPaneButtonClick(object sender, RoutedEventArgs e)
    {
        Caller.NavigationViewPane(sender);
    }
    /// <summary>
    /// 刷新历史记录
    /// </summary>
    private void ReLoad()
    {
        var responders = ResponderService.GetEnabledResponders<IHistoryResponder>();
        var temp = new SortedSet<IHistory>(new HistoryExtension());
        foreach (var responder in responders)
        {
            foreach (var item in responder.GetHistories())
            {
                temp.Add(item);
            }
        }
        Histories.Clear();
        foreach (var t in temp)
        {
            Histories.Add(t);
        }
    }
    /// <summary>
    /// 历史记录显示
    /// </summary>
    public void HistoryFlyout_OnOpening(object? sender, object e)
    {
        ReLoad();
    }
    /// <summary>
    /// 点击历史记录
    /// </summary>
    public void HistoryView_OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not IHistory history) return;
        var responder = ResponderService.GetEnabledResponder<IHistoryResponder>(history.Plugin);
        responder?.ClickHistoryHandler(history);
    }
    /// <summary>
    /// 删除历史记录
    /// </summary>
    public void HistoryDelete(object? sender)
    {
        if (sender is not FrameworkElement { Tag: IHistory history }) return;
        var responder = ResponderService.GetEnabledResponder<IHistoryResponder>(history.Plugin);
        responder?.DeleteHistoryHandler(history);
        ReLoad();
    }
    /// <summary>
    /// 搜索项
    /// </summary>
    public ObservableCollection<IShadowSearchItem> SearchItems { get; } = new();
    /// <summary>
    /// 搜索栏修改文字响应
    /// </summary>
    public void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) SearchItems.Clear();
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            SearchItems.Clear();
            foreach (var plugin in PluginService.EnabledPlugins)
            foreach (var i in plugin.SearchTextChanged(sender, args))
                SearchItems.Add(i);
            if (!string.IsNullOrEmpty(sender.Text))
                SearchItems.Add(new NavigateSearchItem(sender.Text));
        }
    }
    /// <summary>
    /// 搜索栏选择响应
    /// </summary>
    public void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        foreach (var plugin in PluginService.EnabledPlugins) plugin.SearchSuggestionChosen(sender, args);
    }
    /// <summary>
    /// 搜索栏提交响应
    /// </summary>
    public void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            if (args.ChosenSuggestion is NavigateSearchItem item)
                NavigateHelper.ShadowNavigate(new Uri(item.Title));
            else
                foreach (var plugin in PluginService.EnabledPlugins)
                    plugin.SearchQuerySubmitted(sender, args);
        }
        else if (sender.Items.Count != 0)
        {
            if (sender.Items[0] is NavigateSearchItem item)
                NavigateHelper.ShadowNavigate(new Uri(item.Title));
            else
                foreach (var plugin in PluginService.EnabledPlugins)
                    plugin.SearchQuerySubmitted(sender, args);
        }

        sender.Text = string.Empty;
    }
    /// <summary>
    /// 搜索栏聚焦
    /// </summary>
    public void AutoSuggestBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
    }
    /// <summary>
    /// 搜索栏失焦
    /// </summary>
    public void AutoSuggestBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        SearchItems.Clear();
    }
}