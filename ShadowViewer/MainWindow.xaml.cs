using System.Diagnostics;
using DryIoc.ImTools;
using ShadowViewer.Plugin.Local.Models;
using ShadowViewer.Responders;
using WinUIEx;

namespace ShadowViewer;

public sealed partial class MainWindow : WindowEx
{
    public ObservableCollection<IShadowSearchItem> SearchItems { get; } = new();
    public ObservableCollection<IHistory> HistoryHistories { get; set; } = new()
    {
 
    };
    private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
    private PluginService Plugins { get; } = DiFactory.Services.Resolve<PluginService>();

    public MainWindow()
    {
        InitializeComponent();
        AppTitleBar.Window = this;
        SystemBackdrop = new MicaBackdrop();
        Title = "ShadowViewer";
        AppTitleBar.Subtitle = Config.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
        Caller.DebugEvent += (_, _) =>
            AppTitleBar.Subtitle = Config.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
    }

    /// <summary>
    /// ��ʼ�����
    /// </summary>
    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void MainWindow_DebugEvent(object sender, EventArgs e)
    {
    }

    private void AppTitleBar_BackButtonClick(object sender, RoutedEventArgs e)
    {
        Caller.NavigationViewBackRequested(sender);
    }

    private void AppTitleBar_OnPaneButtonClick(object sender, RoutedEventArgs e)
    {
        Caller.NavigationViewPane(sender);
    }

    private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(string.IsNullOrEmpty(sender.Text)) SearchItems.Clear();
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            SearchItems.Clear();
            foreach (var plugin in Plugins.EnabledPlugins)
            {
                foreach(var i in plugin.SearchTextChanged(sender,args))
                {
                    SearchItems.Add(i);
                }
            }
            if (!string.IsNullOrEmpty(sender.Text))
                SearchItems.Add(new NavigateSearchItem(sender.Text));
        }
    }

    private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        foreach (var plugin in Plugins.EnabledPlugins)
        {
            plugin.SearchSuggestionChosen(sender, args);
        }
    }

    private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            if (args.ChosenSuggestion is NavigateSearchItem item)
            {
                NavigateHelper.ShadowNavigate(new Uri(item.Title));
            }
            else
            {
                foreach (var plugin in Plugins.EnabledPlugins)
                {
                    plugin.SearchQuerySubmitted(sender, args);
                }
            }
            
        }
        else if (sender.Items.Count != 0)
        {
            if (sender.Items[0] is NavigateSearchItem item)
            {
                NavigateHelper.ShadowNavigate(new Uri(item.Title));
            }
            else
            {
                foreach (var plugin in Plugins.EnabledPlugins)
                {
                    plugin.SearchQuerySubmitted(sender, args);
                }
            }
        }
        sender.Text = string.Empty;
    }

    private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
    {
        
    }

    private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
    {
        SearchItems.Clear();
    }

    private void FlyoutBase_OnOpening(object? sender, object e)
    {
        var responderService = DiFactory.Services.Resolve<ResponderService>();
        var responders = responderService.GetEnabledResponders<IHistoryResponder>();
        var temp = new SortedSet<IHistory>(new HistoryExtension());
        foreach (var responder in responders)
        {
            foreach (var item in responder.GetHistories())
            {
                temp.Add(item);
            }
        }
        HistoryHistories.Clear();
        foreach (var t in temp)
        {
            HistoryHistories.Add(t);
        }
    }
}