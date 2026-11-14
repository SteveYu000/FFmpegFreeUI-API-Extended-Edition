using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using FFmpegFreeUI;

internal static partial class Program
{
    // Each process has its own base directory and assembly resolver state. Loaded SDKs
    // from another case must not make a missing-dependency test pass accidentally.
    private static async Task TestPluginLoading(string directory)
    {
        var fixtures = Path.Combine(AppContext.BaseDirectory, "PluginLoadFixture");
        foreach (var mode in new[] { "none", "sdk-only", "host-only", "complete", "required-sdk-missing", "ext-initialize-fails" })
        {
            var isolated = Path.Combine(directory, "plugin-loader-" + mode);
            Directory.CreateDirectory(isolated);
            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory))
            {
                var name = Path.GetFileName(file);
                if (name is "FFmpegFreeUI.Ext.PluginSdk.dll" or "FFmpegFreeUI.Ext.PluginHost.dll") continue;
                if (Path.GetExtension(file) is ".dll" or ".exe" ||
                    name.EndsWith(".deps.json", StringComparison.Ordinal) || name.EndsWith(".runtimeconfig.json", StringComparison.Ordinal))
                    File.Copy(file, Path.Combine(isolated, name));
            }
            if (mode is "sdk-only" or "complete" or "ext-initialize-fails")
                File.Copy(Path.Combine(fixtures, "FFmpegFreeUI.Ext.PluginSdk.dll"), Path.Combine(isolated, "FFmpegFreeUI.Ext.PluginSdk.dll"));
            if (mode is "host-only" or "complete" or "ext-initialize-fails")
                File.Copy(Path.Combine(fixtures, "FFmpegFreeUI.Ext.PluginHost.dll"), Path.Combine(isolated, "FFmpegFreeUI.Ext.PluginHost.dll"));
            var plugin = Path.Combine(isolated, "Plugin", "LoaderFixture.3fui.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(plugin)!);
            File.Copy(Path.Combine(fixtures, "FFmpegFreeUI.PluginLoadFixture.dll"), plugin);
            var start = new ProcessStartInfo(Path.Combine(isolated, "FFmpegFreeUI.RegressionTests.exe"))
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = isolated,
                ArgumentList = { "--plugin-load-fixture", plugin }
            };
            start.Environment["FFUI_LOADER_REQUIRE_SDK"] = mode == "required-sdk-missing" ? "1" : "0";
            start.Environment["FFUI_LOADER_FAIL_EXT"] = mode == "ext-initialize-fails" ? "1" : "0";
            start.Environment["FFUI_LOADER_SHOW_PAGE"] = "0";
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var errors = process.StandardError.ReadToEndAsync();
            try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30)); }
            catch { if (!process.HasExited) process.Kill(entireProcessTree: true); throw; }
            var result = await output;
            Check(process.ExitCode == 0, "Isolated plugin loader fixture failed: " + await errors + result);
            Console.WriteLine("PASS: plugin loader " + mode + ": " + result.Trim());
            using var json = JsonDocument.Parse(result);
            var data = json.RootElement;
            Check(data.GetProperty("PluginType").GetString() == "官方与Ext", "Fixture must retain a real SDK reference and official Entry");
            var events = data.GetProperty("Events").EnumerateArray().Select(x => x.GetString()!).ToArray();
            Check(events.Count(x => x.StartsWith("callback:", StringComparison.Ordinal)) == 6, "All official callbacks are injected: " + mode);
            if (mode == "required-sdk-missing")
            {
                Check(!data.GetProperty("Loaded").GetBoolean(), "A genuinely required missing dependency must not be reported as loaded");
                Check(data.GetProperty("ErrorType").GetString() == nameof(FileNotFoundException), "Report the real dependency error: " + result);
                Check(events.Contains("entry:official"), "The official entry must actually run before its dependency failure");
            }
            else if (mode == "ext-initialize-fails")
            {
                Check(!data.GetProperty("Loaded").GetBoolean(), "Ext initialization errors must remain errors");
                Check(!events.Any(x => x.StartsWith("entry:", StringComparison.Ordinal)), "Do not silently fall back after a failed Ext initialization");
            }
            else
            {
                Check(data.GetProperty("Loaded").GetBoolean(), "Plugin must load in " + mode + ": " + result);
                Check(data.GetProperty("Status").GetString() == "已加载", "Manager status must reflect successful loading");
                Check(events.Last() == (mode == "complete" ? "entry:ext" : "entry:official"), "Correct entry mode and initialization order: " + mode);
                Check(events.Any(x => x.StartsWith("ext.initialize:", StringComparison.Ordinal)) == (mode == "complete"), "Ext initialization runs only when available");
                if (mode is "none" or "host-only")
                    Check(!data.GetProperty("SdkLoaded").GetBoolean(), "Official fallback must not resolve unrelated SDK types");
            }
        }
    }

    private static int RunPluginLoadFixture(string pluginPath)
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var manager = typeof(插件管理);
        var info = (插件信息_v6)manager.GetMethod("读取插件元数据", flags)!.Invoke(null, [pluginPath])!;
        object? result = null;
        Exception? error = null;
        try { result = manager.GetMethod("加载单个插件", flags)!.Invoke(null, [info]); }
        catch (Exception caught)
        {
            error = caught;
            while (error is TargetInvocationException { InnerException: { } inner }) error = inner;
        }
        var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.GetName().Name == "FFmpegFreeUI.PluginLoadFixture");
        var events = assembly?.GetType("FFmpegFreeUI.PluginLoadFixture.Entry")?.GetField("Events")?.GetValue(null) as List<string>;
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            PluginType = info.接口类型.ToString(),
            Loaded = result?.GetType().GetProperty("已加载")?.GetValue(result) as bool? ?? false,
            Status = result?.GetType().GetProperty("状态")?.GetValue(result) as string,
            Events = events?.ToArray() ?? [], ErrorType = error?.GetType().Name, Error = error?.Message,
            SdkLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "FFmpegFreeUI.Ext.PluginSdk")
        }));
        return 0;
    }
}
