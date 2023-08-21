using System.Diagnostics;
using ShadowViewer.Plugin.Local.Models;
using WinUIEx;

namespace ShadowViewer;

public sealed partial class MainWindow : WindowEx
{
    public ObservableCollection<IShadowSearchItem> SearchItems { get; } = new();
    public ObservableCollection<IHistory> HistoryHistories { get; } = new()
    {
        new LocalHistory()
        {
            Icon = @"C:\Users\15854\AppData\Local\Packages\27e531ce-1721-4ddf-80ef-f4c28f27a46d_fka8f3r9nhqje\LocalState\Temps\da793ecd6fcf510eac1b97dd93b0482b.png",
            Id = "114514",
            Time = DateTime.Now,
            Title = "PINK SEMINAR (ブルーアーカイブ) [中国翻訳] [DL版]"
        }
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
    /// 初始化插件
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
}