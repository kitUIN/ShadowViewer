using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ShadowPluginLoader.Attributes;
using ShadowViewer.Sdk.Args;
using ShadowViewer.Sdk.Navigation;
using ShadowViewer.Sdk.Services;

namespace ShadowViewer.Services;

/// <summary>
/// <inheritdoc />
/// </summary>
public partial class NavigateService : INavigateService
{
    [Autowired] private Frame ContentFrame { get; }


    /// <inheritdoc />
    public event EventHandler<TrySelectItemEventArgs>? TrySelectItemEvent;

    /// <inheritdoc />
    public void Navigate(Type page, object? parameter = null,
        NavigationTransitionInfo? info = null, bool force = false, string? selectItemId = null)
    {
        if (ContentFrame.CurrentSourcePageType == page && !force) return;
        if (info == null) ContentFrame.Navigate(page, parameter);
        else ContentFrame.Navigate(page, parameter, info);
        TrySelectItemEvent?.Invoke(this, new TrySelectItemEventArgs(selectItemId));
    }

    public void Navigate(ShadowUri uri, NavigationTransitionInfo? info = null)
    {
        if (uri.Scheme != "shadow") return;

        var item = ShadowRouteRegistry.ResolvePage(uri);
        if (item == null) return;
        Navigate(item.Page, item.Parameter ?? uri, item.Info ?? info, item.Force, item.SelectItemId);
    }

    public void Navigate(Uri uri, NavigationTransitionInfo? info  = null)
    {
        Navigate(ShadowUri.Parse(uri), info);
    }
}