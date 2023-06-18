using CommunityToolkit.WinUI.Helpers;
using ShadowViewer.Models;
using ShadowViewer.ToolKits;

namespace ShadowViewer.ViewModels
{
    public partial class AttributesViewModel : ObservableObject
    {
        /// <summary>
        /// 最大文本宽度
        /// </summary>
        [ObservableProperty]
        private double textBlockMaxWidth;
        /// <summary>
        /// 当前漫画
        /// </summary>
        public LocalComic CurrentComic { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public ObservableCollection<ShadowTag> Tags = new ObservableCollection<ShadowTag>();
        /// <summary>
        /// 话
        /// </summary>
        public ObservableCollection<LocalEpisode> Episodes = new ObservableCollection<LocalEpisode>();
        /// <summary>
        /// 是否有话
        /// </summary>
        public bool IsHaveEpisodes { get=>Episodes.Count !=0; }
        public AttributesViewModel(LocalComic currentComic)
        {
            CurrentComic = currentComic;
            ReLoadTags();
            ReLoadEps();
        }
        /// <summary>
        /// 重新加载-话
        /// </summary>
        public void ReLoadEps()
        {
            foreach( var item in LocalEpisode.Query().Where(x => x.ComicId == CurrentComic.Id).ToList())
            {
                Episodes.Add(item);
            }
        }
        /// <summary>
        /// 重新加载-标签
        /// </summary>
        public void ReLoadTags()
        {
            Tags.Clear();
            if (TagsHelper.Affiliations[CurrentComic.Affiliation] is ShadowTag shadow)
            {
                shadow.IsEnable = false;
                shadow.Icon = "\uE23F";
                shadow.ToolTip = ResourcesToolKit.GetString("Shadow.String.Affiliation") + ": " + shadow.Name;
                Tags.Add(shadow);
            }
            foreach (string item in CurrentComic.Tags)
            {
                if (ShadowTag.Query().First(x => x.Name == item) is ShadowTag shadowTag)
                {
                    shadowTag.Icon = "\uEEDB";
                    shadowTag.ToolTip = ResourcesToolKit.GetString("Shadow.String.Tag") + ": " + shadowTag.Name;
                    Tags.Add(shadowTag);
                }
            }
            Tags.Add(new ShadowTag
            {
                Icon = "\uE008",
                // Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseMediumLowBrush"],
                Foreground = new SolidColorBrush((ThemeHelper.IsDarkTheme() ? "#FFFFFFFF" : "#FF000000").ToColor()),
                IsEnable = true,
                Name = ResourcesToolKit.GetString("Xaml.ToolTip.AddTag.Content"),
                ToolTip = ResourcesToolKit.GetString("Xaml.ToolTip.AddTag.Content"),
        });
        }
        /// <summary>
        /// 添加-标签
        /// </summary>
        public void AddNewTag(ShadowTag tag)
        {
            tag.Add();
            if (CurrentComic.Tags.Any(x => x == tag.Name))
            {
                CurrentComic = LocalComic.Query().First(x => x.Id == CurrentComic.Id);
            }
            else
            {
                CurrentComic.Tags.Add(tag.Name);
            }
            ReLoadTags();
        }
        /// <summary>
        /// 删除-标签
        /// </summary>
        public void RemoveTag(string name)
        {
            CurrentComic.Tags.Remove(name);
            ShadowTag.Remove(name);
            ReLoadTags();
        }
        /// <summary>
        /// 是否是最后一个标签
        /// </summary>
        public bool IsLastTag(ShadowTag tag)
        {
            return Tags.IndexOf(tag) == Tags.Count - 1;
        }
    }
}
