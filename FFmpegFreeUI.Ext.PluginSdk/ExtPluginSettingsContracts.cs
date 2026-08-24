using System.Windows.Forms;

namespace FFmpegFreeUI.Ext.PluginSdk;

/// <summary>在插件管理页面中注册当前插件自己的可选设置页。</summary>
public interface IExtPluginSettingsRegistry
{
    /// <summary>
    /// 注册插件设置页。每个 Ext 插件最多注册一个；未注册时插件详情中的设置入口保持禁用。
    /// </summary>
    /// <param name="extension">设置页工厂和可选清理逻辑。</param>
    /// <returns>注销句柄；释放后设置入口失效，已经创建的设置页也会被释放。</returns>
    IDisposable RegisterPage(ExtPluginSettingsPageExtension extension);
}

/// <summary>描述显示在插件管理页面中的插件设置页。</summary>
public sealed class ExtPluginSettingsPageExtension
{
    /// <summary>创建插件设置页定义。</summary>
    /// <param name="createPage">
    /// 在 UI 线程创建设置页的工厂。每次进入设置页时都可能调用，必须返回未被其他容器占用的新控件。
    /// </param>
    public ExtPluginSettingsPageExtension(
        Func<IExtPluginSettingsPageContext, Control> createPage)
    {
        CreatePage = createPage;
    }

    /// <summary>在 UI 线程创建未被其他容器占用的新设置页控件。</summary>
    public Func<IExtPluginSettingsPageContext, Control> CreatePage { get; set; }

    /// <summary>设置页被关闭或注册被注销时，在 UI 线程调用一次的可选清理逻辑。</summary>
    public Action<IExtPluginSettingsPageContext>? Cleanup { get; set; }
}

/// <summary>插件设置页实例的宿主上下文。</summary>
public interface IExtPluginSettingsPageContext
{
    /// <summary>当前 Ext 插件 ID。</summary>
    string PluginId { get; }

    /// <summary>插件工厂返回的设置页控件；调用 CreatePage 期间尚未赋值，此时为 null。</summary>
    Control? PageControl { get; }
}
