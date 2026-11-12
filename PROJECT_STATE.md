# FFmpegFreeUI API Extended Edition 项目状态

> 本文件是开发交接的事实快照。长期规则见 AGENTS.md；源码、项目文件和 Git 是最终事实来源。不要记录密钥或完整日志。

最后更新：2026-11-12

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | main |
| 最近合入的上游版本 / 提交 | 6.5.0 / b5305ec4804535297fec3a8612a4256d50aee559 |
| 上游合并提交 | f66575d10ce4f2ca295484f5ac484f5d610b3ffe |
| 本轮开始状态 | 0fe29cc，工作树干净；用户原 3FUI 正在运行，未关闭或覆盖 |
| 产品版本 / 文件版本 | v6.5.0-ext.1+v2.5 / 6.5.0.1 |
| Ext API / NuGet / SDK AssemblyVersion | v2.5 / 2.5.0 / 2.0.0.0，保持不变 |
| 主程序、PluginHost、回归及 LakeUI 背景样例目标 | net11.0-windows10.0.17763.0，与新上游/依赖匹配 |
| SDK、普通 C# / VB 插件样例目标 | net10.0-windows10.0.17763.0，保持已发布编译目标 |
| LakeUI | 纯 NuGet 5.301.0，不启用本地 DLL 覆盖 |
| LakeUI 包声明的源码提交 | d844f8dfc4b843797fb992cfb3ad9a95f16428ea，与 5.201.0 相同 |
| 主解决方案 | FFmpegFreeUI-API-Extended-Edition.slnx |
| 验证环境 | .NET SDK 11.0.100 / Microsoft.WindowsDesktop.App 11.0.0 / win-x64 |

## 当前工作结果：上游 6.5.0 已合并

- 已通过命令级代理拉取官方最新 b5305ec（2026/11/11-1）；本轮只有一条新的上游提交，Release 于 2026-11-12 发布。
- 上游主要迁移 .NET 11，升级产品版本，调整 README 运行环境及官方 Updater 的运行库检查/下载、TickCount64。没有更改官方插件 API、事件、宿主界面源码或 LakeUI 控件调用。
- 主项目冲突逐项保留扩展版品牌、API 组件拆分和 NuGet 引用；同步 .NET 11 / Windows 17763 和 6.5.0 版本。移除上游重新带入的本机 LakeUIAssembly 路径。
- Updater/Program.vb、Updater/Updater.vbproj 的修改/删除冲突继续保留扩展版既有删除；没有恢复官方强制升级器。本项目仍使用自有仓库检查更新、用户确认后升级。
- PluginHost 和回归项目随主程序升为 .NET 11；引用新 LakeUI 的独立背景测试插件也改为 .NET 11。SDK 目录、公开签名、程序集身份和普通最小插件源码均未改；SDK 的 NuGet 发布工作流继续构建 .NET 10 SDK 项目。
- 发布脚本的 SDK 最低检查及 Desktop Runtime 提示改为 11，保留 UTF-8 BOM，默认仍发布主程序和两个 Ext DLL。
- 更新开发指南、README 扩展版版本及第三方声明；分界线后的 README 已逐字核对为本次上游内容。
- .NET 11 新增分析器暴露两处警告：将裁剪窗口 Designer 的组件字段移至方法后；缓存 PowerShell 工具固定的 command 字段名数组。没有屏蔽分析器或修改 SDK 契约，Agent 原有行为由既有回归覆盖。

## LakeUI 5.301 依赖核对

- 已核对官方 NuGet v3 元数据：5.301.0 资产为 lib/net11.0-windows，5.201.0 为 lib/net10.0-windows；均声明相同源码提交、GPL-3.0-only 和 Vortice 3.8.3 依赖。
- 两版 XML 文档均为 1358 个成员，名称无增删，文档 SHA-256 均为 F4740A1641805BD38DA20E22010012AA0304C12646072DA267F9CA645CEA5296。该检查覆盖文档成员，不代替对所有未公开实现或第三方 DLL 的完整认证。
- 本轮没有新的 LakeUI 渲染源码差异，没有修改宿主布局、背景映射、HWND/presenter 生命周期或 ComboBox/TabControl 调用；沿用上一轮对 d844f8d 源码的审阅结论。
- 已用未重新编译的 .NET 10 SDK 和旧示例 DLL 在真实 .NET 11 宿主检查；另用本轮编译的背景插件验证新开发配置。实际运行结果见下表。

## 自动验证

- 完整 restore + Release 解决方案构建通过；最终合并提交再次构建 **0 错误、0 警告**。
- 完整回归 **583 项通过，退出代码 0**；包括 Agent 进程、参数/预设、队列、Ext 准备阶段、自有更新及回滚。
- 未修改的旧插件 E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1 Release 构建 **0 错误、0 警告**，工作树保持干净。按用户既有范围，不测试其部署旧 DLL 或完整评分链。
- SDK 目录相对 0fe29cc 无差异；实际旧 SDK 程序集身份为 FFmpegFreeUI.Ext.PluginSdk, Version=2.0.0.0。
- 发布 PowerShell 脚本通过语法解析；README 上游内容一致，git diff --check 通过。没有执行本轮单文件发布或覆盖既有发布目录。

## 本轮真实界面验证

使用 f66575d 上实际编译的主程序建立背景关闭/开启两个合法位置的隔离副本，显式设置微软雅黑。用户 C:\Apps\FFmpegFreeUI API Extended 原程序及其配置不变。背景开启副本部署已有合法支持者 DLL 和背景图片，实际画面确认已激活。

Computer Use 的 launch_app 两次报 accessibility window-opened handler did not become ready；改用进程命令启动已编译程序后，Computer Use 正常选择、操作并捕获实际 DirectX 窗口，没有用自建窗体代替宿主。

| 场景 | 实际结果 |
| --- | --- |
| 背景关闭/开启时启动，加载两个修改前的示例 DLL 和旧 SDK | 均执行；管理页面显示两个插件已加载，进程模块路径/哈希吻合 |
| 旧 LakeUI 背景样例嵌套设置页 | 两种背景均实际打开；有背景时继承宿主图像，无背景时显示正常底色 |
| 旧背景样例独立导航页及蓝底开关、恢复 | 有背景执行；蓝底只覆盖本插件页，关闭后恢复宿主背景，左侧全局背景不变 |
| 旧背景样例嵌套设置页的蓝底开关 | 无背景执行，实际切换为蓝底 |
| 旧纯 WinForms 示例设置页 | 无背景执行，实际显示标题、说明、复选框及按钮 |
| 最大化/恢复 | 有背景的旧插件嵌套设置页执行；约 1199×699 / 1707×1067 逻辑像素，背景及页面内容随窗口恢复 |
| 本轮 .NET 11 编译的背景样例 | 有背景副本重启后实际打开独立页并切换蓝底；进程实际加载的新插件哈希已核对，仍使用修改前 SDK |
| 最终视觉检查 | 本轮编译主程序 + 新背景样例的实际窗口再截图；测试副本均正常退出 |
| 浅色主题、多 DPI、指定 1280×720/1920×1080、精确立即/50/250/1000 ms、连续下拉、最小化/恢复、长期 CPU/GPU | **本轮未执行完整渲染验收矩阵**。本轮是目标框架/包重编译迁移，无新增渲染机制；上述烟测不能记成完整矩阵通过 |

测试中没有对第三方插件做源码适配；没有新增长延时、渲染 Timer、循环 Invalidate、自定义 GPU 视图或兼容垫片。

### 实际测试产物哈希（SHA-256）

| 文件 | 版本/用途 | SHA-256 |
| --- | --- | --- |
| FFmpegFreeUI.exe | 6.5.0.1，实际隔离进程 | 6F908813BE768E31B968F03A1C9DFC3827223A0E22B10E906AFB375D26C90CB7 |
| FFmpegFreeUI.dll | 6.5.0.1 | 9B30123BBE260DBD5A31F310744D88E863F5BC16D2302FDCB0D7369DEE2A7B2D |
| FFmpegFreeUI.Ext.PluginHost.dll | 2.5.0.0 | 4E56231FBF4C2D49B0990B73F6E45D1F2B5A03E753FB796E6C64422DD247E299 |
| LakeUI.dll | 5.301 | 71869F199C5FB07C1E06F58AA12EEB3F98A46EE61EAC9886E01865B1095F6D44 |
| 修改前 SDK DLL | AssemblyVersion 2.0.0.0 / FileVersion 2.5.0.0 | 0B4291459C90BF1A353418A70320035ABC31AE8C39314E69F82561FBB400E469 |
| 修改前 C# 示例 DLL | .NET 10，未重新编译后部署 | 6B20A497D442FC283748C9EC42939FEA0A25004EFE509381C6475B66297EE8D8 |
| 修改前背景示例 DLL | .NET 10 / LakeUI 5.201 编译 | CC745C65CB61AD2BD606885E2A505981C6840A788633CC08B0B0CAECDF9FB523 |
| 新背景示例 DLL | .NET 11 / LakeUI 5.301 编译 | D242F033251E8C912207B54B1AE68BDEFBA91B8732FFD2FD6B44CD77AA177367 |

实际进程还核对了 Microsoft.NETCore.App 11.0.0 / Windows Forms 11.0.0 的加载路径；未误测用户原程序。

## 体积与上一轮基线

- 上一轮 6.3.0 + net10.0-windows + LakeUI 5.201 的框架依赖单文件 EXE 为 12.27 MiB，含两 Ext DLL 共 12.42 MiB；这是历史验证结果，不是当前 6.5.0 的发布物。
- 上游这次恢复带 Windows 17763 版本的 TFM，本项目按上游同步后，实际依赖图重新包含 Microsoft.Windows.SDK.NET.dll（23646008 字节）和 WinRT.Runtime.dll（598840 字节），会增加发布体积。
- 官方 6.5.0 GitHub Release 的 x64 EXE 为 37421435 字节 / 35.69 MiB，arm64 为 34.00 MiB。没有擅自覆盖本项目既有单文件发布物，本轮未测出本项目新的单文件大小。
- 本轮没有单独进行 TFM/裁剪/发布体积优化，不能继续把上一轮 12.27 MiB 写成当前体积。

## 既有问题和稳定约定

- 浅色插件详情低对比度是宿主 插件详情视图_v6.vb 的硬编码亮色造成；上一轮实际旧版对照已确认。本轮未改配色，也未重新验证浅色主题，仍待单独修复。
- 官方 API 优先，Ext 补充；统一 IExtFFmpegFreeUIHost，SDK AssemblyVersion 保持 2.0.0.0；SDK 兼容、LakeUI 依赖和插件自身行为分开判断。
- 自有更新从扩展版仓库检查，经用户确认升级。没有恢复官方强制更新器，也没有发布新 NuGet 包、打标签、推送或创建 Release。
- README 分界线后必须与已合入上游原文一致。
- 支持者 DLL：C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll，SHA-256 A08B0B3A00ECBF6AD2A83B39BE105D4C385D3AFD8DC4C5CE94D1C7E400D5741E，仅用于合法隔离验证。
- 本轮隔离副本及旧 DLL 备份均已在核实绝对路径、无重解析点、无运行进程后清理。artifacts/upstream-650-review-20261112-d9c518a7 只保留本轮哈希记录 JSON；未清理其他轮次或用户文件。
- upstream：https://github.com/Lake1059/FFmpegFreeUI.git；origin：https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git。
- 网络使用命令级 http://127.0.0.1:7897 代理与 Git OpenSSL，未更改全局配置、TLS 校验或系统代理。
