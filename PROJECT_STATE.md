# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-09-26

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.30` |
| 上游提交 | `7f86191`（包含 tag `6.2.30` 的提交 `383fc24` 及 README 教程链接更新） |
| 最近上游合并提交 | `957a470`（`Merge upstream 6.2.30`） |
| 产品版本 | `v6.2.30-ext.1+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.109.0`（宿主、PluginHost、回归测试一致） |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

上游合并提交：`957a470`。2026-09-26 查询远程 `upstream/main` 为 `7f86191`，已合入当前主线。本轮上游更新了版本、预设命令行的码率参数规则和 README 教程链接，并删除其回归测试；扩展版因解决方案与扩展功能仍依赖测试套件，保留了原有测试及说明。随后核对 NuGet 官方版本列表及 LakeUI 官方 `5.109` 标签，将宿主、PluginHost 和回归测试的 LakeUI 依赖统一升级到 `5.109.0`。

- Release 解决方案构建：0 错误、0 警告。
- `FFmpegFreeUI.RegressionTests`：419 项检查通过；新增 28 项检查覆盖六种质量控制方式下四个非空码率字段均写入命令行，以及空字段不写入。
- 旧插件兼容样例 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj`：在升级后重新 Release 构建，0 错误、0 警告；实际宿主已加载插件并打开其页面，`ab-av1.exe` 显示就绪。未运行真实编码任务。
- `git diff --check`：通过。
- README 标记后的上游段与 `upstream/main` 一致。Release 主程序文件版本为 `6.2.30.1`、产品版本为 `v6.2.30-ext.1+v2.5`，主程序 SHA-256 为 `EB2008FD836F49F5807BC7657EF77CBD04402FD34061483D5BB51D73C70DD2AB`；Ext SDK `AssemblyVersion` 仍为 `2.0.0.0`，实际输出的 LakeUI DLL 文件版本为 `5.109`、SHA-256 为 `FC61D41BFF8139EB15BAECD1CD3CC5CF6CF247E2A7549AEC7C14F3094369A11F`。
- 使用实际编译出的隔离版 3FUI 检查个性化背景关闭和开启（已复制 `FFmpegFreeUISupporter_v6.dll` 并重启激活）：起始页、参数总览、音频参数及 AAC 下拉切换、插件管理列表/详情、Ext 插件设置入口、旧 AB-AV1 页面和 Ext 示例页面均已打开；最大化、还原及最小化恢复后插件列表持续显示，未见持续黑块或旧帧，观察到的 CPU/GPU 指示值恢复到低占用。未做精确帧时序或定量性能采样。
- 已知的 Ext 示例插件设置页使用透明原生 WinForms 容器，在个性化背景开/关时均显示大片黑底及文字重叠；用同一 `6.2.30` 代码、LakeUI `5.101.0` 构建的真实对照程序复现出相同画面，确认这不是 `5.109.0` 升级引入的回归。小字号中文发虚也在旧版对照中存在。此处尚未修复，不能将示例设置页记为视觉通过。
- 完整视觉验收矩阵尚未完成：切换后 50 ms / 250 ms 精确帧、100%/125%/150%/200% DPI、1280×720 与 1920×1080、定量性能采样仍需按需补测；小字号中文发虚的具体成因未确认。主程序更新首次提示也尚未用线上更高版本重复下载验证。

## 稳定设计决定

- 官方插件 API 与 Ext API 可以在同一插件中共同使用；能用官方 API 时优先使用官方 API。
- Ext API 是补充层，不以替换或复制官方 API 为目标。
- Ext 宿主使用统一的 `IExtFFmpegFreeUIHost`；尚未发布的版本后缀接口已经合并回统一接口。
- SDK 包版本与 API 能力版本同步，但 SDK `AssemblyVersion` 保持 `2.0.0.0`，以便已编译的 2.x 插件继续加载。
- LakeUI 使用 NuGet 引用；宿主、插件宿主和回归测试当前统一为 `5.109.0`，旧插件以 `5.5.0` 编译并由宿主的运行时 LakeUI 提供实现。
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

- 活跃实现任务：无。上游 `6.2.30` 已合入；保留扩展版测试套件和 Ext SDK 公共契约，LakeUI NuGet 已更新至 `5.109.0`，码率参数新规则已由回归测试覆盖。
- 未解决的合并冲突：无。
- 验证待补：修复 Ext 示例插件设置页原有的 WinForms 透明容器合成问题；如需完整视觉验收，还需精确帧时序、目标 DPI/分辨率矩阵及定量性能采样。AB-AV1 实际编码未执行；主程序更新首次提示需有更高版本 Release 才能界面验证。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
