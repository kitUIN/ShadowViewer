﻿namespace ShadowViewer.Models;

public class NavigateSearchItem : IShadowSearchItem
{
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public string Id { get; set; } = "";
    public IconSource Icon { get; set; } = new FontIconSource() { Glyph = "\uE774" };

    public NavigateSearchItem(string title)
    {
        if (title.StartsWith("shadow://"))
            Title = title;
        else
            Title = "shadow://" + title;
        SubTitle = ShadowResourcesHelper.GetString(ResourceKey.Navigate);
    }
}