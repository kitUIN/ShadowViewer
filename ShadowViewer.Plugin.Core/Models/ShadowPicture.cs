using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ShadowViewer.Interfaces;
using System;

namespace ShadowViewer.Plugin.Core.Models;

public partial class ShadowPicture : ObservableObject, IShadowPicture
{
    [ObservableProperty] private int index;
    [ObservableProperty] private ImageSource source;

    public ShadowPicture(int index, BitmapImage image)
    {
        Index = index;
        Source = image;
    }

    public ShadowPicture(int index, Uri uri) : this(index, new BitmapImage() { UriSource = uri })
    {
    }

    public ShadowPicture(int index, string uri) : this(index, new BitmapImage() { UriSource = new Uri(uri) })
    {
    }
}