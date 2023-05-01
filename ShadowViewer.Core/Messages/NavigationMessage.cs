namespace ShadowViewer.Messages
{
    public class NavigationMessage
    {
        public object[] objects;
        public NavigationMessage(params object[] objects)
        {
            this.objects = objects;
        }
    }
}
