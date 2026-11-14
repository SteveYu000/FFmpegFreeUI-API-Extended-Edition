# FFmpegFreeUI API Extended Edition 项目状态

> 本文件是事实快照；长期工作约定见 AGENTS.md，源码与 Git 为最终依据。不保存密钥、个人数据或完整日志。

最后更新：2026-11-14

## 当前工作结果：上游 6.5.1 与页面入口调整

- 本轮起点 main / 6a7cda5，工作树干净；合入 upstream/main = 2ad63956e9bb7b8d8b11d5c5b52ea6048c093913（2026/11/14-1，6.5.1）。本轮仅在本地处理，不推送、不发布。
- 参数总览入口移到主侧栏原准备文件的位置；准备文件入口移到参数面板的预设管理下方。参数面板默认打开预设管理，原有参数编辑页顺序保持。
- 复用原页面实例并调整绑定、刷新和背景映射，不复制页面、不引入新的 GPU 容器或渲染轮询。
- 完整构建和下述功能检查完成；完整多 DPI / 精确时序渲染矩阵未执行，不能据此声称所有显示配置已验收。

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 分支 | main |
| 上游版本 / 提交 | 6.5.1 / 2ad63956e9bb7b8d8b11d5c5b52ea6048c093913 |
| 产品版本 / 文件版本 | v6.5.1-ext.1+v2.5 / 6.5.1.1 |
| Ext API / NuGet / SDK AssemblyVersion | v2.5 / 2.5.0 / 2.0.0.0，不变 |
| 主程序与回归目标 | net11.0-windows，保留精简设置；SupportedOSPlatformVersion 7.0 不代表支持 Windows 7 |
| PluginHost 与 LakeUI 背景样例目标 | net11.0-windows10.0.17763.0 |
| SDK、普通 C# / VB 样例目标 | net10.0-windows10.0.17763.0 |
| LakeUI | 纯 NuGet 5.301.0，本轮无升级、无本地 DLL 覆盖 |
| LakeUI 包声明的源码提交 | d844f8dfc4b843797fb992cfb3ad9a95f16428ea |
| 验证环境 | .NET SDK 11.0.100 / .NET Desktop Runtime 11.0.0 / win-x64 |
| 解决方案 | FFmpegFreeUI-API-Extended-Edition.slnx |

## 上游合并与冲突处理

- 上游新增 QueueTasks.json 全队列持久化、启动自动恢复、恢复进程状态及耗时显示；调整文字选中颜色、UltraDetailListView 内部控件主题和预设总览刷新/提示。
- 上游未更改 LakeUI 引用。已查询 NuGet 官方索引，当前最新仍为 5.301.0；已用包为 net11.0-windows，依赖 Vortice 3.8.3，源码提交未变化。
- 三处文本冲突已逐项解决：主项目版本号；其他初始化的退出保存；编码任务的启动准备。保留扩展版品牌、自有更新/重启助手、精简 TFM、窗口布局保存时机和 Ext before-prepare 的修改/错误修复顺序。
- 用上游新持久化退出流程替换旧的“保留未执行任务”退出选择及恢复弹窗包装方法；旧缓存读写 API 保留，兼容上游旧缓存迁移与既有调用。
- 主程序仍使用扩展版仓库检查更新并等待用户确认，未恢复上游强制升级器或本机 LakeUIAssembly 引用。

### 本轮发现并修复的队列问题

1. 上游恢复监视器把 Nothing 作为 Timer state，却在进程退出时将 state 转成 Timer 并 Dispose，会触发空引用。现在保留监视器引用，先初始化再启动，使用实际 timer 回收并清空持有引用。
2. 恢复任务只按 PID 判断/控制进程，存在 PID 被系统复用后认错进程的风险。现在恢复、暂停/恢复控制和停止前均核对保存的进程启动 UTC 时间；缺少身份或不匹配时不接管其他进程。
3. 回归发现正常步骤结束后仍会把旧 PID 当成当前 PID 返回。现在清理当前进程时清空恢复标记，只有确实处于恢复状态时才返回恢复 PID；保留原有任务/步骤执行顺序。

这些 Timer 用于上游队列文件保存和进程监视，不参与绘制。没有通过 Timer、延时或循环 Invalidate 掩盖界面问题。

## 页面与旧 SDK 兼容

- 总览仍使用原参数面板的私有界面_参数总览；主导航选中时刷新，参数变动时也可刷新当前可见总览。实际修改输出后缀后已确认新总览显示 .mkv。
- 准备文件仍使用原页面，绑定到参数面板现有 ModernTabListControl；个性化背景使用现有 ModernPanel1 / BackgroundSource 链。
- 两个历史目标 ID 保留并跟随页面实际位置：ext.tabpage.parameters.overview 在主导航，ext.tabpage.main.prepare-files 在参数面板。Area 描述当前实际区域。
- 导航位置和预设状态能力独立：插在总览附近的旧插件页面继续 SupportsPresetState=true，准备文件附近继续 false。未改变旧插件的预设/会话状态保存方式。
- SDK 只更新 XML 说明，无公开签名、常量值、接口成员或程序集身份变动；开发指南同步目标表、历史 ID 及 SupportsPresetState 判断方式。
- 官方插件普通入口仍在侧栏底部插件区域、位于插件管理之后，路径未改。没有修改通用示例或第三方插件源码。

## 自动验证

- 完整 restore + Release 解决方案构建：0 错误、0 警告。
- 完整回归：621 项通过、退出代码 0。新增进程身份校验测试；根据上游文字选中色行为更新旧预期，同时继续检查下拉选中色的浅色映射、深色恢复和透明度。
- 既有进程退出尾部输出、取消、调度、输出预留、Ext before-prepare 错误恢复及更新/回滚检查均通过。
- 未修改的旧 AB-AV1 源码 E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1 Release 构建：0 错误、0 警告，工作树保持干净；按既有范围不测该插件旧 DLL 或评分链。
- README 分界线后的上游内容逐字核对；git diff --check / 暂存区检查通过。发布保持 EXE + 两个 Ext DLL，不含运行时、无裁剪、无 Windows SDK .NET 投影。

## 真实程序检查及范围

使用 artifacts/upstream-651-review-20261114-e31a7f90/SingleFile 的实际单文件发布物建立 FinalOff / FinalOn，部署未重新编译的旧 SDK，并用该旧 SDK 编译独立导航兼容性测试插件。用户 C:\Apps 原程序（PID 23396）及其配置、插件、媒体未改；没有覆盖既有正式发布目录。

| 场景 | 实际结果 |
| --- | --- |
| 无背景：首次总览、参数面板、准备文件 | 入口位置符合要求，预设管理默认选中，两个页面内容可见 |
| 无背景：修改后缀后返回总览 | 总览更新为 .mkv，旧警告消失 |
| 旧 SDK 插件目标注册 | 旧二进制 SDK 下编译的探针在两个移动目标后成功插页，实际区域正确 |
| 旧 SDK 状态能力 | 总览目标 true、准备文件目标 false；两处 StateJson 读写往返均通过，记录在 NavigationProbe.jsonl |
| 个性化图片背景真正开启 | 使用合法支持者 DLL、自定义图像、模式 2 / 图像来源 0；实际截图可见背景，不以透明底色代替激活 |
| 有背景：总览、预设管理、准备文件及排序弹出菜单 | 页面背景正确继承；检查菜单打开/关闭及页面切换后的显示 |
| 有背景：普通 / 最大化 / 还原 | 约 1199×699 / 1707×1067 逻辑像素下检查两个页面，内容与背景完成显示 |
| 上游新队列恢复 | 创建本轮专用 30 秒进程及队列快照；真实宿主显示运行中/断开，进程自然退出后显示已停止，PID 清零、耗时保留，宿主未崩溃 |
| 最终视觉回归 | 同一最终 EXE 再次实际打开总览和准备文件；随后正常退出隔离副本 |
| 多 DPI 100%/125%/150%/200%、指定 1280×720/1920×1080、精确立即/50/250/1000 ms、连续下拉选择、最小化恢复、长期 CPU/GPU | 本轮未完成完整矩阵；不将普通/最大化烟测或数学回归写成这些项目已通过 |

Computer Use 初次复制普通构建输出漏掉运行依赖，出现缺库提示；已关闭该副本并改用完整正式发布物复测，未据此修改宿主依赖代码。背景初始测试配置未选图像来源，已在隔离配置补齐后实际确认图片背景。用户手动操作测试窗口导致输入保护拦截时已暂停，收到“现在可以继续测试”后才继续。

## 本轮产物与证据

目录：artifacts/upstream-651-review-20261114-e31a7f90。测试插件仅位于隔离目录，不提交、也不部署到用户安装。

| SingleFile 文件 | 大小 / SHA-256 |
| --- | --- |
| FFmpegFreeUI.exe | 12798328 字节（12.21 MiB）；6CB011B4212DEBCB54CDD6DBC0769A6B8B2957BFAF76E1CB9CA97D83EEB9E941 |
| FFmpegFreeUI.Ext.PluginHost.dll | 103936 字节；047DDBCCF7F8802DA6DA1E36A346D07D2E72E0D5394878204AA36191B7356EFE |
| FFmpegFreeUI.Ext.PluginSdk.dll | 51200 字节；BFD55167499A2BA068C37FCEDE188C169303C2CEFBB71F5EA7981795B0563F91 |
| 实际兼容测试使用的旧 SDK | AssemblyVersion 2.0.0.0 / FileVersion 2.5.0.0；0B4291459C90BF1A353418A70320035ABC31AE8C39314E69F82561FBB400E469 |

三文件共 12953464 字节（12.35 MiB）。保留 final-publish-hashes.json、actual-process-modules.json、queue-recovery-result.json、测试进程及插件日志。实际加载 .NET 11.0.0、Host 和旧 SDK 的路径/哈希已核对；最终 EXE 的两份测试副本哈希相同。

隔离目录保留供复查；FinalOff 被用户手动浏览过，不还原或清理其配置。无发布、推送、打标签或 NuGet 操作。

## 保留的既有行为与问题

- 6a7cda5 的精简 TFM、主窗口尺寸/最大化记忆、实际顶层父窗口解析和功能子窗口同尺寸行为继续保留。系统文件/字体/颜色对话框及紧凑提示仍使用原布局。
- 浅色插件详情硬编码亮色造成的低对比度仍是既有问题，本轮未修改配色或宣称修复。
- 官方 API 优先，Ext 补充；SDK AssemblyVersion 固定 2.0.0.0。SDK、LakeUI 依赖和插件自身行为分别判断兼容性。
- 合法支持者 DLL：C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll；SHA-256 A08B0B3A00ECBF6AD2A83B39BE105D4C385D3AFD8DC4C5CE94D1C7E400D5741E。
- upstream：https://github.com/Lake1059/FFmpegFreeUI.git；origin：https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git。
- 网络使用命令级 http://127.0.0.1:7897 代理和 Git OpenSSL，不修改全局配置或 TLS 校验。
