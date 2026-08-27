# C# Ext Plugin API v2.5 综合示例

本示例用七个场景覆盖当前主要公共接口：

1. 自动质量策略：在参数面板中保存 CRF，在预设捕获和任务准备阶段写入结构化预设。
2. 命令与进程审计：修改任务名、输出后缀、进阶参数、最终命令和实际启动程序，并观察退出码。
3. 成功后校验：异步计算输出文件 SHA-256，通过 `ReportResult` 显示结果，最终释放任务缓存。
4. v2.3 扩展：枚举全部参数控件、装饰音频编码器、向音频页顶部加控件，并把声明式 metadata 和 `cmd.exe` 前置步骤同时带入预览、模板与执行队列。
5. v2.4 页面入口与工具栏扩展：在主导航和参数面板一级导航中插入页面，并在编码队列“定位”按钮右侧插入自定义按钮。
6. 插件设置入口：点亮插件详情卡片中的设置齿轮，在插件管理页面内部显示示例设置页。
7. v2.5 预设总览行：从完整预设中的插件私有状态生成总览，使当前参数面板和预设管理器选中项显示一致。

所有会改变任务或执行耗时工作的选项默认关闭。代码按职责拆成：

- `SamplePlugin.cs`：入口、版本/能力检测、所有注册、状态模型和并发任务缓存。
- `SamplePlugin.Ui.cs`：3 个装饰型锚点和 3 个插入型锚点。
- `SamplePlugin.Pipeline.cs`：14 个处理阶段，按真实调用顺序排列。
- `SamplePlugin.Commands.cs`：v2.3 参数面板目录、动态控件清理、页面插入和声明式命令计划。
- `SamplePlugin.PageEntriesAndToolbar.cs`：v2.4 主导航页、参数导航页和编码队列工具栏控件。
- `SamplePlugin.Settings.cs`：v2.4 插件管理器内的可选设置页。
- `SamplePlugin.PresetOverview.cs`：v2.5 基于完整预设快照的插件私有参数总览行。

## 接口覆盖

- `RegisterChoice`：用稳定 `ChoiceId` 向原生质量下拉框添加安全选项，不直接修改 `Items`。
- `Behaviors.Register`：在原生质量模式联动之后进行有序观察/变换。
- `Resources.Claim`：声明对原始控件的观察意图，展示冲突协调入口。

- `IExtFFmpegFreeUIPlugin`：`Id`、`DisplayName`、`Initialize`。
- `IExtFFmpegFreeUIHost`：`ApiVersion`、`HostVersion`、`PageEntries`、`EncodingQueueToolbar`、`PluginSettings`、`PresetOverview`、`Ui`、`Pipeline`、4 种 `Log` 级别。
- `IExtFFmpegFreeUIHost.PageEntries`：枚举稳定页面目标，并用 `RegisterPage` 相对插入任意数量的页面入口。
- `IExtFFmpegFreeUIHost.EncodingQueueToolbar`：枚举编码队列顶栏目标，并用 `RegisterControl` 插入自定义控件。
- `IExtFFmpegFreeUIHost.PluginSettings`：在插件详情卡片中提供齿轮入口，并按需创建一个设置页。
- `IExtFFmpegFreeUIHost.PresetOverview`：根据完整 `PresetJson` 和当前插件的 `PluginStateJson` 生成有序总览行。
- `IExtFFmpegFreeUIHost.ParameterPanel`：参数页与全部原生控件目录。
- `IExtFFmpegFreeUIHost.Commands`：声明式参数与外部命令步骤。
- `AvailablePages` / `AvailableControls`：页面插槽、全部原生控件锚点和动态资源 ID。
- `RegisterParameterProvider`：向 `BeforeOutput` 位置贡献可预览的参数。
- `RegisterStepProvider`：贡献由队列统一执行和取消的 `BeforeNative` 外部命令。
- `ExtPluginUiExtension.Cleanup`：还原对动态发现控件的修改和事件订阅。
- `IExtPluginUiRegistry`：`AvailableAnchors`、`Register`、注册句柄保存。
- `IExtPluginUiContext`：全部身份字段、两个控件字段、`GetAnchorControl`、`StateJson`、
  `StateRestored`、`RequestParameterRefresh`。
- `IExtPluginPipelineRegistry`：`AvailableStages`、`Register` 和 `Order`。
- `ExtPluginPipelineContext`：全部字段、阶段属性、取消令牌、`ReportProgress`、`ReportResult`。
- `ExtFFmpegFreeUIUiAnchors.All` 中的 6 个锚点。
- `ExtFFmpegFreeUIPipelineStages.All` 中的 14 个阶段。

示例保留旧的命令字符串修改以展示兼容接口；新增功能应优先使用声明式参数/步骤。生产插件仍须正确转义参数，并谨慎提供“接受非零退出码”“替换进程”或 shell 命令这类高风险选项。

## 构建和安装

在仓库根目录执行：

```powershell
dotnet build .\Samples\FFmpegFreeUI.Ext.PluginApi.Sample\FFmpegFreeUI.Ext.PluginApi.Sample.csproj -c Release
```

示例已经导入 SDK 的一键部署目标。目标 FFmpegFreeUI 完全退出后，可直接编译并部署：

```powershell
dotnet build .\Samples\FFmpegFreeUI.Ext.PluginApi.Sample\FFmpegFreeUI.Ext.PluginApi.Sample.csproj `
  -c Release -t:ExtDeployFFmpegFreeUIPlugin `
  -p:ExtFFmpegFreeUIInstallDir="D:\Apps\FFmpegFreeUI-API-Extended-Edition"
```

只把生成的 `FFmpegFreeUI.Ext.PluginApi.Sample.3fui.dll` 放到 FFmpegFreeUI 的 `Plugin` 目录。不要复制构建目录中的
`FFmpegFreeUI.Ext.PluginSdk.dll`；SDK 和 PluginHost 应由 FFmpegFreeUI 发行包统一放在程序根目录。
