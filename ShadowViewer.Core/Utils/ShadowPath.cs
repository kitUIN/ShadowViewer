namespace ShadowViewer.Utils
{
    public class ShadowPath
    {
        public List<string> paths;
        public ShadowPath(string path)
        {
            paths = new List<string>(path.Split('/'));
             
        }
        public void Combine(string newPath)
        {
            paths.Add(newPath);
        }
        public void Pre()
        {
            paths.RemoveAt(paths.Count - 1);
        }
    }
}
