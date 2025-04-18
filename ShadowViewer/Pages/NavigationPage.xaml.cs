using System;
using System.Linq;
using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Serilog;
using ShadowPluginLoader.WinUI;
using ShadowPluginLoader.WinUI.Args;
using ShadowViewer.Core.Args;
using ShadowViewer.Core.Enums;
using ShadowViewer.Core.Models.Interfaces;
using ShadowViewer.Core.Services;
using ShadowViewer.ViewModels;
using ShadowViewer.Services;
using ShadowViewer.Core.Controls;

namespace ShadowViewer.Pages
{
    public sealed partial class NavigationPage : ShadowPage
    {
        public static ILogger Logger { get; } = Log.ForContext<NavigationPage>();
        private NavigationViewModel ViewModel { get; } = DiFactory.Services.Resolve<NavigationViewModel>();
        private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();
        private INotifyService NotifyService { get; } = DiFactory.Services.Resolve<INotifyService>();
        private PluginEventService PluginEventService { get; } = DiFactory.Services.Resolve<PluginEventService>();

        public NavigationPage()
        {
            this.InitializeComponent();

            DiFactory.Services.Register<INavigateService, NavigateService>(reuse: Reuse.Singleton,
                made: Parameters.Of.Type(_ => ContentFrame));
            ViewModel.InitItems();
            PluginEventService.PluginLoaded += CallerOnPluginEnabledEvent;
            PluginEventService.PluginEnabled += CallerOnPluginEnabledEvent;
            PluginEventService.PluginDisabled += CallerOnPluginEnabledEvent;
            NotifyService.TipPopupEvent += NotifyService_TipPopupEvent;
            Caller.TopLevelControlEvent += CallerTopLevelControlEvent;
            DiFactory.Services.Resolve<INavigateService>().TrySelectItemEvent += TrySelectItemCaller;
        }

        /// <summary>
        /// 
        /// </summary>
        private void TrySelectItemCaller(object? sender, TrySelectItemEventArgs e)
        {
            if (e.Id == null) return;
            object? item;
            if (e.Id == "_settings") item = NavView.SettingsItem;
            else
            {
                item = ViewModel.MenuItems.FirstOrDefault(x => x.Id == e.Id) ?? ViewModel.FooterMenuItems.FirstOrDefault(x => x.Id == e.Id);
            }
            if (item == null) return;
            NavView.SelectedItem = item;
        }

        /// <summary>
        /// TopLevelControlEvent
        /// </summary>
        private void CallerTopLevelControlEvent(object? sender, TopLevelControlEventArgs e)
        {
            if (e.Mode == TopLevelControlMode.Add)
            {
                TopGrid.Children.Add(e.Control);
            }
            else
            {
                TopGrid.Children.Remove(e.Control);
            }
        }

        private void NotifyService_TipPopupEvent(object? sender, TipPopupEventArgs e)
        {
            if (e.Position == TipPopupPosition.Right)
            {
                TipContainerRight.Notify(e.TipPopup, e.DisplaySeconds);
            }
            else
            {
                TipContainer.Notify(e.TipPopup, e.DisplaySeconds);
            }
        }


        /// <summary>
        /// 启用或禁用插件时更新左侧导航栏
        /// </summary>
        private void CallerOnPluginEnabledEvent(object? sender, PluginEventArgs e)
        {
            ViewModel.ReloadItems(e.PluginId, e.Status);
        }


        /// <summary>
        /// 左侧点击导航栏
        /// </summary>
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            Type? page = null;
            object? parameter = null;
            var info = args.RecommendedNavigationTransitionInfo;
            if (args.IsSettingsInvoked)
            {
                page = typeof(SettingsPage);
            }
            else if (args.InvokedItemContainer != null &&
                     args.InvokedItemContainer.Tag is IShadowNavigationItem item &&
                     ViewModel.NavigationViewItemInvokedHandler(item) is { } navigation)
            {
                parameter = navigation.Parameter;
                page = navigation.Page;
                info = navigation.Info;
            }

            if (page == null || ContentFrame.CurrentSourcePageType == page) return;
            ContentFrame.Navigate(page, parameter, info);
        }

        /// <summary>
        /// 初始化插件
        /// </summary>
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void SmokeGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 取消导入
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // _cancelTokenSource.Cancel();
        }

        /// <summary>
        /// 导航栏后退按钮 点击
        /// </summary>
        public void AppTitleBar_BackButtonClick(object? sender, RoutedEventArgs e)
        {
            if (!ContentFrame.CanGoBack) return;
            ContentFrame.GoBack();
        }

        /// <summary>
        /// 导航栏面板按钮 点击
        /// </summary>
        public void AppTitleBar_OnPaneButtonClick(object? sender, RoutedEventArgs e)
        {
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }
    }
}