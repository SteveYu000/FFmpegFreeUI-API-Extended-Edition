using FFmpegFreeUI.Ext.PluginSdk;

namespace FFmpegFreeUI.Ext.PluginApi.Sample;

public sealed partial class SamplePlugin
{
    /// <summary>
    /// 使用宿主提供的完整预设快照生成总览行。这里不读取当前参数面板控件，所以参数面板总览、
    /// 预设管理器中选中的磁盘预设和其他宿主总览会得到一致结果。
    /// </summary>
    private void RegisterPresetOverview(IExtFFmpegFreeUIHost host)
    {
        _registrations.Add(host.PresetOverview.RegisterRowProvider(
            new ExtPluginPresetOverviewRowProvider(
                "sample-private-parameters",
                context =>
                {
                    var state = DeserializeState(context.PluginStateJson);
                    if (!IsActive(state))
                    {
                        return;
                    }

                    if (state.Enabled)
                    {
                        context.Rows.Add(new ExtPluginPresetOverviewRow(
                            $"示例自动质量策略：CRF {state.Crf}")
                        {
                            Order = 10
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(state.AdvancedArguments))
                    {
                        context.Rows.Add(new ExtPluginPresetOverviewRow(
                            $"示例进阶参数：{state.AdvancedArguments}")
                        {
                            Order = 20
                        });
                    }
                    if (state.AddDeclarativeMetadata)
                    {
                        context.Rows.Add(new ExtPluginPresetOverviewRow(
                            "示例声明式参数：写入 metadata")
                        {
                            Order = 30
                        });
                    }
                    if (state.RunDeclarativeCommandStep)
                    {
                        context.Rows.Add(new ExtPluginPresetOverviewRow(
                            "示例外部命令步骤：已启用")
                        {
                            Order = 40
                        });
                    }
                    if (state.ComputeSha256)
                    {
                        context.Rows.Add(new ExtPluginPresetOverviewRow(
                            "示例完成后处理：计算输出文件 SHA-256")
                        {
                            Order = 50
                        });
                    }
                })
            {
                Order = 100
            }));
    }
}
