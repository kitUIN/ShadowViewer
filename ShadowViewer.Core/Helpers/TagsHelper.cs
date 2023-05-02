namespace ShadowViewer.Helpers
{
    public static class TagsHelper
    {
        public static List<ShadowTag> ShadowTags = new List<ShadowTag>();
        public static void Init()
        {
            var tags = DBHelper.GetAllShadowTags();
            if(!tags.Any(x => x.tag == "Tag.Local"))
            {
                ShadowTag local = new ShadowTag("Tag.Local", I18nHelper.GetString("Tag.Local"), "#000000", "#ffd657");
                DBHelper.AddShadowTag(local);
                tags.Add(local);
            }
            ShadowTags = tags;
        }
        public static void UpdateName(this ShadowTag shadowTag, string newName)
        {
            DBHelper.UpdateShadowTag("name", "tag", newName, shadowTag.tag);
            if(ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is  ShadowTag tag)
            {
                tag.name = newName;
            }
            shadowTag.name = newName;
        }
        public static void UpdateId(this ShadowTag shadowTag, string newId)
        {
            DBHelper.UpdateShadowTag("tag", "tag", newId, shadowTag.tag);
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                tag.tag = newId;
            }
            shadowTag.tag = newId;
        }
        public static void UpdateForeground(this ShadowTag shadowTag, string newForeground)
        {
            DBHelper.UpdateShadowTag("Foreground", "tag", newForeground, shadowTag.tag);
            shadowTag.foreground = new SolidColorBrush(newForeground.ToColor());
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                tag.foreground = shadowTag.foreground;
            }
        }
        public static void UpdateBackground(this ShadowTag shadowTag, string newBackground)
        {
            DBHelper.UpdateShadowTag("Background", "tag", newBackground, shadowTag.tag);
            shadowTag.background = new SolidColorBrush(newBackground.ToColor());
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                tag.background = shadowTag.background;
            }
        }
        public static void Remove(this ShadowTag shadowTag)
        {
            DBHelper.RemoveShadowTag("tag", shadowTag.tag);
            if (ShadowTags.FirstOrDefault(x => x.tag == shadowTag.tag) is ShadowTag tag)
            {
                ShadowTags.Remove(tag);
            }
        }
    }
}
