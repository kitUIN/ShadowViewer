using Newtonsoft.Json.Linq;
using ShadowViewer.Utils.Args;
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
        private int maximumColumns = 1;
        [ObservableProperty]
        private int imageWidth = 800;
        [ObservableProperty]
        private int currentPage = 1;

        public PicViewModel(LocalComic comic) 
        {
            Comic = comic;
            LoadImageFormComic();
        }
        /// <summary>
        /// 从本地漫画加载图片
        /// </summary>
        private void LoadImageFormComic()
        {
            if(Comic.Affiliation == "Local")
            {
                Episodes = DBHelper.Db.Queryable<LocalEpisode>().Where(x => x.ComicId == Comic.Id).OrderBy(x => x.Order).ToList();
                if (Episodes.Count > 0)
                {
                    Log.Information(Episodes[0].Id);
                    foreach (LocalPicture item in DBHelper.Db.Queryable<LocalPicture>().Where(x => x.EpisodeId == Episodes[0].Id).OrderBy(x => x.Name).ToList())
                    {
                        BitmapImage image = new BitmapImage();
                        image.UriSource = new Uri(item.Img);
                        Images.Add(image);
                    }
                }
            }
        }
    }
}
