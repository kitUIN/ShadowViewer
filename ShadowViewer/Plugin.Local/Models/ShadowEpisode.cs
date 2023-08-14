namespace ShadowViewer.Plugin.Local.Models;

public partial class ShadowEpisode : ObservableObject, IShadowEpisode
{
    [ObservableProperty] private object source;

    [ObservableProperty] private string title;
    public object Tag { get; set; }
}