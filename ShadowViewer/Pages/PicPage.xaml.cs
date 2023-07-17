using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Utils.Args;
using System.Diagnostics;

namespace ShadowViewer.Pages
{

    public sealed partial class PicPage : Page
    {
        public PicViewModel ViewModel { get; set; }
        public PicPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is LocalComic comic)
            {
                ViewModel = new PicViewModel(comic);
            }
        }
        

        private void ScrollViewer_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.PageDown)
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;
                scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + scrollViewer.ViewportHeight, null);
            }
        }

        private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Menu.Visibility = (Menu.Visibility != Visibility.Visible).ToVisibility();
        }
        private bool scroll = false;
        private bool userInput = true; 

        private void PicViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = (VisualTreeHelper.GetChild(PicViewer, 0) as Border).Child as ScrollViewer;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if(PageSlider.FocusState != FocusState.Pointer)
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;
                double y = scrollViewer.VerticalOffset;
                int i;
                for (i = 0; i < PicViewer.Items.Count; i++)
                {
                    FrameworkElement f = PicViewer.ContainerFromIndex(i) as FrameworkElement;
                    if (f == null || f.ActualOffset.Y <= y) continue;
                    else
                    {
                        ViewModel.CurrentPage = i + 1;
                        break;
                    }
                }
            }
        }

        private void PageSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue - 1 >= 0 && e.NewValue - 1 < PicViewer.Items.Count && PageSlider.FocusState == FocusState.Pointer)
            {
                PicViewer.ScrollIntoView(PicViewer.Items[(int)(e.NewValue - 1)]);
            }
        }

        private void PageSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        { 
        }
    }
}
