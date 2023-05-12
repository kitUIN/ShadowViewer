namespace ShadowViewer.Helpers
{
    public static class TagsHelper
    {
        public static List<ShadowTag> ShadowTags = new List<ShadowTag>();
        public static void Init()
        {
            var tags = TagDB.GetAll();
            if(!tags.Any(x => x.tag == "Local"))
            {
                ShadowTag local = new ShadowTag("Local", I18nHelper.GetString("Shadow.Tag.Local"), "#000000", "#ffd657");
                TagDB.Add(local);
                tags.Add(local);
            }
            ShadowTags = tags;
        }
        public static void UpdateName(this ShadowTag shadowTag, string newName)
        {
            TagDB.Update("Name", "Tag", newName, shadowTag.tag);
            if(ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is  ShadowTag tag)
            {
                tag.name = newName;
            }
            shadowTag.name = newName;
        }
        public static void UpdateId(this ShadowTag shadowTag, string newId)
        {
            TagDB.Update("Tag", "Tag", newId, shadowTag.tag);
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                tag.tag = newId;
            }
            shadowTag.tag = newId;
        }
        public static void UpdateForeground(this ShadowTag shadowTag, string newForeground)
        {
            TagDB.Update("Foreground", "Tag", newForeground, shadowTag.tag);
            shadowTag.foreground = new SolidColorBrush(newForeground.ToColor());
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                tag.foreground = shadowTag.foreground;
            }
        }
        public static void UpdateBackground(this ShadowTag shadowTag, string newBackground)
        {
            TagDB.Update("Background", "Tag", newBackground, shadowTag.tag);
            shadowTag.background = new SolidColorBrush(newBackground.ToColor());
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                tag.background = shadowTag.background;
            }
        }
        public static void Remove(this ShadowTag shadowTag)
        {
            TagDB.Remove("Tag", shadowTag.tag);
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                ShadowTags.Remove(tag);
            }
        } 
    }
}
