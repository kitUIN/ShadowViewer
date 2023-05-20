using SharpCompress.Common;
using System.Linq;

namespace ShadowViewer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PicPage : Page
    {
        public ObservableCollection<BitmapImage> Images { get; set; } = new ObservableCollection<BitmapImage>();
        public PicPage()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ShadowEntry entry)
            { 
                await LoadImageFormEntry(entry);
            } 
        }
        public async Task LoadImageFormEntry(ShadowEntry entry)
        {
            foreach (ShadowEntry item in entry.Children.OrderBy(x => x.Name))
            { 
                if(item.IsDirectory)
                {
                    await LoadImageFormEntry(item);
                }
                else if (item.Source != null)
                {
                    BitmapImage image = new BitmapImage();
                    item.Source.Seek(0, SeekOrigin.Begin);
                    await image.SetSourceAsync(item.Source.AsRandomAccessStream());
                    Images.Add(image);
                }
            }
        }
    }
}
