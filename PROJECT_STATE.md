# FFmpegFreeUI API Extended Edition 项目状态

> 本文件是开发交接的事实快照。长期规则见 `AGENTS.md`；源码、项目文件和 Git 是最终事实来源。不要记录密钥或完整日志。

最后更新：2026-11-09

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | main |
| 最近合入的上游版本 | 6.3.0 |
| 上游提交 | 787fb527b2a1e4cd247899e3f295b6b2a4e836cc |
| 上游合并提交 | 10173eb84f11115700faa2a3adffd9e277e2cff8 |
| 合并前扩展版提交 | d7cf5f3edbf8fa88bdba9f214c2d25024b0d6522，开始时工作树干净 |
| 产品版本 / 文件版本 | v6.3.0-ext.1+v2.5 / 6.3.0.1 |
| Ext API / NuGet / SDK AssemblyVersion | v2.5 / 2.5.0 / 2.0.0.0 |
| 目标框架 | 宿主和 PluginHost：net10.0-windows10.0.26100.0；SDK：net10.0-windows10.0.17763.0 |
| LakeUI | 纯 NuGet 5.117.0，宿主、PluginHost、回归及背景样例一致 |
| LakeUI 包源码提交 | a8543d913a5083d07ed2eec95ead3ca27d2e9346 |
| 主解决方案 | FFmpegFreeUI-API-Extended-Edition.slnx |

## 本次合并

从 fc709fb 合入 5b2e8f1、787fb52 两个提交。项目版本元数据和 FormMain_v6.vb 启动加载处有文本冲突，均已解决；没有破坏性 SDK 冲突。

- 同步质量评测原视频路径编辑后的 600 ms 防抖保存，以及宿主关闭时刷新待保存状态。该 Timer 用于配置落盘，不用于重绘。新增空状态/空指标/空帧列表容错和文件列表去重优化。
- Agent JSON 和程序设置改用已有的原子文件写入辅助类，创建父目录、唯一临时文件及覆盖移动；清理临时文件失败不覆盖原始异常。同步 HTTP、元数据和混流字典 TryGetValue 清理。
- 启动时仅在 Plugin 目录存在时加载插件，且仅在未处理任务缓存存在时触发恢复流程。冲突处保留 Ext 主导航、参数面板和队列工具栏注册，并在它们之后执行插件目录判断。
- 保留品牌、自有更新、插件管理、官方/Ext 桥接及现有 Windows 目标框架。README 上游部分已逐字核对一致；未恢复本地 LakeUI DLL 覆盖入口。
- Ext SDK 目录相对 d7cf5f3 没有改动，未修改示例或第三方插件源码。程序集身份保持 2.0.0.0；这不等于所有旧 DLL 已完成运行时验证。
- 既有 Agent 取消修复仍保留：在 OperationCanceledException 的 Catch 正文中检查调用方令牌并重新抛出，不恢复有 Await 闭包问题的过滤器。

## LakeUI 核对

NuGet 官方最新稳定包现为 5.201.0，源码提交 d844f8dfc4b843797fb992cfb3ad9a95f16428ea。已审阅与 5.117 的官方差异，包括 ModernPanel 可选的全透明跳过呈现模式、背景依赖脏区传播、presenter 及批量呈现、动画和图表缓存优化。

本次上游没有更改 LakeUI 引用或调用新包 API，因此仍使用 5.117.0，没有混入独立的渲染机制升级。5.201 的 NuGet 资产目标为 net10.0-windows，而对应仓库默认源码仍写 net10.0-windows10.0.17763.0；后续若升级，应按实际包和程序集验证，不能只看仓库默认属性。VMAF 私有 _dropDownForm、_tipForm、ExFloatingTipForm 依赖也仍需升级时核对。

## 构建与自动回归

- 最终合并提交 10173eb 完整 Release 解决方案构建：**0 错误、0 警告**。
- 完整回归 **583 项通过**，退出代码 0。新增 9 项覆盖 Agent 原子状态的父目录/UTF-8/覆盖/临时文件/空值，以及质量评测不完整旧状态恢复、空帧数组规范化、编辑防抖、宿主关闭刷新及重复刷新。
- 旧插件源码 E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1 Release 构建：**0 错误、0 警告**。没有修改插件源码，旧部署 DLL 的运行时与评分问题按用户既有要求排除。
- 已有 .NET SDK 11.0.100-rc.1.26425.128；NETSDK1057 是预览 SDK 提示，非构建警告。目标仍为 .NET 10。
- 合并暂存区 diff --check 和 README 核对通过；未推送、打标签或发布 Release/NuGet。

## 本轮真实程序检查与停止位置

用 10173eb 的实际 Release 产物 publish --no-build --no-restore，补齐两个 Ext DLL，生成 DarkOff、DarkOn、LightOff、LightOn 四份隔离副本。都明确设置“微软雅黑”，不带用户原配置、模型端点或 API 密钥。背景开启副本部署已有合法支持者 DLL 和两个当前示例；背景关闭副本启动前不含 Plugin 目录。

**用户按物理 Esc 停止了 Computer Use；之后未再发出界面输入。本轮视觉验收未完成。**

实际已执行：

- 启动 DarkOff：无 Plugin 目录时进入真实主程序成功，插件管理入口仍存在，质量评测首次进入可见。
- 在实际质量评测原视频框输入 C:\Review\reference-630.mkv。读取该副本 Settings.json，确认程序仍在运行时已自动保存该路径，Plugin 目录仍不存在。
- 核对 DarkOff 实际进程加载的以下五个文件，版本和路径与本轮产物一致；Computer Use 使用 sky 的实际 DirectX 窗口捕获。
- 首次输入因用户切换窗口中断，重新观察/激活并明确焦点后才重试，没有把其他应用遮挡当成渲染缺陷。

| 实际加载文件 | SHA-256 |
| --- | --- |
| FFmpegFreeUI.exe（6.3.0.1） | 2ADAADD0F5D8FFC0438FAD044F4FF76B601E1AE2CDAB85FD154A0C14CBF265BB |
| FFmpegFreeUI.dll（6.3.0.1） | 933FEC233C57D3B887C858B3945DE6BF652453538202DAE8ED6A85C6281228DB |
| LakeUI.dll（5.117） | E3768E65FF60E15DB6EB04C7C41DF32820E43AB3578706E53B1E3D51E1D78814 |
| FFmpegFreeUI.Ext.PluginSdk.dll（文件 2.5.0.0） | FCDAAFE1A8AE655F86472ED667991309AC456EB273B1E59488C1652AB0DB6D79 |
| FFmpegFreeUI.Ext.PluginHost.dll（2.5.0.0） | 7E23ED78BB1350D1E3EDAFB53EC0E44918F8EF0D05E4D85691060873268FBB63 |

尚未执行本轮的背景开启/浅色实际检查、插件目录存在时启动、关闭后重开恢复、窗口缩放/最小化、多 DPI、精确时点截图和长期 CPU/GPU 测量。上轮 6.2.46 的四组合、媒体选择器和窗口恢复结果在 Git 历史中，不能算作本轮通过。

停止时 DarkOff 测试程序仍在运行。隔离目录 artifacts/upstream-630-review-20261109-c5b9d073 保留，未清理、未强制关闭进程；它只含本轮自建副本。后续继续时重新核对当前窗口、进程、文件和哈希，再操作；清理前检查绝对路径、重解析点和运行进程。

## 程序体积只读调查

用户在验证期间询问上游体积骤降而扩展版仍较大的原因；仅调查，没有修改目标框架、依赖或发布逻辑。

- 官方 6.2.40 x64 为 46.72 MiB，6.2.41 降到 11.05 MiB；最新 6.3.0 为 11.14 MiB（GitHub Release API）。
- 官方提交 16cc179 将宿主从 net10.0-windows10.0.26100.0 改为 net10.0-windows，同时删除 Microsoft.Agents.AI.OpenAI。6.2.41 发布说明明确写移除了多余组件和 .NET 投影。
- 扩展版此前合并保留了旧 Windows 目标框架；当前 project.assets.json 仍隐式引用 Microsoft.Windows.SDK.NET.Ref（10.0.26100.87）。实际构建中 Microsoft.Windows.SDK.NET.dll 为 **25.99 MiB**，WinRT.Runtime.dll 为 **0.57 MiB**。
- 本地现存单文件发布 EXE 为 **38.82 MiB**，两个 Ext DLL 总计约 **0.15 MiB**；该目录为用户既有产物，本轮未重新发布或覆盖。当前多文件隔离 Base 合计约 36.03 MiB。
- 发布脚本明确关闭裁剪、默认不压缩。体积主要差距来自 Windows SDK .NET 投影依赖，Ext 契约 DLL 体积很小。后续精简应检查宿主/PluginHost 目标框架和 LakeUI 包兼容性，不直接删除运行依赖；用户当前只询问原因，未实施精简。

## 稳定设计与环境

- 官方 API 优先、Ext 补充；统一 IExtFFmpegFreeUIHost，保持 SDK AssemblyVersion 2.0.0.0。
- LakeUI 纯 NuGet；最小 WinForms 示例不引入 LakeUI，背景测试使用独立插件。
- README 分界线前是扩展说明，之后原样同步上游。
- 框架依赖单文件默认附带主程序及两个 Ext DLL；更新由扩展版仓库检查并经用户确认。
- upstream：https://github.com/Lake1059/FFmpegFreeUI.git；origin：https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git。
- 命令级代理 http://127.0.0.1:7897 与 Git OpenSSL；未修改全局代理或 TLS 校验。
- 合法支持者 DLL 来源：C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll，仅用于隔离副本。
- 插件管理首次显示首项详情但列表尚未选中的既有问题未混入本轮：齿轮需要明确选中行。
