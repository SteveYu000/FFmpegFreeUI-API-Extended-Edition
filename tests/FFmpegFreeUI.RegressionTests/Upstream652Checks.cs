using System.Collections.Concurrent;
using System.Text.Json;
using FFmpegFreeUI;

internal static partial class Program
{
    private static async Task TestUtilityTaskScheduling()
    {
        var settings = 设置_v6.实例对象;
        var previousAutoStart = settings.自动开始任务选项;
        var previousParallelism = settings.自动同时运行任务数量选项;
        var starts = new ConcurrentDictionary<string, int>();
        var startNumber = 0;
        var tasks = new List<编码任务_v6>();

        void ObserveStart(编码任务_v6 task)
        {
            if (task.状态 == 编码任务状态_v6.正在处理)
                starts.TryAdd(task.ID, Interlocked.Increment(ref startNumber));
        }

        编码队列_v6.任务已更新 += ObserveStart;
        try
        {
            settings.自动同时运行任务数量选项 = 0;
            settings.自动开始任务选项 = 1;
            var older = 编码队列_v6.添加命令行任务("--output-fixture", "Older queued task", "");
            tasks.Add(older);

            settings.自动开始任务选项 = 0;
            var utility = 编码队列_v6.添加命令行任务("--output-fixture", "Manual utility task", "", "", false);
            tasks.Add(utility);
            编码队列_v6.应用自动开始任务设置(true);
            Check(older.允许自动启动 && !utility.允许自动启动 && utility.状态 == 编码任务状态_v6.未处理,
                "Tool tasks stay manual when automatic start is enabled");

            编码队列_v6.立即保存队列();
            using (var snapshot = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "QueueTasks.json"))))
            {
                var saved = snapshot.RootElement.GetProperty("任务").EnumerateArray()
                    .Single(task => task.GetProperty("ID").GetString() == utility.ID);
                Check(!saved.GetProperty("允许自动启动").GetBoolean() &&
                      !saved.GetProperty("完成后自动调度队列").GetBoolean(),
                    "The queue snapshot preserves the utility task's manual scheduling policy");
            }

            编码队列_v6.开始任务([utility.ID]);
            await WaitUntil(() => utility.状态 == 编码任务状态_v6.已完成 && !utility.正在执行,
                "Manually starting a utility task did not finish");
            await Task.Delay(300);
            Check(!starts.ContainsKey(older.ID), "Utility completion must leave the older queue waiting");

            var added = 编码队列_v6.添加命令行任务("--output-fixture", "New queued task", "", "");
            tasks.Add(added);
            await WaitUntil(() => added.状态 == 编码任务状态_v6.已完成 && !added.正在执行 &&
                                older.状态 == 编码任务状态_v6.已完成 && !older.正在执行,
                "The newly added task did not start and advance the queue");
            Check(starts[added.ID] < starts[older.ID],
                "An idle queue starts from the newly added task before wrapping to older tasks");
        }
        finally
        {
            编码队列_v6.任务已更新 -= ObserveStart;
            编码队列_v6.停止任务(tasks.Select(task => task.ID));
            await WaitUntil(() => tasks.All(task => !task.正在执行), "Utility scheduling test left a running task");
            编码队列_v6.移除任务(tasks.Select(task => task.ID));
            编码队列_v6.立即保存队列();
            settings.自动开始任务选项 = previousAutoStart;
            settings.自动同时运行任务数量选项 = previousParallelism;
        }
    }
}
