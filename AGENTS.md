# Repository Guidelines

## Project Structure & Module Organization

ShadowViewer is a WinUI 3 manga reader.

- `ShadowViewer/`: application entry points, XAML `Pages/`, MVVM `ViewModels/`, `Services/`, `Models/`, and `Converters/`.
- `ShadowViewer/Assets/`, `Themes/`, and `Strings/zh-CN/`: images, styles, and localized resources.
- `ShadowViewer.Sdk/`, `ShadowViewer.Controls/`, `ShadowViewer.Plugin.Local/`, and `ShadowViewer.Plugin.PluginManager/`: Git submodules providing shared APIs, controls, the local reader, and plugin management.
- `ShadowViewer.Plugin.Local/tests/Reader.Tests/`: reader regression harness; `ShadowViewer.Controls/Test/`: interactive WinUI control demo.
- `.github/workflows/`: Windows builds and MSIX release packaging.

## Build, Test, and Development Commands

Use Windows, Visual Studio 2022+ with WinUI/MSIX tooling, and Windows SDK 10.0.22621+. Projects target .NET 8; CI uses the .NET 9 SDK. Run from the repository root in Visual Studio Developer PowerShell:

```powershell
git submodule update --init --recursive
msbuild ShadowViewer/ShadowViewer.csproj /restore /p:Configuration=Debug /p:Platform=x64 /p:GithubAction=false
dotnet run --project ShadowViewer.Plugin.Local/tests/Reader.Tests/Reader.Tests.csproj
```

These initialize dependencies, restore/build the application, and run reader checks. Debug in `ShadowViewer.sln`: select `ShadowViewer` as the startup project, choose Debug/x64, and press F5. Obtain `../ShadowViewer.Plugin.Bika/` or unload its solution project.

## Coding Style & Naming Conventions

Follow existing C#/XAML formatting: four-space indentation, C# braces on separate lines, and file-scoped namespaces. Use PascalCase for types, methods, and properties; camelCase for parameters/locals. Preserve suffixes such as `SettingsPage`, `NavigationViewModel`, and `NavigateService`. Keep business logic in view models/services. Nullable references are enabled. `.editorconfig` silences CA1416; no dedicated formatter/linter is configured.

## Testing Guidelines

The reader harness uses custom assertions/adapters with descriptive scenario names in `Program.cs` and `ImageLoadingChecks.cs`. Nonzero exit means failure. Add reader regression checks; no coverage percentage is enforced. Verify gestures, rendering, and native image decoding in a Windows reader window.

## Commit & Pull Request Guidelines

Use observed commit patterns: `Feat | description`, `Fix | description`, `Style | description`, or `Deps | description`. Commit submodule changes separately before updating root pointers. Keep PRs focused; explain behavior changes, link relevant issues, report validation, and include screenshots for UI changes. Keep signing certificates, tokens, and generated build/package outputs out of commits.

## Release Workflow

1. Update `<Version>` in the released project's `.csproj` and affected package references. For application releases, also update `ShadowViewer/Package.appxmanifest`, which supplies CI's version.
2. Add the new version's changes and component versions at the top of `CHANGELOG.md`.
3. Commit as `Release | <version>` and tag it: `git tag <version>`.
4. Push both: `git push origin master`, then `git push origin <version>`. CI automatically builds packages and publishes releases. Stable tags must match `.github/workflows/build.yml`; preview releases follow new manifest versions pushed to `master` via `debug_build.yml`.
