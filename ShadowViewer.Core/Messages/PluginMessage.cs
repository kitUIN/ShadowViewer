namespace ShadowViewer.Messages
{
    public class PluginMessage
    {
        public object[] objects;
        public PluginMessage(params object[] objects)
        {
            this.objects = objects;
        }
    }
}
