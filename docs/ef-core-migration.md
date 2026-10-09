# EF Core / SQLite 迁移计划

## 范围与顺序

1. SDK 移除 SqlSugarCore，使用 EF Core 8 SQLite；`ShadowDbContext` 管理 `ShadowTag`、`CacheZip`。
2. 本地插件使用 `LocalDbContext` 管理节点、详情、章节、图片、作者、标签关联、历史和缓存。共用原来的 `ApplicationData.Current.LocalFolder/ShadowViewer.sqlite`。
3. DryIoc 只注册单例 `IDbContextFactory<T>`；服务、视图模型、响应器每次操作创建并释放上下文，不跨线程共享 DbContext。
4. 把查询、导航加载、更新、删除、插入和 upsert 改为 EF LINQ / SaveChanges / ExecuteUpdate / ExecuteDelete。图插入和批量导入使用事务；不再把 UI 模型 LocalComic 当数据库表更新。
5. SDK 和本地插件分别提交初始迁移及 ModelSnapshot，使用独立的迁移历史表。应用启动先升级 SDK，再升级本地插件，最后加载插件/UI。
6. 用独立 SQLite 回归程序验证新库、旧库接管、重复启动、增量迁移、关系、事务、DryIoc 工厂及并发操作；再构建 WinUI 应用并运行 reader harness。

## 旧库接管及回滚

- 接管保留表名、列名、已有 long ID、复合键和 JSON 文本；新的 long ID 使用独立的 Snowflake 生成器，保持预览对象在入库前已有 ID。原 SqlSugar 会保存计算属性 `ComicNode.IsFolder` 和 `CacheImg.Path`，这两列保留为 EF 隐式属性。
- 对有数据表且存在待执行迁移的库，用 SQLite 在线备份 API 保存完整一致的备份（包含 WAL 中的数据），命名为 `ShadowViewer.sqlite.<Context>.<UTC时间>.<GUID>.bak`。
- 无 EF 历史的旧表按初始迁移的实际建表 SQL 重建，在同一 SQLite 事务中拷贝数据、建立键/索引并记录初始迁移。保留其他插件的表。
- 不认识的列、重复复合键、缺失必需数据或失效外键导致升级失败，事务回滚；禁止静默丢弃数据。缺失的列仅在 SQLite 默认值/可空性允许时补齐。
- 初始迁移以外的升级通过 `Database.Migrate()` 顺序执行；任何失败阻止进入阅读器。恢复时关闭应用，将备份复制回原数据库路径，并清理旧的 `-wal` / `-shm` 文件后启动兼容版本。
- 不调用 `EnsureCreated()`，不根据应用版本号临时改表，不删除已发布的迁移。
- 如果迁移历史包含当前组件不认识的新迁移，拒绝用旧程序打开较新的数据库。

## 后续结构升级

修改实体与 Fluent 配置后，运行 `dotnet ef migrations add <名称> --context ShadowDbContext` 或 `--context LocalDbContext`，检查 Up/Down 和 snapshot，添加旧版本升级检查，再随组件发布。SDK 的表在本地上下文中 `ExcludeFromMigrations()`，只由 SDK 迁移管理。

SDK 数据库 API 从 SqlSugar 改为 EF 工厂属于破坏性变更：SDK 升到 4.0.0，本地插件 2.0.0，插件管理器 2.0.0；依赖 SqlSugar API 的第三方插件需要重新编译迁移。

发布顺序：先分别提交、发布 SDK 4.0.0、本地插件 2.0.0、插件管理器 2.0.0，再提交主仓库的子模块指针及依赖更新。稳定版 CI 使用 `DepsUseNuget=true`，必须等这些 NuGet 包可用后再发布主应用。此次主应用 manifest 的 `Identity.Version` 为 `0.5.9.0`，CI 映射为 `0.6-Preview9`，使用同名发布标签。

## 验证命令

```powershell
dotnet run --project ShadowViewer.Plugin.Local/tests/Database.Tests/Database.Tests.csproj
dotnet run --project ShadowViewer.Plugin.Local/tests/Reader.Tests/Reader.Tests.csproj
msbuild ShadowViewer/ShadowViewer.csproj /restore /p:Configuration=Debug /p:Platform=x64 /p:GithubAction=false
```

数据库回归项目直接编译生产数据库源码（仅 UI 依赖使用适配器），也可用来生成迁移，不需要启动 WinUI：

```powershell
dotnet ef migrations add <名称> --project ShadowViewer.Plugin.Local/tests/Database.Tests --context ShadowDbContext --output-dir ../../../ShadowViewer.Sdk/ShadowViewer.Sdk/Database/Migrations --namespace ShadowViewer.Sdk.Database.Migrations
dotnet ef migrations add <名称> --project ShadowViewer.Plugin.Local/tests/Database.Tests --context LocalDbContext --output-dir ../../ShadowViewer.Plugin.Local/Database/Migrations --namespace ShadowViewer.Plugin.Local.Database.Migrations
```

使用版本匹配的 dotnet-ef 8.0.31。首次通过回归项目生成 migration 时，EF 工具可能把 ModelSnapshot 放到回归项目的命名空间目录；将它移到对应生产 `Database/Migrations` 目录。已有 snapshot 之后，工具会沿用其目录。不要给回归项目显式设置生产 MigrationsAssembly：生产数据库源码通过 Compile 链接编译到回归程序集；上线时自动使用对应生产程序集。

## 验证结果

- 真实旧库样本由迁移前源码和 SqlSugarCore 5.1.4.211 生成，保存为纯 SQL fixture；新程序和回归项目都不依赖 SqlSugar。
- 数据库回归覆盖新库、重复启动、旧库、DryIoc 并发工厂、图插入和共享关系、JSON 变更跟踪、decimal 进度、失败回滚、WAL 备份和增量迁移。
- 已运行 Debug/x64 WinUI 构建和 166 项 reader harness；手动阅读器窗口、原生图像解码以及 MSIX 发布安装需要在发布前另行验证。
