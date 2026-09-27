# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-09-27

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.31` |
| 上游提交 | `30e7406`（tag `6.2.31`） |
| 最近上游合并提交 | `dddaf9b`（`Merge upstream 6.2.31`） |
| 产品版本 | `v6.2.31-ext.1+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.109.0`（宿主、PluginHost、回归测试一致） |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

上游合并提交：`dddaf9b`。2026-09-26 获取的 `upstream/main` 为 `30e7406`（tag `6.2.31`），已合入当前主线。本轮上游更新了手动停止任务后的调度行为、设置页布局和 LakeUI 许可证页；官方 README 未变化。合并时保留扩展版项目身份、LakeUI `5.109.0` NuGet 引用及 Ext SDK 公共契约。上游现在仅阻止“手动停止的任务自身”推进队列，其他任务自然完成仍会继续调度；扩展版原有“手动停止全局暂停调度”测试与此冲突，已将测试改为分别验证这两种情况，未恢复已删除的全局暂停标记。

- Release 解决方案构建：0 错误、0 警告。
- `FFmpegFreeUI.RegressionTests`：419 项检查通过；含手动停止不自行推进队列、其他任务自然完成继续调度，以及既有插件 API、更新器与预设检查。
- 旧插件兼容样例 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj`：本次重新 Release 构建，0 错误、0 警告；`6.2.30` 基线曾在实际宿主加载并打开其页面，本次未重复运行该插件或真实编码任务。
- `git diff --check`：通过。
- README 标记后的上游段与当前 `upstream/main` 逐字一致。Release 主程序文件版本为 `6.2.31.1`、产品版本为 `v6.2.31-ext.1+v2.5`，`FFmpegFreeUI.exe` SHA-256 为 `52428AE17A532D1BFF1ABE1F0FA4FAE5B62883634B8ADFDAA5846500BF85D2CB`，`FFmpegFreeUI.dll` SHA-256 为 `F39EE35CEDDB87AA3071D63C7EA998F7901D24BF60267678CF87499C6558C9FD`；Ext SDK `AssemblyVersion` 仍为 `2.0.0.0`，实际输出的 LakeUI DLL 文件版本为 `5.109`、SHA-256 为 `FC61D41BFF8139EB15BAECD1CD3CC5CF6CF247E2A7549AEC7C14F3094369A11F`。
- 使用与本次 Release 主程序 DLL 哈希一致的隔离版实际 3FUI 进行 Computer Use 视觉检查：复制 `FFmpegFreeUISupporter_v6.dll`，分别在个性化背景关闭与重启后真正开启时查看起始页、设置导航、LakeUI 许可证页和 Agent 设置页；背景开启时还查看插件管理、Ext 示例插件设置页、最大化及最小化恢复。未见持续黑块、旧帧或控件重叠；未做精确帧时序、目标 DPI/分辨率矩阵或定量性能采样。隔离副本测试后已关闭并删除。
- 上一轮尚未提交的 Ext 示例设置页改动仍保留：示例使用最小纯 WinForms 不透明控件，不依赖 LakeUI，故内容区域不会透出个性化背景；宿主可为第三方插件返回的 `ModernPanel1` GPU 根提供 `BackgroundSource`。本轮合并没有改动该公共契约或样例实现。
- 设置页背景映射现在与普通插件页面共用 `ModernPanel1` 查找逻辑，允许页面本身或其内部的 `DockStyle.Fill` LakeUI 根面板；SDK 公共契约与纯 WinForms 示例未改变。独立的 `Samples/FFmpegFreeUI.Ext.BackgroundTest` 同时覆盖直接根页面和嵌套根设置页，两个页面可分别切换宿主映射/普通底色与插件自有蓝色背景。Release 解决方案构建为 0 错误、0 警告，回归测试 419 项通过，旧 AB-AV1 插件构建为 0 错误、0 警告。新插件 DLL 与部署到实际编译宿主的 DLL 哈希一致，`Plugin` 目录未新增 LakeUI 私有副本。真实 3FUI 中，两页在个性化背景关闭、以及复制支持者 DLL 并重启后开启干净玻璃时均能切换和恢复；开启时左侧页确认获得非空背景映射。设置页最大化、最小化恢复、还原窗口后未见持续黑块或遮挡。测试后玻璃背景设置恢复为关闭，临时支持者 DLL 已删除；未做精确帧时序或多 DPI 验证。
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

- 活跃实现任务：无。独立 LakeUI 背景测试插件已加入解决方案并完成构建、回归及真实宿主开/关背景视觉检查；现有纯 WinForms 综合示例未改变。
- 未解决的合并冲突：无。
- 验证待补：如需完整视觉验收，还需精确帧时序、目标 DPI/分辨率矩阵及定量性能采样。小字号中文发虚的具体成因未确认；本轮观察到插件管理初次展示首个插件详情时，先点击插件列表行后齿轮才会打开设置页，尚未确认根因，未纳入本次上游合并修复；AB-AV1 本轮仅构建未运行，实际编码未执行；主程序更新首次提示需有更高版本 Release 才能界面验证。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
