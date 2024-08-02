<div align="right">
🌍English / <a href="https://github.com/kitUIN/ShadowViewer/blob/master/README.md">中文</a> 
</div>
<div align="center">

# ShadowViewer

✨Manga Reader built with Windows App SDK✨

</div>

<p align="center">
  <a href="https://github.com/kitUIN/ShadowViewer/blob/master/LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-green" alt="license">
  </a>
  <a href="https://github.com/kitUIN/ShadowViewer/releases">
    <img src="https://img.shields.io/github/v/release/kitUIN/ShadowViewer" alt="release">
  </a>
    <a href="https://github.com/kitUIN/ShadowViewer.Core/releases">
    <img src="https://img.shields.io/badge/Core-20230821-8A2BE2" alt="chajian">
  </a>
</p> 
<p align="center">
  <a href="https://shadowviewer.kituin.fun/">📖Docs</a>
  ·
  <a href="https://github.com/kitUIN/ShadowViewer/issues/new/choose">🐛Submit Issues</a>
</p>

## Install ▶️

- [Sideloading Installation Tutorial](https://shadowviewer.kituin.fun/wiki/shadowviewer/use/install/#%E5%BE%AE%E8%BD%AF%E5%95%86%E5%BA%97-%E6%9C%AA%E6%94%AF%E6%8C%81)

## Plugin List🔩
| ID | Name                                    |       Summarize           | Author  | Link                    | Latest version |
|--------------------------------|--------------------------------|---------|-----------------------|-----------------------|----|
| Local | Local Manga Reader | Support local reading of manga             | [kitUIN](https://github.com/kitUIN) | [BuiltIn](https://github.com/kitUIN/ShadowViewer.Plugin.Local) | - |
| PluginManager | Plugin Manager | Local Plugin Manager | [kitUIN](https://github.com/kitUIN) | [BuiltIn](https://github.com/kitUIN/ShadowViewer.Plugin.PluginManager) | - |
| Bika | Bika Comic | Support Bika             | [kitUIN](https://github.com/kitUIN) | [Github](https://github.com/kitUIN/ShadowViewer.Plugin.Bika) |![bika](https://img.shields.io/github/v/release/kitUIN/ShadowViewer.Plugin.Bika?color=blue&include_prereleases)|

## Import Plugin📦
                                      
After downloading the release-zip of the plugin project, select `Add Plugin` on the `Plugin Manager` page to import it.

## Contributions🥰

- Environmental requirements
  - SDK: .NET 6.0and above
  - IDE: Visual Studio 2022 and above
  - C# Version: 12

- Initialization
```bash
git clone https://github.com/kitUIN/ShadowViewer.git
cd ShadowViewer
git submodule init
git submodule update
```

- Developed in accordance with our development guidelines (in progress)

## Dependencies 📂
[ShadowViewer.Core](https://github.com/kitUIN/ShadowViewer.Core) - Core Functions  
[ShadowViewer.Analyzer](https://github.com/kitUIN/ShadowViewer.Analyzer) - Source Code Generator  
[ShadowPluginLoader.WinUI](https://github.com/kitUIN/ShadowPluginLoader.WinUI) Windows App SDK Plugin Loader  
[Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - Windows App SDK  
[Windows Community Toolkit](https://github.com/CommunityToolkit/dotnet)/[Windows Community Toolkit Labs](https://github.com/CommunityToolkit/Labs-Windows) - Controls and other helper classes  
[SQLSugarCore](https://github.com/DotNetNext/SqlSugar) - ORM  
[Serilog](https://serilog.net) - Logger  
[SharpCompress](https://github.com/adamhathcock/sharpcompress) - Supports compression and decompression  
[FluentIcon](https://github.com/KitUIN/FluentIcon) - FluentIcon  
[DryIoc](https://github.com/dadhi/DryIoc) - dependency injection

## Thanks ❤️
[WinUI-Gallery](https://github.com/microsoft/WinUI-Gallery) - WinUI3Gallery - Partially written controls are referenced  
