# LakeUI 背景测试插件

这是**独立测试插件**，不会改变原有的纯 WinForms 综合示例。它只引用 Ext SDK 的页面入口和设置页接口，以及 LakeUI 的公开控件。

- 左侧的“LakeUI 背景测试”入口直接返回名为 `ModernPanel1` 的 `ModernPanel`。
- 插件管理中本插件的设置齿轮返回不透明的 `UserControl`，其内部包含同名、`DockStyle.Fill` 的 `ModernPanel`，用于验证嵌套根容器的背景映射。
- 两页各有独立按钮，在“宿主背景映射或普通底色”与“本插件的蓝色专属背景”之间切换。按钮只影响当前页面实例，不修改 FFmpegFreeUI 的全局个性化设置，也不会持久化；重新打开设置页会恢复初始状态。

在仓库根目录构建：

```powershell
dotnet build .\Samples\FFmpegFreeUI.Ext.BackgroundTest\FFmpegFreeUI.Ext.BackgroundTest.csproj -c Release
```

退出目标 FFmpegFreeUI 后，可一键部署：

```powershell
dotnet build .\Samples\FFmpegFreeUI.Ext.BackgroundTest\FFmpegFreeUI.Ext.BackgroundTest.csproj `
  -c Release -t:ExtDeployFFmpegFreeUIPlugin `
  -p:ExtFFmpegFreeUIInstallDir="D:\Apps\FFmpegFreeUI-API-Extended-Edition"
```

本插件只需部署自己的 `*.3fui.dll`；不要把 `LakeUI.dll` 或 Ext SDK 私有副本放进 `Plugin`。插件编译使用与本仓库宿主一致的 LakeUI 版本，运行时使用宿主根目录中的 LakeUI。LakeUI 的使用和分发仍须遵守其许可证。

测试时，分别在 FFmpegFreeUI 全局个性化背景关闭和真正开启的情况下：打开左侧测试页并反复切换按钮；再从插件管理打开本插件设置齿轮，重复切换。观察两页是否分别显示宿主背景或普通深色底色、蓝色插件背景，且切换一个页面不影响另一页或宿主设置。再测试切页、调整窗口大小及最小化/恢复。全局个性化背景开启需要宿主自身满足支持者授权条件；仅看见透明底色不算激活成功。
