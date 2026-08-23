using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using FFmpegFreeUI.Ext.PluginSdk;

namespace FFmpegFreeUI.Ext.PluginApi.Sample;

public sealed partial class SamplePlugin
{
    private void RegisterPageEntries(IExtFFmpegFreeUIHost host)
    {
        if (host.PageEntries.AvailableTargets.Any(target =>
                target.TargetId.Equals(
                    ExtFFmpegFreeUIPageTargets.MainEncodingQueue,
                    StringComparison.OrdinalIgnoreCase)))
        {
            _registrations.Add(host.PageEntries.RegisterPage(new ExtPluginPageExtension(
                "main-api-status",
                ExtFFmpegFreeUIPageTargets.MainEncodingQueue,
                ExtPluginRelativePosition.After,
                "Ext API 示例",
                CreateMainStatusPage)
            {
                Order = 100
            }));
        }

        if (host.PageEntries.AvailableTargets.Any(target =>
                target.TargetId.Equals(
                    ExtFFmpegFreeUIPageTargets.ParametersAudio,
                    StringComparison.OrdinalIgnoreCase)))
        {
            _registrations.Add(host.PageEntries.RegisterPage(new ExtPluginPageExtension(
                "parameter-quality-policy",
                ExtFFmpegFreeUIPageTargets.ParametersAudio,
                ExtPluginRelativePosition.After,
                "示例质量策略",
                CreateParameterStatusPage)
            {
                Order = 100
            }));
        }
    }

    private void RegisterEncodingQueueToolbarControls(IExtFFmpegFreeUIHost host)
    {
        if (host.EncodingQueueToolbar.AvailableTargets.Any(target =>
                target.TargetId.Equals(
                    ExtFFmpegFreeUIToolbarTargets.EncodingQueueLocate,
                    StringComparison.OrdinalIgnoreCase)))
        {
            _registrations.Add(host.EncodingQueueToolbar.RegisterControl(
                new ExtPluginToolbarControlExtension(
                    "queue-api-status",
                    ExtFFmpegFreeUIToolbarTargets.EncodingQueueLocate,
                    ExtPluginRelativePosition.After,
                    CreateQueueStatusButton)
                {
                    Order = 100
                }));
        }
    }

    private Control CreateMainStatusPage(IExtPluginPageContext context)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(28)
        };
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Microsoft YaHei UI", 12F),
            ForeColor = Color.Gainsboro,
            Text = $"{DisplayName}\r\n\r\n页面目标：{context.TargetId}\r\n位置：{context.Position}"
        });
        return panel;
    }

    private Control CreateParameterStatusPage(IExtPluginPageContext context)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(28)
        };
        var enabled = new CheckBox
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Microsoft YaHei UI", 11F),
            ForeColor = Color.Gainsboro,
            Text = "启用示例自动质量策略"
        };
        var restoring = false;

        void RestoreState()
        {
            restoring = true;
            try
            {
                enabled.Checked = DeserializeState(context.StateJson).Enabled;
            }
            finally
            {
                restoring = false;
            }
        }

        enabled.CheckedChanged += (_, _) =>
        {
            if (restoring)
            {
                return;
            }

            var state = DeserializeState(context.StateJson);
            state.Enabled = enabled.Checked;
            context.StateJson = JsonSerializer.Serialize(state);
            context.RequestParameterRefresh();
        };
        EventHandler restored = (_, _) => RestoreState();
        context.StateRestored += restored;
        panel.Disposed += (_, _) => context.StateRestored -= restored;
        RestoreState();
        panel.Controls.Add(enabled);
        return panel;
    }

    private Control CreateQueueStatusButton(IExtPluginToolbarContext context)
    {
        var button = new Button
        {
            AutoSize = false,
            Width = 94,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.CornflowerBlue,
            Text = "Ext API 状态",
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => Log(
            ExtPluginLogLevel.Information,
            $"工具栏控件已触发；目标={context.TargetId}，DPI={context.DeviceDpi}");
        return button;
    }
}
