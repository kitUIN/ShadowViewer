using System.Diagnostics;
using WinUIEx;

namespace ShadowViewer;

public sealed partial class MainWindow : WindowEx
{
    public ObservableCollection<IShadowSearchItem> SearchItems { get; } = new();
    private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
    private IPluginService Plugins { get; } = DiFactory.Services.Resolve<IPluginService>();

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
    /// ³õÊ¼»¯²å¼þ
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