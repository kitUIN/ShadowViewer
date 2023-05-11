namespace ShadowViewer.Utils
{
    public class ShadowPath
    { 
        private LocalComic comic;
        public string Name { get => comic.Name; }
        public string Id { get => comic.Id; }
        public string Img { get => comic.Img; }
        public bool IsFolder { get => comic.IsFolder; }
        public List<ShadowPath> Children { get; } 
        public ShadowPath(LocalComic comic)
        {
            this.comic = comic;
            Children = new List<ShadowPath>();
        }

        public ShadowPath(IEnumerable<string> black)
        {
            this.comic = new LocalComic("local", "local", "", "", "local", img: "ms-appx:///Assets/Default/folder.png");
            var children = ComicDB.Get(new Dictionary<string, object>()
            {
                {"Parent", "local"},
                {"IsFolder", true},
            });
            Children = children.Where(c => !black.Contains(c.Id)).Select(c => new ShadowPath(c)).ToList();
        }
    }
}
