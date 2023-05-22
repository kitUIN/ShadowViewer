using SharpCompress.Common;

namespace ShadowViewer.ViewModels
{
    public partial class PicViewModel : ObservableObject
    {
        private static ILogger Logger = Log.ForContext<PicViewModel>();
        public ObservableCollection<BitmapImage> Images { get; set; } = new ObservableCollection<BitmapImage>();
        public LocalComic Comic { get; private set; }
        public List<LocalEpisode> Episodes { get; private set; }
        [ObservableProperty]
        private int maximumRows = 1;
        [ObservableProperty]
        private int imageWidth = 600; 
        public PicViewModel(ShadowEntry entry) 
        {
            LoadImageFormEntry(entry);
            Logger.Information("缓存流模式加载图片 {Path}", entry.Path);
        }
        public PicViewModel(LocalComic comic) 
        {
            Comic = comic;
            LoadImageFormComic();
            Logger.Information("本地图片流模式加载图片 {Path}", comic.Path);
        }
        /// <summary>
        /// 从本地漫画加载图片
        /// </summary>
        private void LoadImageFormComic()
        {
            Episodes = DBHelper.Db.Queryable<LocalEpisode>().Where(x => x.ComicId == Comic.Id).OrderBy(x => x.Order).ToList();
            if(Episodes.Count > 0 )
            {
                foreach(LocalPicture item in DBHelper.Db.Queryable<LocalPicture>().Where(x => x.EpisodeId == Episodes[0].Id).OrderBy(x => x.Name).ToList())
                {
                    BitmapImage image = new BitmapImage();
                    image.UriSource = new Uri(item.Img);
                    Images.Add(image);
                } 
            }
        }
        /// <summary>
        /// 从缓存的数据流中加载漫画
        /// </summary>
        private async void LoadImageFormEntry(ShadowEntry entry)
        {
            foreach (ShadowEntry item in entry.Children.OrderBy(x => x.Name))
            {
                if (item.IsDirectory)
                {
                    LoadImageFormEntry(item);
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
