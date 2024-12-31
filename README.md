<div align="right">
🌍<a href="https://github.com/kitUIN/ShadowViewer/blob/master/README_EN.md">English</a> / 中文
</div>
<div align="center">

# ShadowViewer

✨使用Windows App SDK 构建的漫画阅读器✨

</div>

<p align="center">
  <a href="https://github.com/kitUIN/ShadowViewer/blob/master/LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-green" alt="license">
  </a>
  <a href="https://github.com/kitUIN/ShadowViewer/releases">
    <img src="https://img.shields.io/github/v/release/kitUIN/ShadowViewer" alt="release">
  </a>
    <a href="https://github.com/kitUIN/ShadowViewer.Core/releases">
    <img src="https://img.shields.io/badge/Core-2024.12.31.6-8A2BE2" alt="chajian">
  </a>
</p> 
<p align="center">
  <a href="https://shadowviewer.kituin.fun/">📖文档</a>
  ·
  <a href="https://github.com/kitUIN/ShadowViewer/issues/new/choose">🐛提交建议</a>
</p>

## 安装 ▶️

- [侧加载安装教程](https://shadowviewer.kituin.fun/wiki/shadowviewer/use/install/#%E5%BE%AE%E8%BD%AF%E5%95%86%E5%BA%97-%E6%9C%AA%E6%94%AF%E6%8C%81)

## 插件列表🔩
| ID | 名称                                    |       简述           | 开发者  | 链接                    |最新版本 |
|--------------------------------|--------------------------------|---------|-----------------------|-----------------------|----|
| Local | 本地阅读器 | 支持本地阅读漫画             | [kitUIN](https://github.com/kitUIN) | [内置](https://github.com/kitUIN/ShadowViewer.Plugin.Local) | - |
| PluginManager | 插件管理器 | 本地管理插件             | [kitUIN](https://github.com/kitUIN) | [内置](https://github.com/kitUIN/ShadowViewer.Plugin.PluginManager) | - |
| Bika | 哔咔漫画 | 适配哔咔漫画             | [kitUIN](https://github.com/kitUIN) | [Github](https://github.com/kitUIN/ShadowViewer.Plugin.Bika) |![bika](https://img.shields.io/github/v/release/kitUIN/ShadowViewer.Plugin.Bika?color=blue&include_prereleases)|

## 导入插件📦
将插件项目的发行版压缩包下载后,在`插件管理器`页面选择`添加插件`进行导入

## 参与贡献🥰

- 环境要求
  - SDK: .NET 6.0及以上
  - IDE: Visual Studio 2022及以上
  - C# Version: 12

- 项目初始化
```bash
git clone https://github.com/kitUIN/ShadowViewer.git
cd ShadowViewer
git submodule init
git submodule update
```

- 按照我们的开发准则(施工中)进行开发

## 依赖 📂
[ShadowViewer.Core](https://github.com/kitUIN/ShadowViewer.Core) - 核心功能  
[ShadowViewer.Analyzer](https://github.com/kitUIN/ShadowViewer.Analyzer) - 源代码生成器  
[ShadowPluginLoader.WinUI](https://github.com/kitUIN/ShadowPluginLoader.WinUI) Windows App SDK插件加载器  
[Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - Windows App SDK  
[Windows Community Toolkit](https://github.com/CommunityToolkit/dotnet)/[Windows Community Toolkit Labs](https://github.com/CommunityToolkit/Labs-Windows) - 控件及其他帮助类  
[SQLSugarCore](https://github.com/DotNetNext/SqlSugar) - ORM框架  
[Serilog](https://serilog.net) - 日志系统  
[SharpCompress](https://github.com/adamhathcock/sharpcompress) - 提供压缩解压支持  
[FluentIcon](https://github.com/KitUIN/FluentIcon) - FluentIcon图标  
[DryIoc](https://github.com/dadhi/DryIoc) - 依赖注入

## 感谢以下项目 ❤️
[WinUI-Gallery](https://github.com/microsoft/WinUI-Gallery) - WinUI3示例 - 参考了部分控件编写  
