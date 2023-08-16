using Windows.System;
using DryIoc;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using ShadowViewer.Args;
using ShadowViewer.Extensions;
using ShadowViewer.Plugin.Local.ViewModels;

namespace ShadowViewer.Plugin.Local.Pages
{

    public sealed partial class PicPage : Page
    {
        public PicViewModel ViewModel { get; set; }
        public PicPage()
        {
            this.InitializeComponent();
            ViewModel = DiFactory.Services.Resolve<PicViewModel>();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is PicViewArg arg)
            {
                ViewModel.Affiliation = arg.Affiliation;
                ViewModel.Init(arg);
            }
        }

        private void ScrollViewer_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.PageDown&& sender is ScrollViewer scrollViewer)
            {
                scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + scrollViewer.ViewportHeight, null);
            }
        }

        private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Menu.Visibility = (Menu.Visibility != Visibility.Visible).ToVisibility();
        }

        private void PicViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //var scrollViewer = (VisualTreeHelper.GetChild(PicViewer, 0) as Border).Child as ScrollViewer;
            //scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        /// <summary>
        /// 响应移动视图
        /// </summary>
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (PageSlider.FocusState == FocusState.Pointer || ViewModel == null ||
                sender is not ScrollViewer scrollViewer) return;
            var y = scrollViewer.VerticalOffset;
            int i;
            for (i = 0; i < ViewModel.Images.Count; i++)
            {
                var f = PicViewer.TryGetElement(i) as FrameworkElement;
                if (f == null || f.ActualOffset.Y <= y) continue;
                if(ViewModel.CurrentPage != i + 1)
                {
                    ViewModel.CurrentPage = i + 1;
                }
                break;
            }
            if (scrollViewer.VerticalOffset + scrollViewer.ActualHeight + 2 >= scrollViewer.ExtentHeight)
            {

            }
        }
        /// <summary>
        /// 移动进度条响应
        /// </summary>
        private void PageSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel == null || !(e.NewValue - 1 >= 0) || !(e.NewValue - 1 < ViewModel.Images.Count) ||
                PageSlider.FocusState != FocusState.Pointer) return;
            var element = PicViewer.GetOrCreateElement((int)(e.NewValue - 1));
            PicViewer.UpdateLayout();
            element.StartBringIntoView(new BringIntoViewOptions() { VerticalOffset = 0D, VerticalAlignmentRatio = 0.0f });
        }

    }
}
