namespace ShadowViewer.Helpers
{
    public static class TagsHelper
    {
        public static Dictionary<string, ShadowTag> Affiliations = new Dictionary<string,ShadowTag>();
        public static List<ShadowTag> ShadowTags = new List<ShadowTag>();
         
        public static void Init()
        {
            Affiliations["Local"] = new ShadowTag(I18nHelper.GetString("Shadow.Tag.Local"), "#000000", "#ffd657");
            //TODO:插件Affiliations注入
            ShadowTags = TagDB.GetAll();
        }
    }
}
