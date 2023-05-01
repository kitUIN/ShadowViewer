namespace ShadowViewer.Models
{
    public class SearchResult
    {
        /// <summary>
        /// 图标
        /// </summary>
        public UIElement Icon { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public object Source { get; set; }
        public SearchResult(UIElement icon, string name, string method,object source)
        {
            this.Icon = icon;
            this.Name = name;
            this.Method = method;
            this.Source = source;
        }
    }
}
