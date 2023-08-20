using System.Collections.Generic;
using ShadowViewer.Interfaces;
using ShadowViewer.Models;

namespace ShadowViewer.Responders;

public class ShadowNavigationResponder : NavigationResponderBase
{
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

    public override void NavigationViewItemInvokedHandler(IShadowNavigationItem item, ref Type? page,
        ref object? parameter)
    {
        if (item.Id == "PluginManager")
        {
            page = typeof(PluginPage);
        }
    }

    public ShadowNavigationResponder(string id) : base(id)
    {
    }
}