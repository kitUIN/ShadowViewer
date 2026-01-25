using DryIoc.ImTools;
using FluentIcons.Common;
using FluentIcons.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Text.RegularExpressions;

namespace ShadowViewer.Converters;

public class StringToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, string language)
    {
        if (value is not string valueString) return null;
        var uri = new Uri(valueString);
        string glyph;
        switch (uri.Scheme)
        {
            case "ms-appx" or "http" or "https":
                return new BitmapIcon
                {
                    UriSource = uri
                };
                ;
            case "font":
                glyph = valueString.Replace("font://", "");
                if (glyph.StartsWith("\\")) glyph = Regex.Unescape(glyph);
                return new FontIcon()
                {
                    Glyph = glyph
                };

            case "symbol":
                glyph = valueString.Replace("symbol://", "");
                return new Microsoft.UI.Xaml.Controls.SymbolIcon(
                    Enum.Parse<Microsoft.UI.Xaml.Controls.Symbol>(glyph, ignoreCase: true));
            case "fluent":
                switch (uri.Host)
                {
                    case "regular":
                        glyph = valueString.Replace("fluent://regular/", "");
                        if (glyph.StartsWith("\\")) glyph = Regex.Unescape(glyph);
                        return new FluentIcon()
                        {
                            IconVariant = IconVariant.Regular,
                            Glyph = glyph,
                        };
                    case "filled":
                        glyph = valueString.Replace("fluent://filled/", "");
                        if (glyph.StartsWith("\\")) glyph = Regex.Unescape(glyph);
                        return new FluentIcon()
                        {
                            IconVariant = IconVariant.Filled,
                            Glyph = glyph,
                        };
                }

                break;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}