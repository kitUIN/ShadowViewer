using Microsoft.UI.Xaml.Markup;
using ShadowViewer.Enums;
using ShadowViewer.Helpers;
using ShadowViewer.Plugin.Local.Enums;
using ShadowViewer.Plugin.Local.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowViewer.Plugin.Local.Extensions
{ 
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    internal sealed class LocalLocaleExtension : MarkupExtension
    {

        /// <summary>
        /// 键值
        /// </summary>
        public LocalResourceKey Key { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            return LocalResourcesHelper.GetString(Key);
        }
    }
}
