using System.Reflection;
using FFmpegFreeUI;
using LakeUI;
using Bitrate = FFmpegFreeUI.比特率可视化数据核心;

internal static partial class Program
{
    private static async Task TestIdleQueueAddition()
    {
        var previous = 设置_v6.实例对象.自动开始任务选项;
        var tasks = new List<编码任务_v6>();
        try
        {
            设置_v6.实例对象.自动开始任务选项 = 0;
            var first = 编码队列_v6.添加命令行任务("--output-fixture", "automatic first", "");
            tasks.Add(first);
            await WaitUntil(() => first.状态 == 编码任务状态_v6.已完成 && !first.正在执行,
                "First automatic task did not finish");
            var second = 编码队列_v6.添加命令行任务("--output-fixture", "automatic after completed task", "");
            tasks.Add(second);
            await WaitUntil(() => second.状态 == 编码任务状态_v6.已完成 && !second.正在执行,
                "A retained completed task blocked the next automatic addition");
            Check(编码队列_v6.根据ID获取任务(first.ID) is not null && second.状态 == 编码任务状态_v6.已完成,
                "Idle queue additions start automatically even when completed tasks remain");
        }
        finally
        {
            设置_v6.实例对象.自动开始任务选项 = previous;
            编码队列_v6.停止任务(tasks.Select(task => task.ID));
            await WaitUntil(() => tasks.All(task => !task.正在执行), "Idle queue fixtures did not release their processes");
            编码队列_v6.移除任务(tasks.Select(task => task.ID));
        }
    }

    private static void TestNavigationCounters()
    {
        using var main = new FormMain_v6();
        Invoke(main, "绑定主页面选项卡");
        var navigation = AgentMember<ModernTabListControl>(main, "ModernTabListControl1");
        var queuePage = navigation.Items.Single(page => page.BoundControl is Form_v6_编码队列);
        var agentPage = navigation.Items.Single(page => page.BoundControl is Form_v6_Agent);
        var beforeQueue = new ModernTabListControl.ModernTabPage { Text = "plugin-before-queue" };
        var beforeAgent = new ModernTabListControl.ModernTabPage { Text = "plugin-before-agent" };
        navigation.Items.Insert(0, beforeQueue);
        navigation.Items.Insert(navigation.Items.IndexOf(agentPage), beforeAgent);
        var task = 编码队列_v6.添加命令行任务("--output-fixture", "navigation counter", "");
        try
        {
            Invoke(task, "开始执行"); // State fixture only; no process or real encoding is started.
            Invoke(main, "刷新任务数量选项卡");
            Check(queuePage.Text == "编码队列 (1)" && agentPage.Text == "Agent 智能体",
                "Navigation counters locate the actual bound pages after insertion");
            Check(beforeQueue.Text == "plugin-before-queue" && beforeAgent.Text == "plugin-before-agent",
                "Navigation counters must not overwrite plugin entry titles");
            task.停止();
            Invoke(task, "结束执行");
            Invoke(main, "刷新任务数量选项卡");
            Check(queuePage.Text == "编码队列", "Idle queue clears the count without changing its page binding");
        }
        finally
        {
            if (task.正在执行) { task.停止(); Invoke(task, "结束执行"); }
            编码队列_v6.移除任务([task.ID]);
        }
    }

    private static void TestBitrateVisualization()
    {
        var file = new Bitrate.文件信息
        {
            起始秒 = 10, 时长秒 = 3,
            流 = [new() { 索引 = 0, 类型 = "video", 起始秒 = 10, 时长秒 = 3 },
                  new() { 索引 = 1, 类型 = "audio", 起始秒 = 10, 时长秒 = 3 }]
        };
        Bitrate.包信息[] packets = [
            new() { 流索引 = 0, 时间秒 = 10, 时长秒 = 1, 字节数 = 1000, 关键帧 = true },
            new() { 流索引 = 1, 时间秒 = 10.25, 时长秒 = 0.5, 字节数 = 500 },
            new() { 流索引 = 0, 时间秒 = 11, 时长秒 = 1, 字节数 = 2000 },
            new() { 流索引 = 0, 时间秒 = 12, 时长秒 = 1, 字节数 = 500, 关键帧 = true }
        ];
        var selected = new HashSet<int> { 0, 1 };
        var seconds = Bitrate.构造序列(file, packets, selected, "每秒", "视频和音频", default);
        Check(seconds.起始秒 == 10 && seconds.时长秒 == 3 && seconds.帧数 == 3,
            "Bitrate statistics retain nonzero PTS origin without inflating playback duration");
        Check(seconds.点.Select(point => point.千比特).SequenceEqual(new[] { 12d, 16d, 4d }),
            "Per-second bitrate combines selected audio/video packet sizes");
        Check(Math.Abs(seconds.平均Kbps - 32d / 3) < 1e-9 && seconds.最低Kbps == 4 && seconds.最高Kbps == 16,
            "Average, minimum and maximum rates use actual playback time");
        var frames = Bitrate.构造序列(file, packets, selected, "每帧", "视频和音频", default);
        Check(!frames.包含音频 && frames.点.Select(point => point.千比特).SequenceEqual(new[] { 8d, 16d, 4d }),
            "Per-frame mode excludes audio and retains raw packet kilobits");
        var gops = Bitrate.构造序列(file, packets, selected, "每关键帧", "视频和音频", default);
        Check(gops.点.Count == 2 && gops.点[0].千比特 == 24 && gops.点[0].结束秒 == 12 && gops.点[1].千比特 == 4,
            "GOP aggregation retains its last group and does not move keyframes to the preceding group");
        var audio = Bitrate.构造序列(file, packets, selected, "每秒", "仅音频", default);
        Check(!audio.包含视频 && audio.帧数 == 1 && audio.点.Select(point => point.千比特).SequenceEqual(new[] { 4d, 0d, 0d }),
            "Audio-only mode counts audio packets and fills empty playback seconds");
        Check(Bitrate.构造序列(file, packets, [], "每秒", "视频和音频", default) is null,
            "Empty stream selection produces no analysis");
        var parser = typeof(Bitrate).GetMethod("解析包", BindingFlags.Static | BindingFlags.NonPublic)!;
        var packet = (Bitrate.包信息)parser.Invoke(null, ["stream_index=0|pts_time=N/A|dts_time=12.5|duration_time=0.5|size=125|flags=K_"])!;
        Check(packet.时间秒 == 12.5 && packet.关键帧 && packet.字节数 == 125,
            "Packet parsing falls back to DTS and retains size and keyframe flags");
        Check(parser.Invoke(null, ["stream_index=0|pts_time=NaN|size=125"]) is null,
            "Non-finite packet timestamps are rejected");
    }
}
