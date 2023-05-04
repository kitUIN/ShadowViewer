namespace ShadowViewer.Utils
{
    public class ShadowPath
    {
        private string name;
        public string Name { get => name; }
        public ShadowPath Parent { get => parent; }
        
        private bool isFile;
        public bool IsFile { get => isFile; }
        public bool IsFolder { get => !isFile; }

        public List<ShadowPath> Children { get; } = new List<ShadowPath>();
        private ShadowPath parent;

        public ShadowPath(string name, bool isFile, ShadowPath parent)
        {
            this.name = name;
            this.isFile = isFile;
            this.parent = parent;
            InitChildren();
        }
        public void InitChildren()
        {
            var children = ComicDB.Get("Parent", name);
            foreach (var child in children)
            {
                Children.Add(new ShadowPath(child.Name, !child.IsFolder, this));
            }
        }
        /*public static ShadowPath CreateFromUri(Uri uri)
        {

        }*/
        public ShadowPath Combine(ShadowPath newPath)
        {
            Children.Add(newPath);
            return this;
        }
        public override string ToString()
        {
            var list = new List<string>();
            var path = this;
            while(path != null)
            {
                list.Add(path.name);
                path = path.Parent;
            }
            list.Reverse();
            return string.Join("/", list);
        }
    }
}
