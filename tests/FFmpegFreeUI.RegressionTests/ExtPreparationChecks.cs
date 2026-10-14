using System.Reflection;
using FFmpegFreeUI;

internal static partial class Program
{
    private static async Task TestExtPreparation(string directory)
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var bridge = typeof(编码任务_v6).Assembly.GetType("FFmpegFreeUI.Ext插件扩展桥接_v2")!;
        var hostField = bridge.GetField("宿主类型", flags)!;
        var initializedField = bridge.GetField("初始化完成", flags)!;
        var methodCache = (Dictionary<string, MethodInfo>)bridge.GetField("方法缓存", flags)!.GetValue(null)!;
        var previousHost = hostField.GetValue(null);
        var previousInitialized = initializedField.GetValue(null);
        var previousMethods = methodCache.ToArray();
        var tasks = new List<编码任务_v6>();
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            // Exercise the neutral bridge transport, without installing an SDK or
            // claiming this fixture is an old third-party DLL loading test.
            hostField.SetValue(null, typeof(PreparationHostFixture));
            initializedField.SetValue(null, true);
            methodCache.Clear();
            var preset = new 预设数据_v6
            {
                输出容器 = "mkv",
                输出_自动命名选项 = 预设数据_v6.自动命名选项.附加_递增时间戳,
                自定义参数_完全自己写 = "--output-fixture"
            };
            var input = Path.Combine(directory, "reservation.mp4");
            tasks.AddRange(编码队列_v6.批量添加预设任务(new[] { input, input }, preset));
            var entered = new HashSet<string>();
            PreparationHostFixture.BeforePrepare = async (context, token) =>
            {
                lock (entered) entered.Add(context.TaskId);
                await (context.TaskId == tasks[0].ID ? releaseFirst.Task : releaseSecond.Task).WaitAsync(token);
            };
            编码队列_v6.开始任务(tasks.Select(task => task.ID));
            var firstOutput = tasks[0].输出文件;
            var secondOutput = tasks[1].输出文件;
            Check(!string.IsNullOrWhiteSpace(firstOutput) && !string.IsNullOrWhiteSpace(secondOutput),
                "Outputs are reserved synchronously before async preparation");
            Check(firstOutput != secondOutput, "Same-input tasks have distinct reserved outputs");
            await WaitUntil(() => { lock (entered) return entered.Count == 2; }, "Preparation gates did not start");
            releaseSecond.SetResult();
            await WaitUntil(() => !tasks[1].正在执行, "Second gated task did not finish");
            releaseFirst.SetResult();
            await WaitUntil(() => !tasks[0].正在执行, "First gated task did not finish");
            Check(tasks.All(task => task.状态 == 编码任务状态_v6.已完成), "Gated preparation tasks complete successfully");
            Check(tasks[0].输出文件 == firstOutput && tasks[1].输出文件 == secondOutput,
                "Async preparation completion order must not renumber reserved outputs");
            编码队列_v6.移除任务(tasks.Select(task => task.ID));
            tasks.Clear();

            var repairedInput = Path.Combine(directory, "repaired.mp4");
            var inputRoot = Path.Combine(directory, "preparation-input");
            var outputRoot = Path.Combine(directory, "preparation-output");
            Directory.CreateDirectory(Path.Combine(inputRoot, "blocked"));
            Directory.CreateDirectory(outputRoot);
            File.WriteAllText(Path.Combine(outputRoot, "blocked"), "disposable fixture");
            var repairing = 编码队列_v6.添加预设任务(Path.Combine(inputRoot, "blocked", "sample.mp4"), new 预设数据_v6
            {
                输出容器 = "mkv", 输出位置 = outputRoot,
                计算机名称 = Environment.MachineName, 额外保存输出位置 = true,
                输出位置_保留子文件夹结构起始点 = inputRoot
            }, "repair fixture");
            tasks.Add(repairing);
            var sawInitialFailure = false;
            PreparationHostFixture.BeforePrepare = (context, _) =>
            {
                sawInitialFailure = typeof(编码任务_v6).GetField("启动准备异常", PrivateInstance)!.GetValue(repairing) is Exception;
                context.InputPath = repairedInput;
                context.PresetJson = System.Text.Json.JsonSerializer.Serialize(preset);
                return Task.CompletedTask;
            };
            编码队列_v6.开始任务(new[] { repairing.ID });
            await WaitUntil(() => !repairing.正在执行, "Repaired output preparation did not finish");
            Check(sawInitialFailure, "Fixture reproduces a pre-hook output reservation failure");
            Check(repairing.状态 == 编码任务状态_v6.已完成, "Before-prepare may repair an initial output reservation error: " + repairing.获取日志文本());
            Check(repairing.输入文件 == repairedInput && repairing.输出文件.Contains("repaired", StringComparison.Ordinal),
                "Repaired inputs are reflected in automatic output naming");
        }
        finally
        {
            releaseFirst.TrySetResult();
            releaseSecond.TrySetResult();
            foreach (var task in tasks.Where(task => task.正在执行)) task.停止();
            await WaitUntil(() => tasks.All(task => !task.正在执行), "Preparation fixture cleanup did not finish");
            编码队列_v6.移除任务(tasks.Select(task => task.ID));
            PreparationHostFixture.BeforePrepare = null;
            hostField.SetValue(null, previousHost);
            initializedField.SetValue(null, previousInitialized);
            methodCache.Clear();
            foreach (var entry in previousMethods) methodCache.Add(entry.Key, entry.Value);
        }
    }

    private static class PreparationHostFixture
    {
        public static Func<Ext插件管线上下文_v2, CancellationToken, Task>? BeforePrepare;
        public static void 执行同步阶段(string stageId, Ext插件管线上下文_v2 context) { }
        public static Task 执行异步阶段Async(string stageId, Ext插件管线上下文_v2 context, CancellationToken token) =>
            stageId == "ext.task.before-prepare" && BeforePrepare is not null ? BeforePrepare(context, token) : Task.CompletedTask;
        public static void 执行行为点(string behaviorId, Ext插件行为上下文_v2 context, Action<Ext插件行为上下文_v2> nativeAction) =>
            nativeAction(context);
    }
}
