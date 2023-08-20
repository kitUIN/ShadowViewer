using Microsoft.Windows.ApplicationModel.Resources;
using ShadowViewer.Enums;
using ShadowViewer.Plugin.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowViewer.Plugin.Core.Helpers
{
    /// <summary>
    /// 本地化 帮助类
    /// </summary>
    internal static class LocalResourcesHelper
    {
        private static readonly ResourceManager resourceManager = new ResourceManager();
        private static readonly string prefix = "ShadowViewer.Plugin.Core/Resources/";
        public static string GetString(string key)
        {
            return resourceManager.MainResourceMap.GetValue(prefix + key).ValueAsString;
        }
        public static string GetString(LocalResourceKey key)
        {
            return GetString(key.ToString());
        }
    }
}
