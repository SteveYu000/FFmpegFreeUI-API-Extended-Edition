# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-09-25

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.29` |
| 上游提交 | `2e03d0b`（tag `6.2.29`） |
| 最近上游合并提交 | `790fbe2`（`Merge upstream 6.2.29`） |
| 产品版本 | `v6.2.29-ext.1+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.101.0` |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

基线提交：`790fbe2`。本轮上游仅修改参数面板界面、文案和官方版本号；扩展版 SDK 与 LakeUI 依赖未变。2026-09-25 重新查询远程 `upstream/main` 仍为 `2e03d0b`，已包含在当前 `HEAD` 中。

- Release 解决方案构建：0 错误、0 警告。
- `FFmpegFreeUI.RegressionTests`：391 项检查通过。
- 旧插件兼容样例 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj`：Release 构建 0 错误、0 警告。
- `git diff --check`：通过。
- README 标记后的上游段与 `upstream/main` 一致。编译产物主程序程序集版本为 `6.2.29.1`，Ext SDK 程序集版本仍为 `2.0.0.0`。
- 已在实际构建的 `FFmpegFreeUI.exe` 上补做视觉验证：Release 主程序与隔离测试副本的 SHA-256 均为 `B4FAC4818BEF415E35A7044CABE3B0378CB085B4983714ABB293BD5D64C7E329`，使用的 LakeUI `5.101.0` DLL SHA-256 为 `35C3A54EB92AE3B9B8BF1DD7C5420CD1973FF022D4A244CAC4661487C0E53E8B`。隔离副本复制了 Supporter DLL，程序确实识别 Supporter Pack；通过程序设置切换个性化背景开/关并分别重启后检查。
- 背景关闭时检查了主页、参数总览、音频及其他参数页和插件管理；背景开启时检查了主页、参数总览、音频、插件管理、AB-AV1 的两个子页，并进行下拉切换、页面切换、最大化/恢复及最小化/恢复。所观察的最终画面未见持续黑块或旧帧。旧 AB-AV1 插件运行时被发现并加载，注册的侧栏入口和两个子页可打开；隔离副本缺少 `ab-av1.exe`，未执行实际编码。
- 用上游提交 `2e03d0b` 的未修改源码另行构建 `6.2.29` 对照程序，引用与扩展版 SHA-256 相同的 LakeUI DLL；同尺寸、背景关闭时，对照程序的小字号中文也出现类似笔画发虚，现有证据不能将此现象归因于上游合并。对照构建因 NuGet 漏洞数据源不可访问出现 2 条 `NU1900`，没有编译错误；这不改变扩展版上述 0 警告的构建记录。
- 视觉验证仍不等于完整验收矩阵：当前截图操作无法精确覆盖切换后 50 ms / 250 ms 的帧；100%、125%、150%、200% DPI 与 1280×720、1920×1080 均未逐项实测。小字号中文发虚的具体成因也未确认。未观察到持续的 CPU/GPU 占用升高，但尚未做定量性能采样。
- 前一基线 `v6.2.28-ext.3+v2.5` 已在隔离单文件副本中验证插件重启首次 PowerShell 提示和再次重启不重复提示；该结果不能代替新基线视觉验证。主程序更新首次提示尚未用线上更高版本重复下载验证。
- 上述记录不能替代后续改动的重新验证。涉及界面或 LakeUI 的改动仍需使用实际构建的 FFmpegFreeUI 做视觉回归。

## 稳定设计决定

- 官方插件 API 与 Ext API 可以在同一插件中共同使用；能用官方 API 时优先使用官方 API。
- Ext API 是补充层，不以替换或复制官方 API 为目标。
- Ext 宿主使用统一的 `IExtFFmpegFreeUIHost`；尚未发布的版本后缀接口已经合并回统一接口。
- SDK 包版本与 API 能力版本同步，但 SDK `AssemblyVersion` 保持 `2.0.0.0`，以便已编译的 2.x 插件继续加载。
- LakeUI 使用 NuGet 引用；宿主和插件宿主当前统一为 `5.101.0`。
- README 的扩展版内容位于 `# 以下是上游仓库原内容` 之前，之后保持上游原文。
- 默认框架依赖单文件发布包含 `FFmpegFreeUI.exe`、Ext Plugin Host DLL 和 Ext Plugin SDK DLL。
- 主程序更新直接由宿主实现：从本项目 Release 下载三份匹配文件，校验 GitHub SHA-256、主程序版本/架构和 Ext DLL 版本；正常退出后由现有 PowerShell 助手机制替换并在失败时回滚。主程序更新与插件变更重启共用首次 PowerShell 提示及已提示设置字段。

## 环境与兼容性夹具

- 上游远程：`upstream = https://github.com/Lake1059/FFmpegFreeUI.git`
- 扩展版远程：`origin = https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git`
- 网络异常时可临时使用系统代理：`http://127.0.0.1:7897`。
- 旧插件兼容性源码：`E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1`。
- 个性化背景完整激活需要测试环境中存在 `C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll`，并将其复制到实际测试程序所需位置。测试报告应说明背景功能是否真正激活。

## 当前工作

- 活跃实现任务：无。
- 未解决的合并冲突：无。
- 验证待补：如仍需完整视觉验收，需补做精确帧时序、目标 DPI/分辨率矩阵及定量性能采样；AB-AV1 的实际编码需要提供 `ab-av1.exe`；主程序更新的首次提示在有更高版本 Release 时做界面验证。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
