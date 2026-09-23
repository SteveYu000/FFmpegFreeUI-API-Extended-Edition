using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static partial class Program
{
    private static readonly string[] UpdateFileNames =
    [
        "FFmpegFreeUI.exe",
        "FFmpegFreeUI.Ext.PluginHost.dll",
        "FFmpegFreeUI.Ext.PluginSdk.dll"
    ];

    private static async Task TestAutomaticUpdate(string directory)
    {
        var updater = typeof(FFmpegFreeUI.版本号).Assembly.GetType("FFmpegFreeUI.网络功能_v6_自动更新")!;
        var fileType = updater.GetNestedType("更新文件", BindingFlags.NonPublic)!;
        var readyType = updater.GetNestedType("待安装更新", BindingFlags.NonPublic)!;
        var parse = updater.GetMethod("解析发行文件", BindingFlags.Static | BindingFlags.NonPublic)!;
        var assetData = UpdateFileNames.Select(name => new
        {
            name,
            size = 10,
            digest = "sha256:" + new string('a', 64),
            browser_download_url = $"https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition/releases/download/v9/{name}"
        }).ToArray();
        var releaseJson = JsonSerializer.Serialize(new { tag_name = "v9", assets = assetData });
        var parsed = (IEnumerable)parse.Invoke(null, [releaseJson, "v9"])!;
        Check(parsed.Cast<object>().Count() == 3, "Updater must require all three release files");
        Check(ParseFails(parse, releaseJson.Replace("sha256:", "md5:")), "Updater must reject releases without SHA-256");
        Check(ParseFails(parse, releaseJson.Replace("github.com", "example.com")), "Updater must reject non-GitHub asset URLs");
        Check(ParseFails(parse, releaseJson.Replace("v9\"", "v8\"")), "Updater must reject a different release tag");

        await RunUpdateFixture(directory, updater, fileType, readyType, failDuringReplace: false);
        await RunUpdateFixture(directory, updater, fileType, readyType, failDuringReplace: true);
    }

    private static bool ParseFails(MethodInfo parse, string json)
    {
        try { parse.Invoke(null, [json, "v9"]); return false; }
        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { return true; }
    }

    private static async Task RunUpdateFixture(string directory, Type updater, Type fileType, Type readyType, bool failDuringReplace)
    {
        var target = Path.Combine(directory, failDuringReplace ? "update-rollback" : "update-success");
        var stage = Path.Combine(target, ".update-staging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        var files = Array.CreateInstance(fileType, UpdateFileNames.Length);
        for (var i = 0; i < UpdateFileNames.Length; i++)
        {
            var name = UpdateFileNames[i];
            File.WriteAllText(Path.Combine(target, name), "old-" + name);
            var data = Encoding.UTF8.GetBytes("new-" + name);
            File.WriteAllBytes(Path.Combine(stage, name), data);
            var hash = Convert.ToHexString(SHA256.HashData(data));
            var asset = Activator.CreateInstance(fileType,
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                [name, new Uri("https://github.com/SteveYu000/FFmpegFreeUI-API-Extended-Edition/releases/download/v9/" + name), (long)data.Length, hash],
                null)!;
            files.SetValue(asset, i);
        }
        if (failDuringReplace)
        {
            var blocked = Path.Combine(target, UpdateFileNames[1]);
            File.Delete(blocked);
            Directory.CreateDirectory(blocked);
        }
        var ready = Activator.CreateInstance(readyType,
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            ["v9", stage, files], null)!;
        var script = (string)updater.GetMethod("创建安装脚本", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [ready, target, int.MaxValue, false])!;
        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
        using var process = Process.Start(new ProcessStartInfo(powershell)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            ArgumentList =
            {
                "-NoLogo", "-NoProfile", "-NonInteractive", "-EncodedCommand",
                Convert.ToBase64String(Encoding.Unicode.GetBytes(script))
            }
        })!;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await process.WaitForExitAsync(timeout.Token);
        var stderr = await process.StandardError.ReadToEndAsync(timeout.Token);
        var statusPath = Path.Combine(target, ".update-result.json");
        Check(File.Exists(statusPath), "Update helper must record its result: " + stderr);
        using var status = JsonDocument.Parse(File.ReadAllText(statusPath));
        var state = status.RootElement.GetProperty("status").GetString();
        Check(state == (failDuringReplace ? "failed" : "success"),
            "Update helper reported the wrong result: " + status.RootElement.GetProperty("detail").GetString() + " " + stderr);
        Check(File.ReadAllText(Path.Combine(target, UpdateFileNames[0])) ==
              (failDuringReplace ? "old-" : "new-") + UpdateFileNames[0],
              "Update helper must replace or roll back the executable");
        if (!failDuringReplace)
        {
            Check(UpdateFileNames.All(name => File.ReadAllText(Path.Combine(target, name)) == "new-" + name),
                "Update helper must install the complete release");
        }
    }
}
