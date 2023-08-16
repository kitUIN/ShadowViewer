using CommunityToolkit.Mvvm.ComponentModel;
using ShadowViewer.Interfaces;
using ShadowViewer.Models;

namespace ShadowViewer.Plugin.Local.Models;

public partial class ShadowEpisode : ObservableObject, IShadowEpisode
{
    public LocalEpisode Source { get; set; }

    [ObservableProperty] private string title;

    public ShadowEpisode(LocalEpisode episode)
    {
        Source = episode;
        Title = episode.Name;
    }
}