namespace ShadowViewer.Utils
{
    public class ShadowPath
    {
        public List<string> paths;
        public ShadowPath(string path)
        {
            paths = new List<string>(path.Split('/'));
             
        }
        public ShadowPath Combine(string newPath)
        {
            paths.Add(newPath);
            return this;
        }
        public ShadowPath Pre()
        {
            paths.RemoveAt(paths.Count - 1);
            return this;
        }
        
        public override string ToString()
        {
            return string.Join("/", paths);
        }
    }
}
