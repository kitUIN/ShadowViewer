using System.Xml.Linq;

namespace ShadowViewer.Utils
{
    public class ShadowPath
    { 
        private LocalComic comic;
        public string Name { get => comic.Name; }
        public string Id { get => comic.Id; }
        public string Img { get => comic.Img; }
        public bool IsFolder { get => comic.IsFolder; }
        public List<ShadowPath> Children { get; } = new List<ShadowPath>();
        public ShadowPath(LocalComic comic)
        {
            this.comic = comic;
        }

        public ShadowPath(List<string> black)
        {
            this.comic = new LocalComic("local", "local", "", "", "local", img: "ms-appx:///Assets/Default/folder.png");
            var children = ComicDB.Get(new Dictionary<string, object>()
            {
                {"Parent", "local"},
                {"IsFolder", true},
            });
            children.RemoveAll(x => black.Contains(x.Id));
            foreach (var child in children)
            {
                Children.Add(new ShadowPath(child));
            }
        } 
    }
}
