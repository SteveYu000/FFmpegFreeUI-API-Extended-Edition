using System.Text;
using System.Text.Json;
using FFmpegFreeUI;
using LakeUI;

internal static partial class Program
{
    private static void TestUpstream630(string directory)
    {
        var statePath = Path.Combine(directory, "agent-state", "nested", "state.json");
        Agent通用工具_v6.WriteJsonAtomically(statePath, new { Message = "中文状态", Revision = 1 });
        var bytes = File.ReadAllBytes(statePath);
        using var initial = JsonDocument.Parse(bytes);
        Check(!bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble) &&
              initial.RootElement.GetProperty("Message").GetString() == "中文状态",
            "Shared Agent persistence creates parent directories and preserves UTF-8 without BOM");
        Agent通用工具_v6.WriteJsonAtomically(statePath, new { Revision = 2 });
        using (var state = JsonDocument.Parse(File.ReadAllBytes(statePath)))
            Check(state.RootElement.GetProperty("Revision").GetInt32() == 2,
                "Atomic Agent persistence replaces an existing state file");
        Check(!Directory.EnumerateFiles(Path.GetDirectoryName(statePath)!, "*.tmp").Any(),
            "Successful atomic state replacement leaves no temporary files");
        var nullRejected = false;
        try { Agent通用工具_v6.WriteJsonAtomically(statePath, null!); }
        catch (ArgumentNullException) { nullRejected = true; }
        Check(nullRejected, "Null Agent state is rejected before replacing existing data");

        var previous = 设置_v6.实例对象.质量评测页面状态;
        using var page = new Form_v6_集成工具_质量评测();
        System.Windows.Forms.Timer? timer = null;
        try
        {
            设置_v6.实例对象.质量评测页面状态 = JsonSerializer.Serialize(new
            {
                指标 = (object?)null, Pooling = "mean", SubSample = "1",
                文件 = new object?[]
                {
                    null,
                    new { 文件路径 = Environment.ProcessPath, 指标结果 = (object?)null },
                    new { 文件路径 = typeof(设置_v6).Assembly.Location,
                        指标结果 = new Dictionary<string, object?>
                        {
                            ["PSNR"] = null,
                            ["VMAF"] = new { 成功 = true, 汇总值 = 88d,
                                每帧数据 = (double[]?)null, 每帧帧号 = (int[]?)null }
                        }
                    }
                }
            });
            Invoke(page, "初始化控件");
            Invoke(page, "恢复页面状态");
            var list = (UltraDetailListView)page.Controls.Find("UltraDetailListView1", true).Single();
            Check(list.Items.Count == 2,
                "Legacy quality state restores later files after null entries or metric dictionaries");
            Invoke(page, "保存页面状态");
            using (var saved = JsonDocument.Parse(设置_v6.实例对象.质量评测页面状态))
            {
                var result = saved.RootElement.GetProperty("文件")[1].GetProperty("指标结果").GetProperty("VMAF");
                Check(result.GetProperty("汇总值").GetDouble() == 88 &&
                      result.GetProperty("每帧数据").GetArrayLength() == 0 &&
                      result.GetProperty("每帧帧号").GetArrayLength() == 0,
                    "Quality state keeps valid scores and normalizes missing frame arrays");
            }
            var beforeEdit = 设置_v6.实例对象.质量评测页面状态;
            var reference = page.Controls.Find("MTB_原视频文件路径", true).Single();
            reference.Text = Path.Combine(directory, "reference.mkv");
            timer = (System.Windows.Forms.Timer)page.GetType()
                .GetField("页面状态保存计时器", PrivateInstance)!.GetValue(page)!;
            Check(timer.Enabled && 设置_v6.实例对象.质量评测页面状态 == beforeEdit,
                "Editing the reference path schedules persistence without writing on each keystroke");
            Invoke(page, "页面宿主窗体_FormClosing", page,
                new FormClosingEventArgs(CloseReason.ApplicationExitCall, false));
            using var flushed = JsonDocument.Parse(设置_v6.实例对象.质量评测页面状态);
            Check(!timer.Enabled && flushed.RootElement.GetProperty("原视频").GetString() == reference.Text,
                "Closing the embedded page host flushes the latest path before the debounce expires");
            Invoke(page, "保存待处理页面状态");
            Check(设置_v6.实例对象.质量评测页面状态 == flushed.RootElement.GetRawText(),
                "A second close flush is harmless when no save is pending");
        }
        finally
        {
            timer?.Stop();
            timer?.Dispose();
            设置_v6.实例对象.质量评测页面状态 = previous;
        }
    }
}
