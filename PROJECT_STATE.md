# FFmpegFreeUI API Extended Edition 项目状态

> 本文件是开发交接的事实快照。长期规则见 AGENTS.md；源码、项目文件和 Git 是最终事实来源。不要记录密钥或完整日志。

最后更新：2026-11-09

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | main |
| 最近合入的上游版本 / 提交 | 6.3.0 / 787fb527b2a1e4cd247899e3f295b6b2a4e836cc |
| 上游合并提交 | 10173eb84f11115700faa2a3adffd9e277e2cff8 |
| 本轮目标框架与依赖提交 | ebbccd8a4022c63c57aa6e51e45046261914a9fb |
| 本轮开始状态 | 6c2a690，工作树干净，无 3FUI 进程 |
| 产品版本 / 文件版本 | v6.3.0-ext.1+v2.5 / 6.3.0.1，未自动升版 |
| Ext API / NuGet / SDK AssemblyVersion | v2.5 / 2.5.0 / 2.0.0.0 |
| 主程序与回归目标 | net10.0-windows，与上游一致 |
| PluginHost、SDK 和插件样例目标 | net10.0-windows10.0.17763.0；保留已发布 SDK 的最低编译目标 |
| LakeUI | 纯 NuGet 5.201.0，宿主、PluginHost、回归及背景样例一致 |
| LakeUI 包源码提交 | d844f8dfc4b843797fb992cfb3ad9a95f16428ea |
| 主解决方案 | FFmpegFreeUI-API-Extended-Edition.slnx |

## 本轮目标框架同步

用户在确认体积原因后授权同步目标框架并核对 LakeUI 兼容性。

- 主程序及回归项目改为 net10.0-windows，主程序 SupportedOSPlatformVersion 跟随上游为 7.0。这不是宣布 .NET 10 支持 Windows 7，实际系统要求仍取决于 .NET 10 和组件。
- LakeUI 5.117 的 NuGet 资产要求 Windows 17763 目标，不能直接配合上游的普通 Windows TFM，因此统一升级到官方 5.201.0 包。新包实际 lib/net10.0-windows 资产可用，不使用本地覆盖或兼容性回退。
- PluginHost 从 26100 降到已发布 SDK 要求的 17763，SDK 目录完全未改。PluginHost 是独立 DLL，不进入主程序的发布依赖图；主程序发布物无需附带 Windows SDK .NET 投影。
- PluginHost 构建后通过主项目的 GetTargetPath 获取实际输出目录，移除父项目 TargetFramework 全局属性后查询；不再假设两个项目的 TFM 相同。已核对新目录中复制的 PluginHost 哈希与构建结果一致。
- 没有改动 Ext SDK 的公共签名、程序集身份、API/NuGet 版本、插件源码或发布脚本。背景样例仅更新编译期 LakeUI 包引用；最小 WinForms 示例仍不依赖 LakeUI。
- 更新开发指南及第三方声明，移除已不在实际依赖图中的 Microsoft.Agents.AI.OpenAI 旧声明。README 上游原文保持不变。

## LakeUI 源码与兼容核对

已按 5.117 / 5.201 包对应提交审阅官方差异：

- ControlSurface 首次绘制必须先有完整内容，Parent/所属窗体变化时重新解析 compositor。
- presenter 在位图和 revision 连续时使用局部更新，句柄、设备等错误仍走恢复路径。
- 背景依赖先传播逻辑脏区，再按控件树由外到内批量呈现；跨 DPI 回退完整区域。
- ModernPanel 的全透明跳过呈现是新增可选模式，默认仍为 Normal。本项目没有启用它或更改背景布局架构。
- ModernTabListControl 的差异只清理注释；ModernComboBox/ModernTabControl 源码本轮没有改动。VMAF 的实际下拉、模型标签及浮动说明已在新宿主检查。
- 核对公开声明未发现既有成员删除；两个表面上的删除分别是 FloatingToolTipSide 枚举移位、ShadowWindow 方法重写。XML 文档从 1349 到 1358 项，唯一消失的名称是私有字段的注释条目 _moreIndicatorFont，不是公开 API 删除。该检查不等于所有第三方 DLL 的完整兼容认证。
- 两个 LakeUI 包的程序集引用均不直接引用 Windows SDK .NET 投影。新包仍声明 GPL-3.0-only。

## 构建与自动验证

在 ebbccd8 上完成完整还原、Release 构建与回归：

- **0 错误、0 警告；583 项回归通过，退出代码 0。**
- 未修改的旧插件源码 E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1 Release 构建 **0 错误、0 警告**，HEAD 87ea7f3，工作树干净。其旧部署 DLL 的运行时/评分问题按用户既有要求排除。
- SDK 目录相对 6c2a690 无改动，实际程序集身份继续为 FFmpegFreeUI.Ext.PluginSdk, Version=2.0.0.0。
- 使用已有 SDK 11.0.100-rc.1.26425.128；NETSDK1057 是预览 SDK 提示，非警告，目标仍为 .NET 10。
- 没有新增长延时、渲染 Timer、循环 Invalidate、自定义 GPU 视图或兼容垫片。

## 单文件发布验证

以现有脚本相同参数发布到本轮新建目录：Release、win-x64、框架依赖、单文件、包含原生库、不裁剪、不压缩、无调试符号。未覆盖用户既有发布目录，也未改其配置或插件。

发布结果位于：

artifacts/framework-sync-20261109-f19d0b2e/SingleFileFinal

| 文件 | 字节数 / 大小 | SHA-256 |
| --- | --- | --- |
| FFmpegFreeUI.exe | 12867960 / 12.27 MiB | 08A81D43611644CDEDE32BB5A0A54B79F2F48782349DA7927E503EA117FCD240 |
| FFmpegFreeUI.Ext.PluginHost.dll | 103936 / 0.10 MiB | 9564C25F97F1DA9209380D4D8122A180D1FD72DA1D46214DCBD1D9042C6444B4 |
| FFmpegFreeUI.Ext.PluginSdk.dll | 51200 / 0.05 MiB | CFED3A488D62FEA0CC9D4FA94F59F1F8386108F5B799A87256CE2ECB61B92258 |

三文件总计 **12.42 MiB**。本机既有发布 EXE 为 38.82 MiB，用于体积参照，未覆盖。当前发布 deps.json 已不含 Microsoft.Windows.SDK.NET 或 WinRT.Runtime；不是通过删除依赖文件或裁剪得到的结果。现有发布脚本下次发布会使用新的项目配置。

打包的 LakeUI 5.201.0 DLL 哈希为 7FA085E47AE157BD5E5B3CDDD2AFF7C6FDF0A9C4F37DC3EC9B458DC74616BBF9。单文件中的托管程序集在内存加载，不伪造独立 DLL 的运行路径；实际进程 EXE 的哈希与上表一致。

## 本轮真实程序与旧二进制验证

使用最终单文件 EXE 建立深色/浅色 × 个性化背景关闭/开启四份隔离副本，明确设置微软雅黑。背景开启部署已有合法支持者 DLL。四份故意使用本轮修改前已编译的 SDK DLL 和两个旧示例 DLL，没有重新编译测试插件来绕过二进制兼容问题。

实际进程加载的旧 SDK 哈希为 FCDAAFE1A8AE655F86472ED667991309AC456EB273B1E59488C1652AB0DB6D79，新 PluginHost 哈希为上表值。背景开启进程支持者 DLL 哈希 A08B0B3A00ECBF6AD2A83B39BE105D4C385D3AFD8DC4C5CE94D1C7E400D5741E。实际 EXE、SDK、PluginHost、支持者加载路径与哈希已核对；全部屏幕检查通过 Computer Use / sky 捕获实际 DirectX 窗口。

| 场景 | 实际结果 |
| --- | --- |
| 四种主题/背景组合启动，两个旧插件初始化 | 均执行；入口存在，实际检查的管理列表显示两个插件已加载 |
| 旧 LakeUI 5.117 编译背景样例的独立页面、插件专属蓝底及恢复 | 深色背景关闭/开启均执行，宿主全局背景未改变 |
| 旧背景样例的嵌套设置页 | 深色背景关闭/开启均执行，根背景链可见 |
| 纯 WinForms 旧示例设置页 | 浅色无背景实际打开，控件及正文可见 |
| 插件详情滚动 | 深色有背景执行，未观察到旧文字拖影；浅色对比度问题见下 |
| 质量评测首次进入、VMAF 模型下拉/说明/切换 | 深色与浅色有背景执行；稳定画面核对正常。截图捕获到正常开合动画，未当成故障 |
| 最大化/恢复、最小化/恢复 | 深色有背景质量评测页执行，模型与背景恢复；常规约 1199×699、最大化约 1707×1067 逻辑像素 |
| 新单文件状态保存、关闭后重开 | 浅色有背景输入本轮测试路径，读取隔离 Settings.json 确认落盘；重启后实际显示恢复路径，模型按上游规则回到 AUTO |
| 静止页面 CPU 样本 | 深色背景关/开各约 8 秒，归一化平均 CPU 0.414% / 0.406%，不是长期性能验收 |
| 多 DPI、指定 1280×720/1920×1080、精确立即/50/250/1000 ms、长期 GPU/无限重绘 | **未完成完整矩阵**，未改变用户系统显示设置，不能记为全部通过 |
| 旧 AB-AV1 DLL 与完整评分链 | 按既有范围排除；仅构建未修改源码 |

本轮没有用自建 WinForms 窗体替代实际主程序。浅色无背景首次抓图报 no monitor found for window，重新选择唯一窗口后捕获成功，没有持续错误。

### 发现并对照确认的既有问题

浅色模式插件详情仍用亮色文字，部分对比度不足。原因是宿主 插件详情视图_v6.vb 直接用 Color.Silver、半透明白色绘制，没有调用现有主题前景色映射；不是新 LakeUI 的首帧、背景采样或旧帧问题。

已把修改前真实 6.3.0 + LakeUI 5.117 产物复制到另一隔离目录，使用同字体/浅色设置与旧插件实际运行，屏幕复现同样的浅色详情。实际对照主 DLL 哈希 933FEC233C57D3B887C858B3945DE6BF652453538202DAE8ED6A85C6281228DB，LakeUI 哈希 E3768E65FF60E15DB6EB04C7C41DF32820E43AB3578706E53B1E3D51E1D78814。该既有配色问题没有混入本轮目标框架调整，仍待单独修复。

## 收尾与稳定约定

- 测试程序均已正常退出。核实绝对路径、重解析点和进程后，已清理本轮临时测试副本、旧版对照、基线备份和下载包，只保留 SingleFileFinal 三个交付文件。没有清理上轮产物或用户既有发布目录。
- 官方 API 优先，Ext 补充；统一 IExtFFmpegFreeUIHost，保持 SDK 2.0.0.0 程序集身份；SDK 兼容结论与 LakeUI/插件自身行为分开。
- README 分界线前是扩展说明，之后原样同步本次上游。
- 自有更新仍由扩展版仓库检查并经用户确认。未推送、打标签或发布 Release/NuGet。
- upstream：https://github.com/Lake1059/FFmpegFreeUI.git；origin：https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git。
- 命令级代理 http://127.0.0.1:7897 与 Git OpenSSL；未修改全局配置或 TLS 校验。
- 合法支持者 DLL 来源：C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll，仅用于隔离副本。
