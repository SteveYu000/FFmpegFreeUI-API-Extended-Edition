# FFmpegFreeUI API Extended Edition 项目状态

> 本文件是开发交接的事实快照。长期规则见 AGENTS.md；源码、项目文件和 Git 是最终事实来源。不要记录密钥或完整日志。

最后更新：2026-11-12

## 当前工作：精简发布与窗口布局

- 起点 main / 6719237，工作树干净。用户要求恢复精简发布；界面显示新增“窗口布局”（使用默认大小 / 使用上次关闭时的大小）；功能子窗口打开时与实际顶层父窗口同大小。
- 功能实现及下述功能验证已完成，改动尚未提交；没有推送、打标签或发布 Release / NuGet。
- 只修改宿主、回归及目标框架说明，SDK 契约、程序集身份、普通示例和第三方插件源码未改。用户原 C:\Apps\FFmpegFreeUI API Extended 及配置未覆盖、未关闭。
- 保留本轮隔离副本和发布物供复查；用户手动操作过的 BackgroundOff 配置未还原或清理。测试副本已正常退出。

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 分支 / 本轮起点 | main / 6719237 |
| 最近合入的上游版本 / 提交 | 6.5.0 / b5305ec4804535297fec3a8612a4256d50aee559 |
| 上游合并提交 | f66575d10ce4f2ca295484f5ac484f5d610b3ffe |
| 产品版本 / 文件版本 | v6.5.0-ext.1+v2.5 / 6.5.0.1，不升版 |
| Ext API / NuGet / SDK AssemblyVersion | v2.5 / 2.5.0 / 2.0.0.0，不变 |
| 主程序与回归目标 | net11.0-windows，SupportedOSPlatformVersion 7.0 是 TFM 配置要求，不代表支持 Windows 7 |
| PluginHost 与 LakeUI 背景样例目标 | net11.0-windows10.0.17763.0 |
| SDK、普通 C# / VB 插件样例目标 | net10.0-windows10.0.17763.0，保持已发布目标 |
| LakeUI | 纯 NuGet 5.301.0，无本地 DLL 覆盖 |
| LakeUI 包声明的源码提交 | d844f8dfc4b843797fb992cfb3ad9a95f16428ea，与 5.201.0 相同 |
| 主解决方案 | FFmpegFreeUI-API-Extended-Edition.slnx |
| 验证环境 | .NET SDK 11.0.100 / Microsoft.WindowsDesktop.App 11.0.0 / win-x64 |

## 本轮修改机制

- 精简：主程序和回归使用普通 Windows TFM，避免引入未使用的 Windows SDK .NET 投影。PluginHost 保留其 SDK 引用需要的 17763 目标；发布仍为主 EXE + 两个 Ext DLL，无裁剪、无运行时打包。
- 设置页：在 Designer 中按现有页面风格新增窗口布局行。旧配置默认使用默认大小；记忆选项保存正常窗口的 96 DPI 尺寸及最大化状态，启动按当前 DPI 还原并限制在工作区内，显式 fullscreen 参数优先。
- 保存时机：从关闭流程中移除提前保存，改为实际 FormClosed 记录布局后保存设置，避免取消退出时覆盖历史布局；最小化时保留最近的普通/最大化状态。
- 子窗口：新增内部窗口布局_v6，解析嵌入页面所属的实际顶层 Form；首次显示前及窗体自身 Load 完成后统一应用父窗口大小，避开字体、DPI 和 LakeUI Attach 对首次尺寸的改写。重新打开已隐藏窗口时重新应用当前父窗口尺寸。
- 清理旧方案：移除参数预览、任务日志、质量图表各自竞争的 BeginInvoke 居中/定位逻辑；参数子页、裁剪返回页、混流和媒体流选择器传递真实父窗口。系统文件/字体/颜色选择器及紧凑消息框保留原布局。
- 未修改 LakeUI 绘制、背景映射或 presenter 生命周期；没有增加渲染 Timer、循环 Invalidate、长延时或插件兼容垫片。

## 自动验证

- 完整 restore + Release 解决方案构建：0 错误、0 警告。
- 完整回归：612 项通过，退出代码 0；新增 29 项覆盖旧配置、非法选项、JSON 往返、96/120/144/192 DPI 尺寸换算、1280×720/1920×1080/800×450 工作区限制、嵌入页面父窗口解析及 Load 后子窗口尺寸。
- 未修改的旧插件 E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1 Release 构建：0 错误、0 警告，工作树保持干净。按既有范围不测试该插件旧 DLL 或完整评分链。
- SDK 目录无改动；README 分界线后仍与本次上游原文一致；git diff --check 通过。
- DPI/工作区数学回归与通用 WinForms 回归不等于实际多 DPI 视觉测试。

## 本轮真实程序验证

实际发布物位于 artifacts/window-layout-review-20261112-4a7e8c10/SingleFileFinal。BackgroundOff / BackgroundOn 均使用同一最终 EXE 和 Host DLL，并加载修改前 SDK、.NET 10 编译的 C# 示例及旧 LakeUI 背景示例；未给插件目录放入私有 LakeUI。背景开启副本使用已有合法支持者 DLL 和背景图片，已从实际屏幕确认激活。

| 场景 | 实际结果 |
| --- | --- |
| 背景关闭：默认启动与旧插件初始化 | 真实窗口显示，两个旧插件入口出现；已核对实际加载路径/哈希 |
| 背景关闭：新设置行、展开下拉和选择记忆大小 | 标题与两个选项可见；正常退出后配置保存模式 1、1199×699、非最大化 |
| 背景关闭：抽帧子窗口 | 实际捕获的主窗口/子窗口客户区均为约 1199×699 逻辑像素，内容显示完成 |
| 背景开启：设置页及下拉切换 | 窗口布局标题、说明、当前选项与下拉显示，背景正确；通过 UI 切回默认模式 |
| 背景开启：保存尺寸启动 | 启动恢复预先准备的 1350×820 尺寸 |
| 背景开启：实际拖动缩放、退出、重开 | 调整到 1301×761，退出后配置保存该尺寸，重开实际窗口恢复同尺寸 |
| 背景开启：普通及最大化父窗口的抽帧子窗口 | 实际显示约 1301×761 和 1707×1067，分别与打开时父窗口同大小，背景/内容完整显示 |
| 背景开启：最大化退出/重开/还原 | 配置保存正常尺寸 1301×761 和 Maximized=true；重开实际为约 1707×1067，恢复普通窗口后为 1301×761 |
| 背景开启：默认模式忽略历史布局 | UI 选择模式 0 后保留历史尺寸/最大化记录，重开实际为默认约 1199×699，未受历史最大化影响 |
| 背景开启：最小化/恢复与最终视觉回归 | 最小化后恢复到同一默认尺寸；最终真实 EXE 首页显示完成，随后正常退出 |
| 100%/125%/150%/200% DPI 完整屏幕矩阵；指定 1280×720/1920×1080；精确立即/50/250/1000 ms；全部功能子窗口逐项；长期 CPU/GPU | 未完整执行。仅完成当前显示配置下的上述真实功能检查，不能标记完整渲染验收全部通过 |

Computer Use 的 activate_window 会将最大化窗口还原；已用激活前后的实际截图核对该工具副作用，不据此修改宿主代码。重开后的首次捕获偶有其他用户应用遮挡，激活测试副本后再次检查；不将外部遮挡视为宿主黑块。此前用户手动使用窗口并以 Escape 暂停的阶段未继续输入；收到“继续”后恢复测试。

## 发布物与证据

- SingleFileFinal：EXE 12786040 字节 / 12.19 MiB；Host 103936 字节；SDK 51200 字节；三文件共 12941176 字节 / 12.34 MiB。
- 发布依赖不含 Microsoft.Windows.SDK.NET.dll / WinRT.Runtime.dll。此前 Windows 17763 主目标会额外带入约 23.12 MiB Windows SDK .NET 和 WinRT.Runtime，是体积增长的原因。
- 使用默认发布脚本对应的参数发布到新目录，未运行会清理既有发布目录的常规脚本；没有覆盖原 artifacts/FrameworkDependentSingleFile。

| 文件 | SHA-256 |
| --- | --- |
| SingleFileFinal/FFmpegFreeUI.exe | AD5165999E6F0D9E7D0C18F341D938741935AD86BCC9E7368DBE43E418F8A4DE |
| SingleFileFinal/FFmpegFreeUI.Ext.PluginHost.dll | B3B4A72095DD1A44E7202BE2B4DE791D5EFA69E9902B5C18D51215A19307A614 |
| SingleFileFinal/FFmpegFreeUI.Ext.PluginSdk.dll | 50D9DC9926A7AB5FAF7A392469B3D2525F911701401AB07954BA759D2017D2CE |
| 实际隔离测试的修改前 SDK DLL，AssemblyVersion 2.0.0.0 / FileVersion 2.5.0.0 | B583D43E7D82EC1260AE6872A57E074427873513D3B19F7EFF39D115C276A5AC |

同目录保留 publish-hashes.json、off-loaded-modules.json 及 off-saved-layout.json、on-saved-normal-layout.json、on-saved-maximized-layout.json、on-saved-default-layout.json。隔离副本供复查，勿当成用户原安装目录或擅自清理手动改过的配置。

## 上游 6.5.0 合并基线摘要

- 已合入 b5305ec（2026/11/11-1，Release 于 2026-11-12 发布）：迁移 .NET 11、产品版本、README 运行环境及官方 Updater 的运行库检查/下载、TickCount64；没有官方插件 API、事件或宿主界面源码变动。
- 逐项解决项目及 Updater 修改/删除冲突；保留扩展版品牌、API 组件拆分、自有仓库检查更新和用户确认，未恢复上游强制升级器；移除重新带入的本机 LakeUIAssembly 引用。
- LakeUI 5.201/5.301 包资产分别为 net10/net11，均声明相同源码提交、GPL-3.0-only 和 Vortice 3.8.3。两版 XML 1358 个成员一致，SHA-256 F4740A1641805BD38DA20E22010012AA0304C12646072DA267F9CA645CEA5296；该结果不等于所有内部实现或第三方插件完整认证。
- 合并轮次曾通过 583 项回归、旧插件源码构建和背景开/关真实旧插件烟测；本轮已用 612 项回归及上述实际发布物功能测试更新基线。旧轮详细哈希保留在 artifacts/upstream-650-review-20261112-d9c518a7。

## 既有问题和稳定约定

- 浅色插件详情低对比度是宿主 插件详情视图_v6.vb 的硬编码亮色造成，旧版本对照已确认；本轮未改配色，未重新验证浅色主题，仍待单独修复。
- 官方 API 优先，Ext 补充；SDK AssemblyVersion 保持 2.0.0.0；SDK 兼容、LakeUI 依赖和插件自身行为分别判断。
- README 的“以下是上游仓库原内容”之后必须与已合入上游原文一致。
- 合法支持者 DLL：C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll，SHA-256 A08B0B3A00ECBF6AD2A83B39BE105D4C385D3AFD8DC4C5CE94D1C7E400D5741E，仅用于合法隔离验证。
- upstream：https://github.com/Lake1059/FFmpegFreeUI.git；origin：https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git。
- 网络需要时用命令级 http://127.0.0.1:7897 代理与 Git OpenSSL，不更改全局配置、TLS 校验或系统代理。
