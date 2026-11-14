using System.Diagnostics;
using FFmpegFreeUI;

internal static partial class Program
{
    private static void TestRecoveredProcessIdentity()
    {
        using var current = Process.GetCurrentProcess();
        var started = current.StartTime.ToUniversalTime();
        var matching = new 编码任务_v6
        {
            状态 = 编码任务状态_v6.正在处理,
            恢复进程ID = current.Id,
            进程启动时间Utc = started
        };
        Check((bool)Invoke(matching, "检查恢复进程")! && matching.恢复进程仍在运行,
            "Recovered process must match both the PID and saved start time");
        var called = false;
        Func<Process, int> control = _ => { called = true; return 0; };
        Check((bool)Invoke(matching, "执行恢复进程控制", control)! && called,
            "A matching recovered process can use the existing control path");
        matching.进程启动时间Utc = started.AddSeconds(-1);
        called = false;
        Check(!(bool)Invoke(matching, "执行恢复进程控制", control)! && !called,
            "A reused PID must never be passed to a recovered process control callback");
        Check(!(bool)Invoke(matching, "检查恢复进程")! && matching.恢复进程ID == 0 &&
              !matching.恢复进程仍在运行 && matching.状态 == 编码任务状态_v6.已停止,
            "A stale process identity is marked stopped and detached");
        var unknown = new 编码任务_v6 { 恢复进程ID = current.Id };
        Check(!(bool)Invoke(unknown, "检查恢复进程")!,
            "A snapshot without a process start time must not attach by PID alone");
    }
}
