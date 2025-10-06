using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShadowPluginLoader.WinUI;
using ShadowViewer.ViewModels;
using System;
using ShadowViewer.Sdk.Configs;
using ShadowViewer.Sdk.Helpers;
using ShadowViewer.Sdk.Models.Interfaces;
using ShadowViewer.Sdk.Responders;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShadowViewer.Pages;

/// <summary>
/// 
/// </summary>
public sealed partial class ShadowTitleBar : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    public TitleBarViewModel ViewModel { get; } = DiFactory.Services.Resolve<TitleBarViewModel>();

    public CoreConfig CoreConfig { get; } = DiFactory.Services.Resolve<CoreConfig>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    public ShadowTitleBar(Window window)
    {
        this.InitializeComponent();
        AppTitleBar.Window = window;
    }

    /// <summary>
    /// 历史记录显示
    /// </summary>
    public void HistoryFlyout_OnOpening(object? sender, object e)
    {
        ViewModel.ReLoadHistory();
    }

    /// <summary>
    /// 改变主题监听
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void AppTitleBar_ThemeChangedEvent(object? sender, EventArgs e)
    {
        AppTitleBar.InvokeThemeChanged(this);
    }

    /// <summary>
    /// 初始化后退按钮
    /// </summary>
    /// <param name="action"></param>
    public void InitAppTitleBar_BackButtonClick(EventHandler<RoutedEventArgs> action)
    {
        AppTitleBar.BackButtonClick += action;
    }

    /// <summary>
    /// 初始化汉堡包按钮
    /// </summary>
    /// <param name="action"></param>
    public void InitAppTitleBar_OnPaneButtonClick(EventHandler<RoutedEventArgs> action)
    {
        AppTitleBar.PaneButtonClick += action;
    }

    /// <summary>
    /// 删除历史记录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HistoryDelete_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: IHistory history }) return;
        var responder = ResponderHelper.GetEnabledResponder<IHistoryResponder>(history.PluginId);
        responder?.DeleteHistoryHandler(history);
        ViewModel.ReLoadHistory();
    }
}