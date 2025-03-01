using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShadowPluginLoader.WinUI;
using ShadowViewer.Core;
using ShadowViewer.ViewModels;
using System;
using ShadowViewer.I18n;

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
    /// 调试模式监听
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void AppTitleBar_DebugEvent(object? sender, EventArgs e)
    {
        ViewModel.SubTitle = CoreSettings.IsDebug ? I18N.Debug : "";
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
}