using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ShadowPluginLoader.Attributes;
using ShadowViewer.Core.Args;
using ShadowViewer.Core.Helpers;
using ShadowViewer.Core.Responders;
using ShadowViewer.Core.Services;
using ShadowViewer.Pages;

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

    public void Navigate(Uri uri)
    {
        if (uri.Scheme != "shadow") return;
        var urls = uri.AbsolutePath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        switch (uri.Host)
        {
            case "settings":
                Navigate(typeof(SettingsPage), selectItemId: "_settings");
                return;
            default:
                if (ResponderHelper.GetEnabledResponder<INavigationResponder>(uri.Host) is { } responder)
                {
                    var res = responder.Navigate(uri, urls);
                    if (res == null) return;
                    Navigate(res.Page, res.Parameter, res.Info, res.Force, res.SelectItemId);
                }

                break;
        }
    }
}