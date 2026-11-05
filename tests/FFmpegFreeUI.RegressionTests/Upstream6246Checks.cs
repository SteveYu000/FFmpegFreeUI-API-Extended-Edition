using System.Text.Json;
using FFmpegFreeUI;
using LakeUI;

internal static partial class Program
{
    private static void TestMp4FastStart()
    {
        Check(!JsonSerializer.Deserialize<预设数据_v6>("{}")!.输出_MP4索引前置,
            "Legacy presets keep faststart disabled by default");
        var preset = new 预设数据_v6
        {
            输出容器 = ".mp4", 输出_MP4索引前置 = true,
            视频参数_编码器_具体编码 = "libx264", 视频参数_比特率_基础 = "2500k"
        };
        string Command(预设数据_v6.命令行阶段 stage = 预设数据_v6.命令行阶段.普通单次) =>
            预设管理_v6.生成命令行(preset, "input.mkv", "output.mp4", stage).命令行;
        Check(Command().Contains("-movflags +faststart"), "MP4 output includes faststart");
        Check(!Command(预设数据_v6.命令行阶段.二次编码第一遍).Contains("+faststart") &&
            Command(预设数据_v6.命令行阶段.二次编码第二遍).Contains("+faststart"),
            "Two-pass encoding applies faststart only to the final output");
        preset.输出_输出文件参数使用方法 = 预设数据_v6.输出文件参数使用方法.声明丢弃输出;
        Check(!Command().Contains("+faststart"), "Discarded output does not request faststart");
        preset.输出_输出文件参数使用方法 = 预设数据_v6.输出文件参数使用方法.正常使用;
        foreach (var container in new[] { ".mkv", ".mov", "", ".MP4 " })
        {
            preset.输出容器 = container;
            Check(Command().Contains("+faststart") == (container == ".MP4 "),
                "Faststart follows the selected MP4 container with case/space normalization");
        }
        using var panel = new Form_v6_参数面板();
        panel.初始化默认参数();
        预设管理_v6.显示预设(preset, panel);
        var output = panel.私有界面_输出文件设置;
        var fastStart = (ModernCheckBox)output.Controls.Find("MCK_MP4索引前置", true).Single();
        Check(fastStart.Checked && fastStart.Enabled,
            "Preset restores the new MP4 checkbox");
        var captured = 预设管理_v6.从面板创建预设(panel);
        Check(JsonSerializer.Deserialize<预设数据_v6>(JsonSerializer.Serialize(captured))!.输出_MP4索引前置,
            "Panel capture and JSON roundtrip retain faststart");
        using var overview = new ModernTextBox();
        预设管理_v6.显示参数总览(overview, captured);
        Check(overview.Text.Contains("MP4 索引前置"), "Preset overview explains the enabled option");
        output.Controls.Find("MTB_后缀", true).Single().Text = ".mkv";
        Check(!fastStart.Enabled && !预设管理_v6.将预设数据转换为命令行(
            预设管理_v6.从面板创建预设(panel), "input.mkv", "output.mkv").Contains("+faststart"),
            "Changing to a non-MP4 container disables the control and omits the flag");
    }

    private static void TestOtherVmafCandidates()
    {
        var listed = Models();
        var candidates = (List<string>)StaticQuality("获取其他Vmaf候选模型",
            new[] { "vmaf_v0.6.1neg", "vmaf_float_v0.6.1", " VMAF_FLOAT_V0.6.1 ", "vmaf_b_v0.6.3", "" }, listed);
        Check(candidates.Count == 2 && !candidates.Contains("vmaf_v0.6.1neg"),
            "Other-model scan excludes listed models, blanks and case-insensitive duplicates");
        using var combo = new VmafModelComboBox();
        combo.ReplaceDisplayItems(listed);
        var commandIndex = combo.Models.ToList().FindIndex(item => item.IsCheckOtherModels) + 1;
        combo.SelectedIndex = commandIndex - 1;
        var previous = combo.SelectedModelValue;
        var requests = 0;
        combo.CheckOtherModelsRequested += (_, _) => requests++;
        combo.SelectedIndex = commandIndex;
        Check(requests == 1 && combo.SelectedModelValue == previous,
            "Manual scan restores the precise previous model instead of selecting the command");
        combo.AddDisplayItem(new() { ModelValue = "vmaf_v0.6.1", IsOtherModel = true, IsNonNeg = true }, false);
        Invoke(combo, "OnKeyDown", new KeyEventArgs(Keys.Down));
        Check(requests == 1 && combo.SelectedModelValue == "vmaf_v0.6.1",
            "Keyboard model navigation skips the scan command without launching a scan");
        Check(Auto(combo.Models.ToList(), 1920, 1080, 24, out _).ModelValue != "vmaf_v0.6.1",
            "AUTO ignores manually detected non-NEG/other models");
    }

    private static async Task TestSchedulerForwardAndWrap()
    {
        var previousLimit = 设置_v6.实例对象.自动同时运行任务数量选项;
        var tasks = new List<编码任务_v6>();
        try
        {
            设置_v6.实例对象.自动同时运行任务数量选项 = 0;
            tasks.Add(编码队列_v6.添加命令行任务("--wait-fixture", "forward head", ""));
            tasks.Add(编码队列_v6.添加命令行任务("--output-fixture", "forward middle", ""));
            tasks.Add(编码队列_v6.添加命令行任务("--wait-fixture", "forward tail", ""));
            tasks.Add(编码队列_v6.添加命令行任务("--output-fixture", "wrap trigger", ""));
            tasks[0].允许自动启动 = tasks[2].允许自动启动 = true;
            编码队列_v6.开始任务([tasks[1].ID]);
            await WaitUntil(() => tasks[2].当前进程ID > 0, "Natural completion did not scan forward");
            Check(tasks[0].状态 == 编码任务状态_v6.未处理 && tasks[1].状态 == 编码任务状态_v6.已完成,
                "With one free slot, natural completion prefers the pending tail over the head");
            编码队列_v6.停止任务([tasks[2].ID]);
            await WaitUntil(() => !tasks[2].正在执行, "Tail fixture did not stop");
            Check(tasks[0].状态 == 编码任务状态_v6.未处理, "Manual stop still does not advance the queue");
            编码队列_v6.开始任务([tasks[3].ID]);
            await WaitUntil(() => tasks[0].当前进程ID > 0, "Natural completion did not wrap to the head");
            Check(tasks[3].状态 == 编码任务状态_v6.已完成, "Scanning wraps after reaching the end of the queue");
        }
        finally
        {
            编码队列_v6.停止任务(tasks.Select(task => task.ID));
            await WaitUntil(() => tasks.All(task => !task.正在执行), "Forward/wrap fixtures did not stop");
            编码队列_v6.移除任务(tasks.Select(task => task.ID));
            设置_v6.实例对象.自动同时运行任务数量选项 = previousLimit;
        }
    }
}
