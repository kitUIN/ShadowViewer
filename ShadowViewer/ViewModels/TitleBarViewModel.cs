using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Serilog;
using ShadowPluginLoader.MetaAttributes;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core;
using ShadowViewer.Core.Extensions;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Models.Interfaces;
using ShadowViewer.Core.Responders;
using ShadowViewer.Core.Services;
using ShadowViewer.Helpers;
using ShadowViewer.I18n;
using ShadowViewer.Models;

namespace ShadowViewer.ViewModels;

public partial class TitleBarViewModel : ObservableObject
{
    /// <summary>
    /// 历史记录
    /// </summary>
    public ObservableCollection<IHistory> HistoryCollection { get; } = new();
    [Autowired]
    public PluginLoader PluginService { get;}
    [Autowired]
    public ICallableService Caller { get;}

    [ObservableProperty] private string subTitle = CoreSettings.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
    
    
    /// <summary>
    /// 刷新历史记录
    /// </summary>
    public void ReLoadHistory()
    {
        var responders = ResponderHelper.GetEnabledResponders<IHistoryResponder>();
        var temp = new SortedSet<IHistory>(new HistoryComparer());
        foreach (var responder in responders)
        {
            foreach (var item in responder.GetHistories())
            {
                temp.Add(item);
            }
        }
        HistoryCollection.Clear();
        foreach (var t in temp)
        {
            HistoryCollection.Add(t);
        }
    }
    
    /// <summary>
    /// 点击历史记录
    /// </summary>
    public void HistoryView_OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not IHistory history) return;
        var responder = ResponderHelper.GetEnabledResponder<IHistoryResponder>(history.PluginId);
        responder?.ClickHistoryHandler(history);
        if (sender is ListView {Parent:Grid{Parent:FlyoutPresenter{Parent:Popup popup}}} )
        {
            popup.IsOpen = false;
        }
    }
    /// <summary>
    /// 删除历史记录
    /// </summary>
    [RelayCommand]
    private void HistoryDelete(IHistory history)
    {
        var responder = ResponderHelper.GetEnabledResponder<IHistoryResponder>(history.PluginId);
        responder?.DeleteHistoryHandler(history);
        ReLoadHistory();
    }
    /// <summary>
    /// 搜索项
    /// </summary>
    public ObservableCollection<IShadowSearchItem> SearchItems { get; } = [];
    /// <summary>
    /// 搜索栏修改文字响应
    /// </summary>
    public void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(sender.Text)) SearchItems.Clear();
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            SearchItems.Clear();
            foreach (var plugin in PluginService!.GetEnabledPlugins())
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
        foreach (var plugin in PluginService!.GetEnabledPlugins()) plugin.SearchSuggestionChosen(sender, args);
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
                foreach (var plugin in PluginService!.GetEnabledPlugins())
                    plugin.SearchQuerySubmitted(sender, args);
        }
        else if (sender.Items.Count != 0)
        {
            if (sender.Items[0] is NavigateSearchItem item)
                NavigateHelper.ShadowNavigate(new Uri(item.Title));
            else
                foreach (var plugin in PluginService!.GetEnabledPlugins())
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