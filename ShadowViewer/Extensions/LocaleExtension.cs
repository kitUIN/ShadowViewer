using Microsoft.UI.Xaml.Markup;
namespace ShadowViewer.Extensions
{
    /// <summary>
    /// 多语言本地化
    /// </summary>
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    internal sealed class LocaleExtension : MarkupExtension
    {

        /// <summary>
        /// 键值
        /// </summary>
        public ResourceKey Key { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            return ShadowResourcesHelper.GetString(Key);
        }
    }
}