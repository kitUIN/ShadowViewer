namespace ShadowViewer.Messages
{
    public class StatusMessage
    {
        public object[] objects;
        public StatusMessage(params object[] objects)
        {
            this.objects = objects;
        }
    }
}
