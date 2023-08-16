using Microsoft.UI.Xaml.Controls;
using ShadowViewer.Interfaces;
using ShadowViewer.Plugin.Local.Enums;

namespace ShadowViewer.Plugin.Local.Models;

public class LocalSearchItem : IShadowSearchItem
{
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public string Id { get; set; }
    public string ComicId { get; set; }
    public IconSource Icon { get; set; }
    public LocalSearchMode Mode { get; set; }

    public LocalSearchItem(string title, string id,string comicId, LocalSearchMode mode)
    {
        Title = title;
        Mode = mode;
        switch (mode)
        {
            case LocalSearchMode.SearchComic:
                SubTitle = "本地漫画";
                Icon = new FontIconSource() { Glyph = "\uE82D" };
                break;
            case LocalSearchMode.SearchTag:
                SubTitle = "本地标签";
                break;
            default:
                break;
        }

        ComicId = comicId;
        Id = id;
    }
}