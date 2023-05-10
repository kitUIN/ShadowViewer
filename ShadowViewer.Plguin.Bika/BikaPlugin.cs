// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.
using ShadowViewer.Models;
using ShadowViewer.Plguin.Bika;

namespace ShadowViewer.Plugin.Bika
{
    public class BikaPlugin : IPlugin
    {
        public static readonly I18nHelper i18NHelper = new I18nHelper("ShadowViewer.Plugin.Bika/Resources/");
        public static PluginMetaData BikaMetaData { get; } = new PluginMetaData(
            "Bika", "��������",
                "��������������",
                "kitUIN", "0.1.0",
                new Uri("https://github.com/kitUIN/ShadowViewer/tree/master/ShadowViewer.Plguin.Bika/README.md"),
                new Uri("ms-appx://ShadowViewer.Plguin.Bika/Assets/Icons/logo.png"), 1);
         
        public void Init()
        {
            throw new NotImplementedException();
        }
        public PluginMetaData MetaData()
        {
            return BikaMetaData;
        }

        public void NavigationViewItemsHandler(NavigationViewItem navItem)
        {
            navItem.MenuItems.Add(new NavigationViewItem
            {
                Content = i18NHelper.Get("Bika.NavigationItem.Title"),
                Icon = XamlHelper.CreateImageIcon(BikaMetaData.Logo),
                Tag = BikaMetaData.ID,
            });
        }

        public Type NavigationPage()
        {
            return typeof(BikaHomePage);
        }

        public Type NavigationViewItemInvokedHandler(string tag)
        {
            if(tag == BikaMetaData.ID)
                return typeof(BikaHomePage);
            return null;
        }

        public void PluginSettingsExpander(SettingsExpander expander)
        {
            SettingsCard webUri = new SettingsCard
            {
                Header = i18NHelper.Get("Bika.WebUriSettingsCard.Title"),
                HeaderIcon = XamlHelper.CreateBitmapIcon("ms-appx://ShadowViewer.Plguin.Bika/Assets/Icons/github.png"),
                Description = "GitHub@" + BikaMetaData.Author,
                IsClickEnabled = true,
                ActionIcon = XamlHelper.CreateFontIcon("\uE8A7"),
                Tag = true,
            };
            webUri.Click += (s, e) =>
            {
                BikaMetaData.WebUri.LaunchUriAsync();
                
            };
            expander.Items.Add(webUri);

        }

        public ShadowTag PluginTagHandler(string tag)
        {
            throw new NotImplementedException();
        }
 
         
 
        public void NavigationViewItemInvokedHandler(string tag, out Type _page, out object parameter)
        {
            _page = null;
            parameter = null;
        }
    }
}
