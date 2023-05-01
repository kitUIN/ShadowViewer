namespace ShadowViewer.Messages
{
    public class FilesMessage
    {
        public object[] objects;
        public FilesMessage(params object[] objects)
        {
            this.objects = objects;
        }
    }
}
