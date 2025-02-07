using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Core.Models.Interfaces;

namespace ShadowViewer.Models;

public partial class ShadowNavigationItem : ObservableObject, IShadowNavigationItem
{
    /// <summary>
    /// 内容
    /// </summary>
    [ObservableProperty] private object? content;

    /// <summary>
    /// 图标
    /// </summary>
    [ObservableProperty] private IconElement? icon;

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public string? Id { get;  }

    /// <inheritdoc />
    public string PluginId { get; }
}