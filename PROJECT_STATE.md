# FFmpegFreeUI API Extended Edition 项目状态

> 这是供后续开发和交接使用的事实快照，不是第二份指令文件。工作规则以 `AGENTS.md` 为准；源码、项目文件和 Git 与本文件冲突时，以核实后的真实状态为准并更新本文件。不要在这里记录密钥或完整日志。

最后更新：2026-09-23

## 当前基线

| 项目 | 当前值 |
| --- | --- |
| 主分支 | `main` |
| 最近合入的上游版本 | `6.2.28` |
| 上游提交 | `7790f60`（tag `6.2.28`） |
| 最近上游合并提交 | `9ac5125`（`Merge upstream 6.2.28`） |
| 产品版本 | `v6.2.28-ext.2+v2.5` |
| Ext API / NuGet 版本 | `v2.5` / `2.5.0` |
| Ext SDK AssemblyVersion | `2.0.0.0`（兼容既有 2.x 插件） |
| 目标框架 | 宿主和 PluginHost：`net10.0-windows10.0.26100.0`；SDK：`net10.0-windows10.0.17763.0` |
| LakeUI | NuGet `5.101.0` |
| 主解决方案 | `FFmpegFreeUI-API-Extended-Edition.slnx` |

## 最近验证基线

工作树基于 `444ca3f`；本次自动更新改动尚未提交。

- Release 解决方案构建：0 错误、0 警告。
- `FFmpegFreeUI.RegressionTests`：391 项检查通过，新增 Release 资产校验、退出助手替换和失败回滚检查。
- 旧插件兼容样例 `E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1\FFmpegFreeUI.AbAv1.vbproj`：Release 构建 0 错误、0 警告。
- `git diff --check`：通过。
- 实际程序视觉验证：运行当前构建，并使用独立的 `v6.2.28-ext.0+v2.5` 单文件测试副本触发线上 `v6.2.28-ext.1+v2.5` Release；确认“立即更新”弹窗、卡片下载百分比和“关闭程序时安装”状态。
- 隔离端到端验证：关闭测试副本后，后台助手成功替换 EXE、PluginHost DLL 和 PluginSdk DLL，三份文件的 SHA-256 均与 GitHub Release 摘要一致；未触碰现有安装目录。因已有同版本安装实例正在运行，测试副本下一次启动的结果提示未做视觉验证。
- 个性化背景开启状态和其他上游页面的视觉回归不属于本次验证范围。
- 上述记录不能替代后续改动的重新验证。涉及界面或 LakeUI 的改动仍需使用实际构建的 FFmpegFreeUI 做视觉回归。

## 稳定设计决定

- 官方插件 API 与 Ext API 可以在同一插件中共同使用；能用官方 API 时优先使用官方 API。
- Ext API 是补充层，不以替换或复制官方 API 为目标。
- Ext 宿主使用统一的 `IExtFFmpegFreeUIHost`；尚未发布的版本后缀接口已经合并回统一接口。
- SDK 包版本与 API 能力版本同步，但 SDK `AssemblyVersion` 保持 `2.0.0.0`，以便已编译的 2.x 插件继续加载。
- LakeUI 使用 NuGet 引用；宿主和插件宿主当前统一为 `5.101.0`。
- README 的扩展版内容位于 `# 以下是上游仓库原内容` 之前，之后保持上游原文。
- 默认框架依赖单文件发布包含 `FFmpegFreeUI.exe`、Ext Plugin Host DLL 和 Ext Plugin SDK DLL。
- 主程序更新直接由宿主实现：从本项目 Release 下载三份匹配文件，校验 GitHub SHA-256、主程序版本/架构和 Ext DLL 版本；正常退出后由现有 PowerShell 助手机制替换并在失败时回滚。

## 环境与兼容性夹具

- 上游远程：`upstream = https://github.com/Lake1059/FFmpegFreeUI.git`
- 扩展版远程：`origin = https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition.git`
- 网络异常时可临时使用系统代理：`http://127.0.0.1:7897`。
- 旧插件兼容性源码：`E:\DesktopPlus\Works\Code\3fui_plugin_ab-av1`。
- 个性化背景完整激活需要测试环境中存在 `C:\Apps\FFmpegFreeUI ReadyToRun x64\FFmpegFreeUISupporter_v6.dll`，并将其复制到实际测试程序所需位置。测试报告应说明背景功能是否真正激活。

## 当前工作

- 活跃实现任务：无。
- 未解决的合并冲突：无。
- 验证待补：个性化背景开启状态和其他上游 UI 的视觉回归；本轮只验证自动更新相关界面与隔离安装流程。

## 更新本文件时

只保留最新、可复核的状态，并至少更新受影响的项目：

1. 上游版本、提交与合并提交。
2. 产品版本、Ext API/SDK 版本、目标框架和 LakeUI 版本。
3. 最近一次实际执行的构建、回归、旧插件兼容和视觉验证结果。
4. 尚未完成的工作、已知问题、阻塞原因和下一步。
5. 影响未来实现的兼容性或架构决定。
