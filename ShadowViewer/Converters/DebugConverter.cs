using Microsoft.UI.Xaml.Data;
using System;
using ShadowViewer.I18n;

namespace ShadowViewer.Converters;

/// <summary>
/// 标题栏子标题Debug显示
/// </summary>
public class DebugConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? I18N.Debug : string.Empty;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}