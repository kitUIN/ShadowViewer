using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ShadowViewer.Sdk.Helpers;

namespace ShadowViewer.Converters;
/// <summary>
/// 
/// </summary>
public class ThemeComboConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ElementTheme)
        {
            return (int)value;
        }

        return -1;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is >= 0 and <= 2)
        {
            return (ElementTheme)value;
        }
        return ThemeHelper.RootTheme;
    }
}