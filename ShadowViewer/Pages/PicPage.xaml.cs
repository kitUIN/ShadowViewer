using CommunityToolkit.WinUI.Behaviors;
using System.Diagnostics;

namespace ShadowViewer.Pages
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
                ViewModel.Init(arg);
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

        private void PicViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //var scrollViewer = (VisualTreeHelper.GetChild(PicViewer, 0) as Border).Child as ScrollViewer;
            //scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (PageSlider.FocusState != FocusState.Pointer&&ViewModel != null)
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;
                double y = scrollViewer.VerticalOffset;
                int i;
                for (i = 0; i < ViewModel.Images.Count; i++)
                {
                    FrameworkElement f = PicViewer.TryGetElement(i) as FrameworkElement;
                    if (f == null || f.ActualOffset.Y <= y) continue;
                    else
                    {
                        if(ViewModel.CurrentPage != i + 1)
                        {
                            ViewModel.CurrentPage = i + 1;
                        }
                        break;
                    }
                }
                if (scrollViewer.VerticalOffset + scrollViewer.ActualHeight + 2 >= scrollViewer.ExtentHeight)
                {

                }
            }
        }

        private void PageSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel!=null&&e.NewValue - 1 >= 0 && e.NewValue - 1 < ViewModel.Images.Count && PageSlider.FocusState == FocusState.Pointer)
            {
                var element = PicViewer.GetOrCreateElement((int)(e.NewValue - 1));
                PicViewer.UpdateLayout();
                element.StartBringIntoView(new BringIntoViewOptions() { VerticalOffset = 0D, VerticalAlignmentRatio = 0.0f });
            }
        }

        public void ViewportBehavior_EnteredViewport(object sender, EventArgs e)
        {
            if(sender is Grid gird)
            {
                //gird.EffectiveViewportChanged
                //ViewModel.CurrentPage = pic.Index;
            }
        }
    }
}
