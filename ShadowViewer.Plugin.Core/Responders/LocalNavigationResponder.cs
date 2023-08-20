using System.Collections.Generic;
using System;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using ShadowViewer.Interfaces;
using ShadowViewer.Models;
using ShadowViewer.Plugin.Core.Enums;
using ShadowViewer.Plugin.Core.Helpers;
using ShadowViewer.Plugin.Core.Models;
using ShadowViewer.Responders;
using Symbol = Microsoft.UI.Xaml.Controls.Symbol;

namespace ShadowViewer.Plugin.Core.Responders;

public class LocalNavigationResponder : NavigationResponderBase
{
    public override IEnumerable<IShadowNavigationItem> NavigationViewMenuItems { get; } =
        new List<IShadowNavigationItem>
        {
            new LocalNavigationItem
            {
                Icon = new SymbolIcon(Symbol.Home),
                Id = "BookShelf",
                Content = LocalResourcesHelper.GetString(LocalResourceKey.BookShelf)
            }
        };

    public override void NavigationViewItemInvokedHandler(IShadowNavigationItem item, ref Type? page,
        ref object? parameter)
    {
        /*if (item.Id == "BookShelf")
        {
            page = typeof(BookShelfPage);
            parameter = new Uri("shadow://local/");
        }*/
    }

    public LocalNavigationResponder(string id) : base(id)
    {
    }
}