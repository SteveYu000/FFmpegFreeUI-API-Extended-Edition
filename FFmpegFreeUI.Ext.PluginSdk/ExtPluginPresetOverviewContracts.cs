namespace FFmpegFreeUI.Ext.PluginSdk;

/// <summary>
/// 注册基于完整预设快照的总览行提供器。宿主生成参数面板、预设管理或其他预设总览时，
/// 都会调用同一组提供器。
/// </summary>
public interface IExtPluginPresetOverviewRegistry
{
    IDisposable RegisterRowProvider(ExtPluginPresetOverviewRowProvider provider);
}

public delegate void ExtPluginPresetOverviewRowCallback(ExtPluginPresetOverviewContext context);

/// <summary>描述一个插件预设总览行提供器。</summary>
public sealed class ExtPluginPresetOverviewRowProvider
{
    public ExtPluginPresetOverviewRowProvider(
        string id,
        ExtPluginPresetOverviewRowCallback callback)
    {
        Id = id;
        Callback = callback;
    }

    public string Id { get; set; }

    /// <summary>
    /// 同一插件管理顺序中的提供器排序值。不同插件仍优先遵循插件管理器中的全局处理顺序。
    /// </summary>
    public int Order { get; set; }

    public ExtPluginPresetOverviewRowCallback Callback { get; set; }
}

/// <summary>预设总览提供器的只读输入和输出行集合。</summary>
public sealed class ExtPluginPresetOverviewContext
{
    public ExtPluginPresetOverviewContext(
        string pluginId,
        string presetJson,
        string pluginStateJson)
    {
        PluginId = pluginId ?? string.Empty;
        PresetJson = presetJson ?? string.Empty;
        PluginStateJson = string.IsNullOrWhiteSpace(pluginStateJson) ? "{}" : pluginStateJson;
    }

    /// <summary>当前提供器所属插件的 ID。</summary>
    public string PluginId { get; }

    /// <summary>
    /// 宿主正在生成总览的完整 v6 预设 JSON。它可能来自当前参数面板，也可能来自预设管理器中选中的预设。
    /// </summary>
    public string PresetJson { get; }

    /// <summary>宿主从 PresetJson 的 插件扩展数据[PluginId] 中提取并规范化的插件私有 JSON。</summary>
    public string PluginStateJson { get; }

    /// <summary>插件要追加到预设总览中的行。</summary>
    public IList<ExtPluginPresetOverviewRow> Rows { get; } = new List<ExtPluginPresetOverviewRow>();
}

/// <summary>插件提供的一行预设总览文本。</summary>
public sealed class ExtPluginPresetOverviewRow
{
    public ExtPluginPresetOverviewRow(string text)
    {
        Text = text;
    }

    public string Text { get; set; }

    /// <summary>同一提供器返回的多行之间的排序值。</summary>
    public int Order { get; set; }

    public ExtPluginPresetOverviewRowLevel Level { get; set; }
}

/// <summary>预设总览行的显示级别。</summary>
public enum ExtPluginPresetOverviewRowLevel
{
    Normal,
    Warning,
    Error
}
