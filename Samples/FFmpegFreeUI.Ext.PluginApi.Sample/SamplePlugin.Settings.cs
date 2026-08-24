using System.Drawing;
using System.Windows.Forms;
using FFmpegFreeUI.Ext.PluginSdk;

namespace FFmpegFreeUI.Ext.PluginApi.Sample;

public sealed partial class SamplePlugin
{
    private bool _detailedSettingsLog;

    private void RegisterPluginSettings(IExtFFmpegFreeUIHost host)
    {
        _registrations.Add(host.PluginSettings.RegisterPage(
            new ExtPluginSettingsPageExtension(CreatePluginSettingsPage)));
    }

    private Control CreatePluginSettingsPage(IExtPluginSettingsPageContext context)
    {
        var page = new Panel
        {
            AutoScroll = true,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Padding = new Padding(24)
        };
        var content = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            Padding = Padding.Empty,
            WrapContents = false
        };
        var title = new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 13F),
            ForeColor = Color.Gainsboro,
            Margin = new Padding(0, 0, 0, 12),
            Text = "C# Ext API 示例设置"
        };
        var description = new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = Color.Silver,
            Margin = new Padding(0, 0, 0, 18),
            MaximumSize = new Size(720, 0),
            Text = "这个页面通过 host.PluginSettings 注册，只显示在插件管理器中，不占用主导航入口。"
        };
        var detailedLog = new CheckBox
        {
            AutoSize = true,
            Checked = _detailedSettingsLog,
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = Color.Silver,
            Margin = new Padding(0, 0, 0, 14),
            Text = "启用详细示例日志（仅当前运行）"
        };
        detailedLog.CheckedChanged += (_, _) => _detailedSettingsLog = detailedLog.Checked;

        var writeLog = new Button
        {
            AutoSize = false,
            BackColor = Color.FromArgb(45, 220, 220, 220),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = Color.CornflowerBlue,
            Margin = Padding.Empty,
            Size = new Size(130, 34),
            Text = "写入测试日志",
            UseVisualStyleBackColor = false
        };
        writeLog.FlatAppearance.BorderSize = 0;
        writeLog.Click += (_, _) => _host?.Log(
            ExtPluginLogLevel.Information,
            $"插件设置页测试；PluginId={context.PluginId}，详细日志={_detailedSettingsLog}");

        content.Controls.Add(title);
        content.Controls.Add(description);
        content.Controls.Add(detailedLog);
        content.Controls.Add(writeLog);
        page.Controls.Add(content);
        return page;
    }
}
