using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace FFmpegFreeUI.Ext.PluginSdk;

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
