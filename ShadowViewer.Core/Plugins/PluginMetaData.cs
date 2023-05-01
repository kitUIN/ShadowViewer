namespace ShadowViewer.Plugins
{
    /// <summary>
    /// 插件元数据
    /// </summary>
    public class PluginMetaData
    {
        public string ID { get; }
        public string Name { get; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public Uri WebUri { get; set; }
        public Uri Logo { get; set; }
        public int MinVersion { get; set; }
        public PluginMetaData(string id, string name, string description, string author, string version, Uri webUri,Uri logo, int requireVersion)
        {
            ID = id;
            Name = name;
            Description = description;
            Author = author;
            Version = version;
            WebUri = webUri;
            Logo = logo;
            MinVersion = requireVersion;
        }
        public PluginMetaData() { }
    }
}
