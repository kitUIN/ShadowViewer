using System.Diagnostics;

namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    public ObservableCollection<IShadowSearchItem> SearchItems { get; } = new();
    private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
    private IPluginService Plugins { get; } = DiFactory.Services.Resolve<IPluginService>();

    public MainWindow()
    {
        InitializeComponent();
        AppTitleBar.Window = this;
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
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput&&!string.IsNullOrEmpty(sender.Text))
        {
            SearchItems.Clear();
            foreach (var plugin in Plugins.EnabledPlugins)
            {
                foreach(var i in plugin.SearchTextChanged(sender,args))
                {
                    SearchItems.Add(i);
                }
            }
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
        else
        {
            
            // Use args.QueryText to determine what to do.
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