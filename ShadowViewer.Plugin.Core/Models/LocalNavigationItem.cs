using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Interfaces;

namespace ShadowViewer.Plugin.Core.Models;

public partial class LocalNavigationItem: ObservableObject, IShadowNavigationItem
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
    public string? Id { get; set; }
}