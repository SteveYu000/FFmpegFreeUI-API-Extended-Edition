using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace FFmpegFreeUI.Ext.PluginSdk;

/// <summary>
/// 在主窗口或参数面板的 TabList 中注册插件页面入口。所有位置均使用稳定目标 ID，
/// 不需要引用 LakeUI，也不要依赖宿主控件序号。
/// </summary>
public interface IExtPluginPageEntryRegistry
{
    /// <summary>当前宿主实际提供的主窗口和参数面板 TabPage 目标快照。</summary>
    IReadOnlyCollection<ExtPluginPageTargetDescriptor> AvailableTargets { get; }
    /// <summary>相对于一个原生 TabPage 目标注册完整插件页面。</summary>
    /// <param name="extension">页面 ID、目标、位置、标题、顺序、工厂和清理逻辑。</param>
    /// <returns>注销句柄；释放后宿主移除页面并执行清理逻辑。</returns>
    IDisposable RegisterPage(ExtPluginPageExtension extension);
}

/// <summary>宿主中可注册插件页面入口的 TabList 区域。</summary>
public enum ExtPluginPageEntryArea
{
    /// <summary>FFmpegFreeUI 主窗口最左侧的 ModernTabListControl。</summary>
    MainTabList,
    /// <summary>参数面板内部左侧的 ModernTabListControl。</summary>
    ParameterPanelTabList
}

/// <summary>一个可以在其上方或下方插入插件页面的原生 TabPage 目标。</summary>
public sealed class ExtPluginPageTargetDescriptor
{
    /// <summary>创建页面目标描述符；一般只由宿主调用。</summary>
    /// <param name="targetId">稳定目标 ID。</param>
    /// <param name="displayName">面向用户的原生目标名称。</param>
    /// <param name="area">目标所在 TabList 区域。</param>
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
    /// <summary>目标所在 TabList 区域。</summary>
    public ExtPluginPageEntryArea Area { get; }
}

/// <summary>向原生 TabPage 目标的上方或下方注册一个插件页面入口。</summary>
public sealed class ExtPluginPageExtension
{
    /// <summary>描述一个相对于原生 TabPage 目标插入的完整插件页面。</summary>
    /// <param name="id">当前插件内唯一且发布后稳定的扩展 ID。</param>
    /// <param name="targetId">来自 host.PageEntries.AvailableTargets 或 ExtFFmpegFreeUIPageTargets 的目标 ID。</param>
    /// <param name="position">目标上方或下方。</param>
    /// <param name="title">TabList 中显示的页面标题。</param>
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
    /// <summary>TabList 中显示的页面标题。</summary>
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
    /// <summary>页面所在 TabList 区域。</summary>
    ExtPluginPageEntryArea Area { get; }
    /// <summary>相对于原生目标的位置。</summary>
    ExtPluginRelativePosition Position { get; }
    /// <summary>插件工厂返回的页面控件；调用 CreatePage 期间尚未赋值，此时为 null。</summary>
    Control? PageControl { get; }

    /// <summary>
    /// 参数面板 TabPage 为 true，此时 StateJson 会随 v6 预设保存和恢复；
    /// 主窗口 TabPage 为 false，此时 StateJson 只在本次页面实例中保存。
    /// </summary>
    bool SupportsPresetState { get; }
    /// <summary>按插件 ID 隔离的有效 JSON 状态。</summary>
    string StateJson { get; set; }
    /// <summary>宿主从预设恢复插件状态后触发；主窗口页面不会触发。</summary>
    event EventHandler? StateRestored;
    /// <summary>请求宿主刷新参数总览和预览；主窗口页面调用时不执行操作。</summary>
    void RequestParameterRefresh();
}

/// <summary>主窗口和参数面板 TabList 中全部原生 TabPage 的稳定目标 ID。</summary>
public static class ExtFFmpegFreeUIPageTargets
{
    public const string MainStart = "ext.tabpage.main.start";
    public const string MainEncodingQueue = "ext.tabpage.main.encoding-queue";
    public const string MainPrepareFiles = "ext.tabpage.main.prepare-files";
    public const string MainParameters = "ext.tabpage.main.parameters";
    public const string MainAgent = "ext.tabpage.main.agent";
    public const string MainStudios = "ext.tabpage.main.studios";
    public const string MainMediaInfo = "ext.tabpage.main.media-info";
    public const string MainDebugPlayer = "ext.tabpage.main.debug-player";
    public const string MainPerformance = "ext.tabpage.main.performance";
    public const string MainIntegratedTools = "ext.tabpage.main.integrated-tools";
    public const string MainSettings = "ext.tabpage.main.settings";
    public const string MainSupporters = "ext.tabpage.main.supporters";
    public const string MainPluginManager = "ext.tabpage.main.plugin-manager";

    public const string ParametersOverview = "ext.tabpage.parameters.overview";
    public const string ParametersPresets = "ext.tabpage.parameters.presets";
    public const string ParametersOutput = "ext.tabpage.parameters.output";
    public const string ParametersDecoder = "ext.tabpage.parameters.decoder";
    public const string ParametersVideoEncoder = "ext.tabpage.parameters.video-encoder";
    public const string ParametersVideoFrame = "ext.tabpage.parameters.video-frame";
    public const string ParametersVideoQuality = "ext.tabpage.parameters.video-quality";
    public const string ParametersColor = "ext.tabpage.parameters.color";
    public const string ParametersFrameServer = "ext.tabpage.parameters.frame-server";
    public const string ParametersAudio = "ext.tabpage.parameters.audio";
    public const string ParametersTrim = "ext.tabpage.parameters.trim";
    public const string ParametersFilterOrder = "ext.tabpage.parameters.filter-order";
    public const string ParametersCustom = "ext.tabpage.parameters.custom";
    public const string ParametersStreamControl = "ext.tabpage.parameters.stream-control";
    public const string ParametersAdditional = "ext.tabpage.parameters.additional";

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
