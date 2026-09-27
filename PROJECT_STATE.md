# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-09-27

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.32` |
| 上游提交 | `d868c8d`（2026-09-27 的 `upstream/main`） |
| 最近上游合并提交 | `63d3d2f`（`Merge upstream 6.2.32`） |
| 产品版本 | `v6.2.32-ext.1+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.109.0`（宿主、PluginHost、回归测试一致） |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

2026-09-27 将 `upstream/main` 的 `d868c8d`（产品 `6.2.32`）合入主线。上游本轮主要拆分预设管理与命令行代码，调整参数页 Designer、编码器数据及 Agent 技能资料，并更新 README 与第三方声明。冲突处理保留扩展版身份、LakeUI `5.109.0` NuGet 和 Ext API v2.5；将扩展版的预设总览、质量选项映射移植到上游拆分后的模块。旧 `预设面板映射_v6.vb` 随上游删除，不恢复过时代码。

- `dotnet restore` 成功；Release 解决方案构建 0 错误、0 警告；回归测试 419 项检查通过。
- 旧插件源码 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj` Release 构建 0 错误、0 警告。旧部署 DLL 的运行时加载失败被用户明确判定为插件自身问题，本轮不修复、不计作宿主兼容通过；实际编码任务也未执行。
- `git diff --check` 与暂存区检查通过；README 标记后的上游段与 `upstream/main` 逐字一致。Ext SDK `AssemblyVersion` 保持 `2.0.0.0`，公共契约未改。主程序版本为 `v6.2.32-ext.1+v2.5`，文件版本 `6.2.32.1`。
- 实际 Release `FFmpegFreeUI.exe` SHA-256：`602D26091627F9178DEB208DDD3DCC7ADAB64DA8737C462085835D0C6AEB3133`；`FFmpegFreeUI.dll`：`AA4595C6A65B12F93F1475C6761A9C7B05D013E9A941832CD60CFC4C042192CA`；输出 LakeUI DLL：`FC61D41BFF8139EB15BAECD1CD3CC5CF6CF247E2A7549AEC7C14F3094369A11F`。
- 用与 Release 主程序 DLL 哈希一致的隔离发布副本运行实际 3FUI，复制支持者 DLL，分别检查背景关闭、开启干净玻璃并重启后的画面；查看起始页、参数总览、色彩管理、质量/流控制及插件管理。CRF 选择能出现在参数总览；未见持续黑块或旧帧。隔离发布目录和其中的临时支持者 DLL 已删除，源 DLL 未动。
- 本轮仅执行合并相关的常规视觉烟测；未做 50 ms / 250 ms 精确帧、多 DPI/分辨率矩阵或定量性能采样。此前已知的小字号中文发虚原因尚未确认；主程序更新首次提示也尚未用线上更高版本验证。

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

- 活跃实现任务：无。本轮上游合并及常规验证已完成。
- 未解决的合并冲突：无。
- 验证待补：如需完整视觉验收，还需精确帧时序、目标 DPI/分辨率矩阵及定量性能采样。小字号中文发虚的具体成因未确认；旧 AB-AV1 插件本轮仅构建，运行时错误按用户要求排除，实际编码未执行；主程序更新首次提示需有更高版本 Release 才能界面验证。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
