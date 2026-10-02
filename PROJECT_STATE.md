# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-10-02

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.35` |
| 上游提交 | `4fca883`（2026-10-02 再次核对的 `upstream/main`） |
| 最近上游合并提交 | `f8cb35e`（`Merge upstream 6.2.35 and update LakeUI to 5.110.0`） |
| 产品版本 | `v6.2.35-ext.1+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.110.0`（宿主、PluginHost、回归测试及背景样例一致；包源码提交 `18163b6`） |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

2026-10-02 将 `upstream/main` 的 `4fca883`（产品 `6.2.35`，3 个新增上游提交）合入主线。唯一内容冲突是主项目版本号，保留扩展版身份与 NuGet 引用，更新为 `v6.2.35-ext.1+v2.5` / 文件版本 `6.2.35.1`。上游将各列表的框选区域改为 `DragSelectZoneRatio = 0.5F`，删除宿主按控件宽度反复设置 `DragSelectZoneWidth` 的代码；保留扩展版队列列宽校准、插件工具栏等逻辑。其他更新包括 Agent 临时草稿/附件归属、工具取消传播、提取工具取消与目录枚举修复。

- `dotnet restore` 成功；合并完成后、提交前后的两轮 Release 解决方案构建均为 0 错误、0 警告；两轮回归均通过 419 项检查。
- 旧插件源码 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj` Release 构建 0 错误、0 警告。旧部署 DLL 的运行时加载失败被用户明确判定为插件自身问题，本轮不修复、不计作宿主兼容通过；实际编码任务也未执行。
- `git diff --check` 与暂存区检查通过；README 标记后的上游段与 `upstream/main` 一致。Ext SDK 源码/公共契约未改，`AssemblyVersion` 保持 `2.0.0.0`。LakeUI `5.110.0` 的包元数据和输出 DLL 均对应官方源码 `18163b6edc0cf339883eae5df833e2f5ff7a70a5`；旧 `DragSelectZoneWidth` 属性仍保留。LakeUI 本轮增加比例框选属性、新 `AgentDialogueList` 控件，优化 `AgentRoom` 的文本测量缓存/脏矩形裁剪，并提取进度环内部绘制器；GPU 引擎、背景映射、ModernPanel/Tab/ComboBox 核心未改。
- 合并提交 `f8cb35e` 后构建的 Release `FFmpegFreeUI.exe` SHA-256：`18DE23CEABE4C43C045D9C6A62268340E32EC0D8332921EB3160AC7B63A7A81D`；`FFmpegFreeUI.dll`：`D9A0C94FD9B1A213415E5B1220B3F4396B1F6F8C6876E80A5E3372BBE1B72A0F`；输出 LakeUI DLL：`52F1CA48C6357E712C667866AE55BB45389DF65D64427930778E68BD6557F35C`。
- 用实际编译的 3FUI 隔离副本完成视觉烟测，未用自建测试窗体代替。背景关闭时查看起始页、准备文件、参数总览/预设管理、Agent、插件管理及背景插件设置页，并切换预设来源、选择预设；背景开启时复制现有支持者 DLL，确认个性化控件已解锁，使用干净玻璃/内置背景图（设置值 `SP_毛玻璃模式 = 1`、`SP_毛玻璃背景来源 = 0`），检查起始页、准备文件、编码队列、Agent、插件管理、插件入口和设置页。入口/设置页均可切换为插件专属蓝色背景并恢复宿主背景；Agent 用本地离线会话查看长文本、表格、代码及向上滚动/滚动条拖动，并放大/恢复、最小化/恢复窗口。未见持续黑块或旧帧；即时截图中出现的绘制过渡未当成异常修复。
- 最终提交产物重新发布时使用 `--no-build`，核对实际进程加载路径、主程序/LakeUI/Ext DLL 版本和主 DLL SHA-256，与最终 Release 输出一致；再次检查背景开启的起始页/插件管理及背景关闭的起始页/Agent 长对话与滚动。测试副本、离线会话及临时 LakeUI 源码目录均已清理，原支持者 DLL 未改。
- 本轮是当前显示缩放、常规窗口与最大化窗口的合并相关视觉烟测，未执行完整多 DPI/指定分辨率矩阵、50 ms / 250 ms / 1 s 精确帧或定量 CPU/GPU 采样，不能据此声称全部历史渲染问题已修复。小字号中文发虚原因仍未确认；插件管理初次显示首项预览但未正式选中时，设置入口需要先点击列表选中后才响应，该既有逻辑本轮未改；主程序更新首次提示尚未用线上更高版本验证。

## 稳定设计决定

- 官方插件 API 与 Ext API 可以在同一插件中共同使用；能用官方 API 时优先使用官方 API。
- Ext API 是补充层，不以替换或复制官方 API 为目标。
- Ext 宿主使用统一的 `IExtFFmpegFreeUIHost`；尚未发布的版本后缀接口已经合并回统一接口。
- SDK 包版本与 API 能力版本同步，但 SDK `AssemblyVersion` 保持 `2.0.0.0`，以便已编译的 2.x 插件继续加载。
- LakeUI 使用 NuGet 引用；宿主、插件宿主、回归测试和背景样例当前统一为 `5.110.0`，旧插件以 `5.5.0` 编译并由宿主的运行时 LakeUI 提供实现。
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

- 活跃实现任务：无。本轮上游合并、LakeUI 更新审查及常规验证已完成；未推送、发布或打标签。
- 未解决的合并冲突：无；未发现必须破坏 Ext SDK 公共契约的改动。
- 验证待补：完整精确帧时序、目标 DPI/分辨率矩阵及定量性能采样未做；小字号中文发虚与插件管理首项预览未选中时的设置入口行为见上文。旧 AB-AV1 插件本轮仅构建，旧部署 DLL 运行时错误按用户要求排除，实际编码未执行；主程序更新首次提示需有更高版本 Release 才能界面验证。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
