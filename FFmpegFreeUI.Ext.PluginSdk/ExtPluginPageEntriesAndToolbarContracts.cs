using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace FFmpegFreeUI.Ext.PluginSdk;

/// <summary>
/// 在主导航或参数面板一级导航中注册插件页面入口。所有位置均使用稳定目标 ID，
/// 不需要引用 LakeUI，也不要依赖宿主控件序号。
/// </summary>
public interface IExtPluginPageEntryRegistry
{
    /// <summary>当前宿主实际提供的主导航和参数面板一级导航目标快照。</summary>
    IReadOnlyCollection<ExtPluginPageTargetDescriptor> AvailableTargets { get; }
    /// <summary>相对于一个原生导航目标注册完整插件页面。</summary>
    /// <param name="extension">页面 ID、目标、位置、标题、顺序、工厂和清理逻辑。</param>
    /// <returns>注销句柄；释放后宿主移除页面并执行清理逻辑。</returns>
    IDisposable RegisterPage(ExtPluginPageExtension extension);
}

/// <summary>
/// 在编码队列顶部工具栏中注册插件控件。所有位置均使用稳定目标 ID，
/// 不需要引用 LakeUI，也不要依赖宿主控件序号。
/// </summary>
public interface IExtPluginEncodingQueueToolbarRegistry
{
    /// <summary>当前宿主实际提供的编码队列工具栏目标快照。</summary>
    IReadOnlyCollection<ExtPluginToolbarTargetDescriptor> AvailableTargets { get; }
    /// <summary>相对于编码队列的一个原生按钮注册自定义控件。</summary>
    /// <param name="extension">控件 ID、目标、位置、顺序、工厂和清理逻辑。</param>
    /// <returns>注销句柄；释放后宿主移除控件并执行清理逻辑。</returns>
    IDisposable RegisterControl(ExtPluginToolbarControlExtension extension);
}

/// <summary>宿主中可注册插件页面入口的导航区域。</summary>
public enum ExtPluginPageEntryArea
{
    /// <summary>FFmpegFreeUI 窗口最左侧的主导航。</summary>
    MainNavigation,
    /// <summary>参数面板内部左侧的一级导航。</summary>
    ParameterPanelNavigation
}

/// <summary>
/// 相对于目标的位置。页面中的 Before/After 分别表示上方/下方；
/// 横向工具栏中的 Before/After 分别表示左侧/右侧。
/// </summary>
public enum ExtPluginRelativePosition
{
    /// <summary>纵向导航目标上方，或横向工具栏目标左侧。</summary>
    Before,
    /// <summary>纵向导航目标下方，或横向工具栏目标右侧。</summary>
    After
}

/// <summary>一个可以在其上方或下方插入插件页面的原生导航目标。</summary>
public sealed class ExtPluginPageTargetDescriptor
{
    /// <summary>创建页面目标描述符；一般只由宿主调用。</summary>
    /// <param name="targetId">稳定目标 ID。</param>
    /// <param name="displayName">面向用户的原生目标名称。</param>
    /// <param name="area">目标所在导航区域。</param>
    public ExtPluginPageTargetDescriptor(
        string targetId,
        string displayName,
        ExtPluginPageEntryArea area)
    {
        TargetId = targetId;
        DisplayName = displayName;
        Area = area;
    }

    /// <summary>稳定目标 ID；注册时应原样传回。</summary>
    public string TargetId { get; }
    /// <summary>面向用户的原生目标名称，仅用于展示和诊断。</summary>
    public string DisplayName { get; }
    /// <summary>目标所在导航区域。</summary>
    public ExtPluginPageEntryArea Area { get; }
}

/// <summary>一个可以在其左侧或右侧插入插件控件的原生工具栏目标。</summary>
public sealed class ExtPluginToolbarTargetDescriptor
{
    /// <summary>创建工具栏目标描述符；一般只由宿主调用。</summary>
    /// <param name="targetId">稳定目标 ID。</param>
    /// <param name="displayName">面向用户的原生按钮名称。</param>
    public ExtPluginToolbarTargetDescriptor(
        string targetId,
        string displayName)
    {
        TargetId = targetId;
        DisplayName = displayName;
    }

    /// <summary>稳定目标 ID；注册时应原样传回。</summary>
    public string TargetId { get; }
    /// <summary>面向用户的原生按钮名称，仅用于展示和诊断。</summary>
    public string DisplayName { get; }
}

/// <summary>向原生导航目标的上方或下方注册一个插件页面入口。</summary>
public sealed class ExtPluginPageExtension
{
    /// <summary>描述一个相对于原生导航目标插入的完整插件页面。</summary>
    /// <param name="id">当前插件内唯一且发布后稳定的扩展 ID。</param>
    /// <param name="targetId">来自 host.PageEntries.AvailableTargets 或 ExtFFmpegFreeUIPageTargets 的目标 ID。</param>
    /// <param name="position">目标上方或下方。</param>
    /// <param name="title">导航中显示的页面标题。</param>
    /// <param name="createPage">在 UI 线程创建未被其他容器占用的新页面控件。</param>
    public ExtPluginPageExtension(
        string id,
        string targetId,
        ExtPluginRelativePosition position,
        string title,
        Func<IExtPluginPageContext, Control> createPage)
    {
        Id = id;
        TargetId = targetId;
        Position = position;
        Title = title;
        CreatePage = createPage;
    }

    /// <summary>当前插件内唯一的注册 ID。一个插件可以注册任意数量的页面。</summary>
    public string Id { get; set; }
    /// <summary>稳定原生页面目标 ID。</summary>
    public string TargetId { get; set; }
    /// <summary>目标上方或下方。</summary>
    public ExtPluginRelativePosition Position { get; set; }
    /// <summary>导航中显示的页面标题。</summary>
    public string Title { get; set; }
    /// <summary>同一目标同一侧的排序值；较小值在前。</summary>
    public int Order { get; set; }
    /// <summary>在 UI 线程创建未被其他容器占用的新页面控件。</summary>
    public Func<IExtPluginPageContext, Control> CreatePage { get; set; }
    /// <summary>注销页面前在 UI 线程调用的可选清理逻辑。</summary>
    public Action<IExtPluginPageContext>? Cleanup { get; set; }
}

/// <summary>插件页面实例的宿主上下文。</summary>
public interface IExtPluginPageContext
{
    /// <summary>当前插件 ID。</summary>
    string PluginId { get; }
    /// <summary>当前页面扩展 ID。</summary>
    string ExtensionId { get; }
    /// <summary>稳定原生目标 ID。</summary>
    string TargetId { get; }
    /// <summary>页面所在导航区域。</summary>
    ExtPluginPageEntryArea Area { get; }
    /// <summary>相对于原生目标的位置。</summary>
    ExtPluginRelativePosition Position { get; }
    /// <summary>插件工厂返回的页面控件；调用 CreatePage 期间尚未赋值，此时为 null。</summary>
    Control? PageControl { get; }

    /// <summary>
    /// 参数面板导航页为 true，此时 StateJson 会随 v6 预设保存和恢复；
    /// 主导航页为 false，此时 StateJson 只在本次页面实例中保存。
    /// </summary>
    bool SupportsPresetState { get; }
    /// <summary>按插件 ID 隔离的有效 JSON 状态。</summary>
    string StateJson { get; set; }
    /// <summary>宿主从预设恢复插件状态后触发；主导航页面不会触发。</summary>
    event EventHandler? StateRestored;
    /// <summary>请求宿主刷新参数总览和预览；主导航页面调用时不执行操作。</summary>
    void RequestParameterRefresh();
}

/// <summary>向原生工具栏目标的左侧或右侧注册一个自定义 WinForms 控件。</summary>
public sealed class ExtPluginToolbarControlExtension
{
    /// <summary>描述一个相对于编码队列原生按钮插入的自定义控件。</summary>
    /// <param name="id">当前插件内唯一且发布后稳定的扩展 ID。</param>
    /// <param name="targetId">来自 host.EncodingQueueToolbar.AvailableTargets 或 ExtFFmpegFreeUIToolbarTargets 的目标 ID。</param>
    /// <param name="position">目标左侧或右侧。</param>
    /// <param name="createControl">在 UI 线程创建未被其他容器占用的新控件。</param>
    public ExtPluginToolbarControlExtension(
        string id,
        string targetId,
        ExtPluginRelativePosition position,
        Func<IExtPluginToolbarContext, Control> createControl)
    {
        Id = id;
        TargetId = targetId;
        Position = position;
        CreateControl = createControl;
    }

    /// <summary>当前插件内唯一的注册 ID。一个插件可以注册任意数量的工具栏控件。</summary>
    public string Id { get; set; }
    /// <summary>稳定原生工具栏目标 ID。</summary>
    public string TargetId { get; set; }
    /// <summary>目标左侧或右侧。</summary>
    public ExtPluginRelativePosition Position { get; set; }
    /// <summary>同一目标同一侧的排序值；较小值在前。</summary>
    public int Order { get; set; }
    /// <summary>在 UI 线程创建未被其他容器占用的新控件。</summary>
    public Func<IExtPluginToolbarContext, Control> CreateControl { get; set; }
    /// <summary>注销控件前在 UI 线程调用的可选清理逻辑。</summary>
    public Action<IExtPluginToolbarContext>? Cleanup { get; set; }
}

/// <summary>插件工具栏控件实例的宿主上下文。</summary>
public interface IExtPluginToolbarContext
{
    /// <summary>当前插件 ID。</summary>
    string PluginId { get; }
    /// <summary>当前工具栏扩展 ID。</summary>
    string ExtensionId { get; }
    /// <summary>稳定原生工具栏目标 ID。</summary>
    string TargetId { get; }
    /// <summary>相对于原生按钮的位置。</summary>
    ExtPluginRelativePosition Position { get; }
    /// <summary>插件工厂返回的工具栏控件；调用 CreateControl 期间尚未赋值，此时为 null。</summary>
    Control? ExtensionControl { get; }
    /// <summary>编码队列顶部工具栏；仅用于读取布局信息。</summary>
    Control ToolbarControl { get; }
    /// <summary>作为相对位置目标的原生按钮；不要修改或重新挂载。</summary>
    Control TargetControl { get; }
    /// <summary>宿主将赋给插件控件的推荐统一高度。</summary>
    int RecommendedHeight { get; }
    /// <summary>原生目标当前的每英寸点数，可用于 DPI 尺寸计算。</summary>
    int DeviceDpi { get; }
}

/// <summary>主导航和参数面板一级导航的稳定目标 ID。</summary>
public static class ExtFFmpegFreeUIPageTargets
{
    public const string MainStart = "ext.navigation.main.start";
    public const string MainEncodingQueue = "ext.navigation.main.encoding-queue";
    public const string MainPrepareFiles = "ext.navigation.main.prepare-files";
    public const string MainParameters = "ext.navigation.main.parameters";
    public const string MainAgent = "ext.navigation.main.agent";
    public const string MainStudios = "ext.navigation.main.studios";
    public const string MainMediaInfo = "ext.navigation.main.media-info";
    public const string MainDebugPlayer = "ext.navigation.main.debug-player";
    public const string MainPerformance = "ext.navigation.main.performance";
    public const string MainIntegratedTools = "ext.navigation.main.integrated-tools";
    public const string MainSettings = "ext.navigation.main.settings";
    public const string MainSupporters = "ext.navigation.main.supporters";
    public const string MainPluginManager = "ext.navigation.main.plugin-manager";

    public const string ParametersOverview = "ext.navigation.parameters.overview";
    public const string ParametersPresets = "ext.navigation.parameters.presets";
    public const string ParametersOutput = "ext.navigation.parameters.output";
    public const string ParametersDecoder = "ext.navigation.parameters.decoder";
    public const string ParametersVideoEncoder = "ext.navigation.parameters.video-encoder";
    public const string ParametersVideoFrame = "ext.navigation.parameters.video-frame";
    public const string ParametersVideoQuality = "ext.navigation.parameters.video-quality";
    public const string ParametersColor = "ext.navigation.parameters.color";
    public const string ParametersFrameServer = "ext.navigation.parameters.frame-server";
    public const string ParametersAudio = "ext.navigation.parameters.audio";
    public const string ParametersTrim = "ext.navigation.parameters.trim";
    public const string ParametersFilterOrder = "ext.navigation.parameters.filter-order";
    public const string ParametersCustom = "ext.navigation.parameters.custom";
    public const string ParametersStreamControl = "ext.navigation.parameters.stream-control";
    public const string ParametersAdditional = "ext.navigation.parameters.additional";

    public static IReadOnlyCollection<string> All { get; } = new ReadOnlyCollection<string>(
        new[]
        {
            MainStart,
            MainEncodingQueue,
            MainPrepareFiles,
            MainParameters,
            MainAgent,
            MainStudios,
            MainMediaInfo,
            MainDebugPlayer,
            MainPerformance,
            MainIntegratedTools,
            MainSettings,
            MainSupporters,
            MainPluginManager,
            ParametersOverview,
            ParametersPresets,
            ParametersOutput,
            ParametersDecoder,
            ParametersVideoEncoder,
            ParametersVideoFrame,
            ParametersVideoQuality,
            ParametersColor,
            ParametersFrameServer,
            ParametersAudio,
            ParametersTrim,
            ParametersFilterOrder,
            ParametersCustom,
            ParametersStreamControl,
            ParametersAdditional
        });
}

/// <summary>编码队列顶部工具栏中全部原生按钮的稳定目标 ID。</summary>
public static class ExtFFmpegFreeUIToolbarTargets
{
    public const string EncodingQueueTaskMenu = "ext.toolbar.encoding-queue.task-menu";
    public const string EncodingQueueStart = "ext.toolbar.encoding-queue.start";
    public const string EncodingQueuePause = "ext.toolbar.encoding-queue.pause";
    public const string EncodingQueueResume = "ext.toolbar.encoding-queue.resume";
    public const string EncodingQueueStop = "ext.toolbar.encoding-queue.stop";
    public const string EncodingQueueRemove = "ext.toolbar.encoding-queue.remove";
    public const string EncodingQueueReset = "ext.toolbar.encoding-queue.reset";
    public const string EncodingQueueLocate = "ext.toolbar.encoding-queue.locate";

    public static IReadOnlyCollection<string> All { get; } = new ReadOnlyCollection<string>(
        new[]
        {
            EncodingQueueTaskMenu,
            EncodingQueueStart,
            EncodingQueuePause,
            EncodingQueueResume,
            EncodingQueueStop,
            EncodingQueueRemove,
            EncodingQueueReset,
            EncodingQueueLocate
        });
}
