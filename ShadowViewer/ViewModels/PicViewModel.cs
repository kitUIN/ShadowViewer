using SqlSugar;

namespace ShadowViewer.ViewModels
{
    public partial class PicViewModel : ObservableObject
    {
        
        private static ILogger Logger = Log.ForContext<PicViewModel>();
        public ObservableCollection<BitmapImage> Images { get; set; } = new ObservableCollection<BitmapImage>();
        public LocalComic Comic { get; private set; }
        [ObservableProperty]
        private int currentEpisodeIndex;
        public ObservableCollection<LocalEpisode> Episodes { get;   } = new ObservableCollection<LocalEpisode>();
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
            var Db =  DiFactory.Services.Resolve<ISqlSugarClient>();
            if(Comic.Affiliation == "Local")
            {
                Episodes.Clear();
                Db.Queryable<LocalEpisode>().Where(x => x.ComicId == Comic.Id).OrderBy(x => x.Order).ForEach(x =>
                {
                    Episodes.Add(x);
                });
                if (Episodes.Count > 0)
                {
                    foreach (LocalPicture item in Db.Queryable<LocalPicture>().Where(x => x.EpisodeId == Episodes[CurrentEpisodeIndex].Id).OrderBy(x => x.Name).ToList())
                    {
                        BitmapImage image = new BitmapImage();
                        image.UriSource = new Uri(item.Img);
                        Images.Add(image);
                    }
                }
            }
        }
        partial void OnCurrentEpisodeIndexChanged(int oldValue, int newValue)
        {
             
        }
    }
}
