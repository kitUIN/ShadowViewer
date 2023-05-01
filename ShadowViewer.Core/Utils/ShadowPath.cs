namespace ShadowViewer.Utils
{
    public class ShadowPath
    {
        public Collection<string> paths;
        public ShadowPath(string path)
        {
            paths = new Collection<string>(path.Split('/'));
        }
    }
}
