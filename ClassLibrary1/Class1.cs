// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.


using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.ApplicationModel.Resources;
using Serilog;
using ShadowViewer.Core.Plugins;
using ShadowViewer.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace ClassLibrary1
{
    public class Class1 : IPlugin
    {
        public static ResourceManager resourceManager;
        public void test(string tt)
        {
            var stri = new ResourceManager(tt).MainResourceMap.GetValue("ClassLibrary1/Resources/String1");
            Log.Information(stri.ValueAsString);
        }

        public void Init()
        {
            Log.Information("Hello World");
        }

        public PluginMetaData MetaData() => new PluginMetaData()
        {
            Name = "Test",
            Author = "kituin",
            Description = "Test Description",
            WebUri = "",
            RequireVersion = 1,
            Version = "1.0.0"
        };

        public void Init(string priPath)
        {
            resourceManager = new ResourceManager(Path.Combine(priPath, MetaData().ID + ".pri"));
        }

        public Type Page()
        {
            return typeof(BlankPage1);
        }
    }
}
