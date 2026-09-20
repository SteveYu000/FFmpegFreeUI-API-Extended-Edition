# FFmpegFreeUI API Extended Edition 工作约定

本文件适用于整个仓库。它只保存长期有效、每次工作都要遵守的规则；会随版本变化的事实记录在根目录的 `PROJECT_STATE.md`。

## 开始工作前

- 先阅读 `PROJECT_STATE.md`，再执行 `git status --short`、`git branch --show-current` 和 `git rev-parse --short HEAD`。
- 修改前先阅读相关源码、项目文件和现有测试。排查已发布程序或 DLL 时，还要核对实际加载路径、程序集版本和文件哈希，确认测试对象就是本次构建产物。
- 用户手动修改或删除的文件默认都是有意为之。不得擅自还原、覆盖、清理或把无关改动纳入当前任务。
- 工作树不干净时，只改当前任务必需的文件。不要使用 `git reset --hard`、`git checkout --` 或其他会丢失本地修改的命令。
- 复杂、跨多步或可能跨会话的任务，先给出可验证的计划，并在 `PROJECT_STATE.md` 的“当前工作”中记录目标、进度、验证结果和阻塞；完成后将其归档为简短结果。

## 项目身份与版本

- 项目名称为 `FFmpegFreeUI API Extended Edition`，无空格标识为 `FFmpegFreeUI-API-Extended-Edition`；主程序文件名仍为 `FFmpegFreeUI.exe`。
- `origin` 是扩展版仓库，`upstream` 是 Lake1059/FFmpegFreeUI 官方仓库。不要混淆推送目标。
- 产品版本格式为 `v{上游版本}-ext.{扩展修订号}+v{Ext API 版本}`。合并新的上游版本时通常从 `ext.1` 开始；仅修改扩展版时递增扩展修订号。
- Ext SDK 的 NuGet/FileVersion 跟随 API 版本。`FFmpegFreeUI.Ext.PluginSdk` 的 `AssemblyVersion` 当前固定为 `2.0.0.0` 以维持二进制兼容，除非用户明确批准破坏性升级，否则不得修改。
- `README.md` 中 `# 以下是上游仓库原内容` 之后必须与当前合入的上游 README 保持一致；扩展版说明只能写在该标记之前。

## API 与插件兼容性

- 官方插件 API 是首选；Ext API 只补充官方 API 尚未提供的能力。插件应能同时使用两者。
- 新增的扩展公开契约必须清楚归属于 Ext 命名空间/宿主，避免与未来官方事件、类型或成员重名。
- 保持已发布 Ext SDK 的源码和二进制兼容。优先给现有契约追加可选能力、默认实现或独立服务，不随意改签名、删除成员或更换已发布类型。
- 继续扩展统一的 `IExtFFmpegFreeUIHost`，不要自行恢复版本后缀宿主接口。若确实无法在兼容前提下实现，停止修改并向用户说明破坏点、影响范围和备选迁移方案。
- 改动插件发现、加载、启停、页面入口、设置页、参数面板、队列工具栏、事件派发或执行顺序时，必须同时检查官方插件与 Ext 插件路径。
- 公共 API 发生变化时，同步更新 XML 文档、示例插件和 `doc/Ext-Plugin-API-v2.zh-CN.md`。

## 合并上游

- 只有用户要求时才合并。合并前确认工作树、当前版本、`upstream/main` 新提交及双方差异，不要直接用整树覆盖。
- 逐项处理冲突：保留上游功能修复，同时重新应用扩展版 API、插件管理、更新逻辑和品牌差异。不得机械地全选 `ours` 或 `theirs`。
- 如果冲突会删除 Ext 公共契约、破坏旧插件兼容、丢失用户修改，或无法可靠判断行为，停止合并并报告冲突文件和决策点。
- 合并后更新产品版本、`PROJECT_STATE.md` 的上游基线，并核对 README 分界线后的内容。
- 网络失败时可临时为当前进程使用 `http://127.0.0.1:7897` 作为 HTTP/HTTPS 代理；不要把本机代理写入仓库或全局 Git 配置。

## LakeUI 与界面

- LakeUI 通过 NuGet `PackageReference` 管理；不要恢复本地 DLL `HintPath` 或把 LakeUI 二进制提交进仓库。当前版本见 `PROJECT_STATE.md`。
- 需要参与个性化背景映射的 Ext 页面，以名为 `ModernPanel1` 的 `LakeUI.ModernPanel` 作为底板容器，并遵守宿主的背景映射和页面生命周期。
- 保持 WinForms 设计器可打开：控件初始化和静态布局留在 `*.Designer.vb`，运行期逻辑放在对应代码文件；除非必要，不手工破坏设计器生成结构。
- 渲染问题必须从布局、父子关系、可见性、页面切换和背景合成生命周期解决。禁止用长延时、频繁 Timer、循环 `Invalidate` 或等待偶发重绘来掩盖问题。
- UI/LakeUI 相关改动不能只凭编译、Bounds、Visible 或 presenter 对象判断完成。应在实际构建的 FFmpegFreeUI 中验证；涉及背景时测试个性化背景开启和关闭，涉及切页/下拉/缩放时覆盖相应交互、窗口恢复和目标 DPI。
- 无法进行真实视觉验证时，明确记录未验证项和阻塞原因，不得声称界面问题已经完全修复。

## 构建与验证

在仓库根目录按改动范围执行；代码、项目或依赖变更至少通过完整构建和回归测试。

```powershell
dotnet restore "FFmpegFreeUI-API-Extended-Edition.slnx"
dotnet build "FFmpegFreeUI-API-Extended-Edition.slnx" -c Release --no-restore
dotnet run --project "tests/FFmpegFreeUI.RegressionTests/FFmpegFreeUI.RegressionTests.csproj" -c Release --no-build
git diff --check
```

- 完整构建的验收标准是 0 错误、0 警告；回归程序必须以退出代码 0 结束。
- 修改 SDK、插件宿主、插件加载或 LakeUI 交互后，还要构建旧插件兼容样例：

```powershell
dotnet build "E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj" -c Release
```

- 仅修改 Markdown 或 Solution Items 时，可用 `dotnet sln "FFmpegFreeUI-API-Extended-Edition.slnx" list` 加 `git diff --check` 做最小验证；不要把未运行的测试写成已通过。
- 发布框架依赖单文件时使用 `Tools/发布框架依赖单文件.ps1`；默认产物必须包含主程序以及 `FFmpegFreeUI.Ext.PluginHost.dll`、`FFmpegFreeUI.Ext.PluginSdk.dll`。

## 完成与交接

- 完成前检查 `git status --short` 和 `git diff --check`，只报告实际执行过的验证。
- 当上游基线、产品/API/LakeUI 版本、兼容结论、验证基线、已知问题或阻塞发生变化时，更新 `PROJECT_STATE.md`；只读答疑或没有改变这些事实的微小修改不必制造状态噪声。
- `PROJECT_STATE.md` 不保存密钥、令牌、个人数据或大段日志。源码、项目文件和 Git 是最终事实来源；状态文件过期时应先核实再更正。
- 未经用户明确要求，不推送、打标签、创建 GitHub Release 或发布 NuGet 包。
