using System.Drawing;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using FFmpegFreeUI;

internal static partial class Program
{
    private static object? WindowLayoutCall(string name, params object[] args) =>
        typeof(设置_v6).Assembly.GetType("FFmpegFreeUI.窗口布局_v6", throwOnError: true)!
            .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, args);

    private static void TestWindowLayout()
    {
        var original = 设置_v6.实例对象;
        try
        {
            var legacy = JsonSerializer.Deserialize<设置_v6>("{\"窗口样式\":2}")!;
            Check(legacy.窗口布局 == 0 && legacy.上次窗口宽度 == 0 && !legacy.上次窗口最大化,
                "Existing settings keep the default window layout");
            设置_v6.实例对象 = legacy;
            设置_v6.设置窗口布局(1);
            设置_v6.设置窗口布局(9);
            Check(legacy.窗口布局 == 1, "Invalid window layout values do not replace a valid preference");
            foreach (var dpi in new[] { 96, 120, 144, 192 })
            {
                var physical = new Size(1200 * dpi / 96, 700 * dpi / 96);
                WindowLayoutCall("保存正常窗口尺寸", physical, dpi, true);
                Check(legacy.上次窗口宽度 == 1200 && legacy.上次窗口高度 == 700 && legacy.上次窗口最大化,
                    $"Window size uses logical coordinates at DPI {dpi}");
                var restored = (Size)WindowLayoutCall("还原窗口大小", new Size(legacy.上次窗口宽度, legacy.上次窗口高度),
                    dpi, Size.Empty, new Size(7680, 4320))!;
                Check(restored == physical, $"Window dimensions round-trip at DPI {dpi}");
                foreach (var available in new[] { new Size(1280, 720), new Size(1920, 1080), new Size(800, 450) })
                {
                    var bounded = (Size)WindowLayoutCall("还原窗口大小", new Size(int.MaxValue, int.MaxValue),
                        dpi, new Size(1200, 700), available)!;
                    Check(bounded == available, $"Oversized saved dimensions fit the working area {available} at DPI {dpi}");
                }
            }
            var roundTrip = JsonSerializer.Deserialize<设置_v6>(JsonSerializer.Serialize(legacy))!;
            Check(roundTrip.窗口布局 == 1 && roundTrip.上次窗口宽度 == 1200 && roundTrip.上次窗口高度 == 700 && roundTrip.上次窗口最大化,
                "Saved window layout survives the settings JSON round-trip");
            设置_v6.设置窗口布局(0);
            WindowLayoutCall("保存正常窗口尺寸", new Size(800, 500), 96, false);
            Check(legacy.上次窗口宽度 == 1200 && legacy.上次窗口最大化,
                "Default-size mode preserves the previous remembered layout without updating it");
            using var top = new Form { Size = new Size(1200, 700) };
            using var page = new Form { TopLevel = false };
            using var container = new Panel();
            top.Controls.Add(page);
            page.Controls.Add(container);
            Check(ReferenceEquals(WindowLayoutCall("获取顶层窗口", container), top),
                "Embedded pages resolve to their actual top-level parent window");
            var before = top.Bounds;
            WindowLayoutCall("恢复主窗口", top);
            Check(top.Bounds == before, "Default-size mode leaves startup window dimensions untouched");
            legacy.窗口布局 = 1;
            legacy.上次窗口宽度 = 0;
            legacy.上次窗口高度 = 700;
            WindowLayoutCall("恢复主窗口", top);
            Check(top.Bounds == before, "Incomplete remembered dimensions fall back to the default layout");
            legacy.上次窗口宽度 = 1200;
            top.WindowState = FormWindowState.Maximized;
            WindowLayoutCall("恢复主窗口", top);
            Check(top.WindowState == FormWindowState.Maximized, "Explicit maximized/fullscreen startup takes precedence");
            top.WindowState = FormWindowState.Normal;
            using var child = new Form { MinimumSize = new Size(640, 360) };
            child.Load += (_, _) => child.Size = new Size(680, 400);
            typeof(设置_v6).Assembly.GetType("FFmpegFreeUI.Module1")!
                .GetMethod("显示窗体", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, [child, page]);
            Application.DoEvents();
            Check(child.Size == top.Size && ReferenceEquals(child.Owner, top),
                "Child size is inherited after its Load handlers and uses the actual top-level owner");
            child.Close();
        }
        finally
        {
            设置_v6.实例对象 = original;
        }
    }
}
