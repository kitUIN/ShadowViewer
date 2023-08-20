namespace ShadowViewer.Responders;

public class LocalNavigationViewResponder : NavigationViewResponderBase
{
    public override IEnumerable<IShadowNavigationItem> NavigationViewMenuItems { get; } =
        new List<IShadowNavigationItem>
        {
            new ShadowNavigationItem
            {
                Icon = new SymbolIcon(Symbol.Home),
                Id = "BookShelf",
                Content = ResourcesHelper.GetString(ResourceKey.BookShelf)
            }
        };

    public override IEnumerable<IShadowNavigationItem> NavigationViewFooterItems { get; } =
        new List<IShadowNavigationItem>
        {
            new ShadowNavigationItem
            {
                Icon = new FontIcon { Glyph = "\uE835" },
                Id = "PluginManager",
                Content = ResourcesHelper.GetString(ResourceKey.PluginManager)
            }
        };

    public override void NavigationViewItemInvokedHandler(IShadowNavigationItem item, ref Type page,
        ref object parameter)
    {
    }
}