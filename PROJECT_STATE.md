# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-10-04

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.36` |
| 上游提交 | `65aec1f`（2026-10-04 合并后再次核对远程 `main`） |
| 最近上游合并提交 | `e932dda`（`Merge upstream 6.2.36 and update LakeUI to 5.112.0`） |
| 产品版本 | `v6.2.36-ext.1+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.112.0`（宿主、PluginHost、回归测试及背景样例一致；包源码提交 `8bfe1c7`） |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

2026-10-04 将 `upstream/main` 的 `65aec1f`（产品 `6.2.36`，1 个新增上游提交）合入主线，并将 LakeUI 从 `5.110.0` 升级至 NuGet 最新稳定版 `5.112.0`。内容冲突位于主项目文件和起始页 Designer：保留扩展版身份、NuGet 引用、居中检查更新卡片及扩展版更新机制，同时合入上游页面调整；更新产品为 `v6.2.36-ext.1+v2.5` / 文件版本 `6.2.36.1`，无遗留合并冲突。

- 上游主要调整起始页 3F 产品介绍、移除过时的“即将推出”内容、增加 ARXChem 教程入口；补齐原生窗口样式下字幕页显示时的列宽初始化；画面裁剪窗口改用 LakeUI 的 `PixelPictureTiledSource`、sRGB 图像源与可编辑预览时间下拉框，补充图像生命周期、比例解析和裁剪范围保护；清理过时新闻条目。扩展版插件管理、API、队列工具栏及自有更新逻辑保留。
- `dotnet restore` 成功；提交前后两轮完整 Release 解决方案构建均为 0 错误、0 警告，两轮完整回归均通过 459 项检查。新增裁剪预览回归检查覆盖预览时间选择/手工输入、有效与越界比例以及裁剪参数解析，共增加 40 项检查；这些解析检查不代替真实界面验证。
- 旧插件源码 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj` Release 构建 0 错误、0 警告，插件源码未改。旧部署 DLL 的运行时加载失败被用户明确判定为插件自身问题，本轮不修复、不计作宿主兼容通过；实际编码任务、视频时间戳提帧及插件评分流程未执行。
- `git diff --check` 通过；README 标记后的上游段与合入的 `upstream/main` 一致。相对合并前 `f7f82a9`，Ext SDK 全部源码/公共契约未改，`AssemblyVersion` 保持 `2.0.0.0`。
- 已阅读 LakeUI 官方源码 `18163b6..8bfe1c7` 的相关变更。`5.112.0` 包元数据与输出 DLL 对应源码 `8bfe1c74f6aac1c2d1acf4d1238a62fdd2377bd9`；主要变化包括 PixelPictureBox 图像源/分块呈现与色彩编码、ModernComboBox 前缀和文本区域/光标偏移处理、浮动提示及对话/图表控件更新。**LakeUI 自身删除了 `PixelPictureBox.Image`，改用 `Source`；第三方插件若直接调用旧 `Image` 属性，需要自行适配，不能把 Ext SDK ABI 未变等同于所有 LakeUI 插件都二进制兼容。** 本次 AB-AV1 源码中未找到该控件或属性的调用。
- 合并提交 `e932dda` 后构建的 Release `FFmpegFreeUI.exe` SHA-256：`BE6A7D3CDA6DB7BE1B813DCAB11B39AD64EB91A665A193DCD15C8DBC1BEED61D`；`FFmpegFreeUI.dll`：`E534485FDFE74DA8C0D12147F4B778916E9F0A086AEF99D4F7FA57524F5FE895`；输出 LakeUI DLL：`0E8D81B8EE19E19FA8A265A87184F9A00E6F30B68DAEF17F2955A69DE1BA7937`。独立测试副本以 `publish --no-build --no-restore` 创建，核对实际进程加载路径、主 DLL/LakeUI/两个 Ext DLL 的版本和哈希，与最终 Release 输出一致。
- 通过 Computer Use 检查实际编译后的 3FUI，不使用自建窗体替代。背景关闭时检查起始页、参数总览/画面帧页、实际图片裁剪预览、插件管理、LakeUI 插件入口/设置页；裁剪预览检查滚轮缩放、左键平移，以及选择 `16:9` 和 `1 分钟` 后的完整下拉文字；原生窗口样式 `1` 下查看字幕布局。测试图片复制自仓库已有背景资源，不使用用户媒体。
- 背景开启时将现有支持者 DLL 复制到独立测试副本，使用 `SP_毛玻璃模式 = 1`、`SP_毛玻璃背景来源 = 0`、窗口样式 `2`，实际屏幕确认内置图片/玻璃背景已启用。检查起始页、参数页、裁剪窗口（空预览背景映射、加载图片及缩放）、字幕页、插件管理、最小 WinForms 示例设置页、LakeUI 背景插件入口和设置页；入口/设置页均测试插件专属蓝色背景开启后再恢复宿主映射。完成常规窗口到最大化再恢复、最小化再恢复，截图未见持续黑块或旧帧；未用 Timer、循环 Invalidate 或延时修补渲染。
- 本轮仅是合并相关视觉烟测：当前窗口截图逻辑尺寸约 `1199×699`，最大化 `1707×1067`。未执行完整多 DPI/指定分辨率矩阵、50 ms / 250 ms / 1 s 精确帧或定量 CPU/GPU 采样，不能据此声称全部历史渲染问题已修复。小字号中文发虚原因仍未确认；插件管理初次显示首项预览但未正式选中时，设置入口需要先点击列表选中后才响应，该既有逻辑本轮未改；主程序更新首次提示尚未用线上更高版本验证。
- 测试程序正常退出后，已清理本次自行创建的独立发布/配置/图片副本和临时 LakeUI 源码审查目录，这些内容均可重建；原支持者 DLL、原软件配置及用户文件未改。合并后再次查询远程 `main`，仍为 `65aec1f`。未推送、发布或打标签。

## 稳定设计决定

- 官方插件 API 与 Ext API 可以在同一插件中共同使用；能用官方 API 时优先使用官方 API。
- Ext API 是补充层，不以替换或复制官方 API 为目标。
- Ext 宿主使用统一的 `IExtFFmpegFreeUIHost`；尚未发布的版本后缀接口已经合并回统一接口。
- SDK 包版本与 API 能力版本同步，但 SDK `AssemblyVersion` 保持 `2.0.0.0`，以便已编译的 2.x 插件继续加载。
- LakeUI 使用 NuGet 引用；宿主、插件宿主、回归测试和背景样例当前统一为 `5.112.0`。旧插件以 `5.5.0` 编译并由宿主的运行时 LakeUI 提供实现；直接依赖 LakeUI 控件 API 的插件还需单独检查 LakeUI 自身的破坏性变更。
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

- 活跃实现任务：无；本轮上游 6.2.36 合并、LakeUI 5.112.0 升级、构建/回归及合并相关背景开/关真实界面烟测完成，结果见上文。未推送、发布或打标签。
- 未解决的合并冲突：无；Ext SDK 公共契约未改。LakeUI `PixelPictureBox.Image` 迁移风险见上文，不宣称全部第三方插件运行时兼容。
- 验证待补：完整精确帧时序、目标 DPI/分辨率矩阵及定量性能采样未做；小字号中文发虚与插件管理首项预览未选中时的设置入口行为见上文。旧 AB-AV1 插件本轮仅构建，旧部署 DLL 运行时错误按用户要求排除，实际编码未执行；主程序更新首次提示需有更高版本 Release 才能界面验证。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
