using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowViewer.ViewModels
{
    public partial class AttributesViewModel : ObservableObject
    {
        [ObservableProperty]
        private double textBlockMaxWidth;
        public LocalComic CurrentComic { get; set; }
        public ObservableCollection<ShadowTag> Tags = new ObservableCollection<ShadowTag>();

        public AttributesViewModel(LocalComic currentComic)
        {
            CurrentComic = currentComic;
            ReLoadTags();
        }
        public void ReLoadTags()
        {
            Tags.Clear();
            if (TagsHelper.Affiliations[CurrentComic.Affiliation] is ShadowTag shadow)
            {
                shadow.IsEnable = false;
                Tags.Add(shadow);
            }
            foreach (string item in CurrentComic.Tags)
            {
                if (ShadowTag.Query().First(x => x.Name == item) is ShadowTag shadowTag)
                {
                    Tags.Add(shadowTag);
                }
            }
            Tags.Add(new ShadowTag
            {
                Icon = "\uE008",
                // Background = (SolidColorBrush)Application.Current.Resources["SystemControlBackgroundBaseMediumLowBrush"],
                Foreground = new SolidColorBrush((ThemeHelper.IsDarkTheme() ? "#FFFFFFFF" : "#FF000000").ToColor()),
                IsEnable = true,
                IsIcon = true,
                Name = "添加"
            });
        }
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
        public void RemoveTag(string name)
        {
            CurrentComic.Tags.Remove(name);
            ShadowTag.Remove(name);
            ReLoadTags();
        }
    }
}
