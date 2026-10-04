using System.Drawing;
using System.Reflection;
using FFmpegFreeUI;

internal static partial class Program
{
    private static void TestCropPreviewParsing()
    {
        var selector = typeof(Form_v6_参数面板_画面区域选择窗口);
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        var timestampParser = selector.GetMethod("解析预览时间戳", flags)!;
        var ratioParser = selector.GetMethod("尝试解析比例文本", flags)!;
        var cropParser = selector.GetMethod("尝试解析裁剪参数", flags)!;

        foreach (var (text, expected) in new[]
        {
            ("", "10"), (" （默认第 10 秒） ", "10"), ("第一帧", "0"),
            ("1 分钟", "60"), ("2 分钟", "120"), ("3 分钟", "180"),
            ("5 分钟", "300"), ("10 分钟", "600"),
            (" 1.25 ", "1.25"), ("00:00:03.500", "00:00:03.500")
        })
        {
            Check((string)timestampParser.Invoke(null, [text])! == expected,
                $"Crop preview timestamp did not preserve the current text: {text}");
        }

        foreach (var (text, expected) in new[]
        {
            ("", 0f), ("自由", 0f), ("16:9", 16f / 9), ("16/9", 16f / 9),
            ("4X3", 4f / 3), ("16：9", 16f / 9), ("1.777", 1.777f)
        })
        {
            object?[] arguments = [text, 0f];
            Check((bool)ratioParser.Invoke(null, arguments)!, $"Valid crop ratio rejected: {text}");
            Check(Math.Abs((float)arguments[1]! - expected) < 0.0001f,
                $"Crop ratio parsed incorrectly: {text}");
        }

        foreach (var text in new[] { "NaN", "Infinity", "0", "-1", "0:1", "1:0", "1e100", "1e-100" })
        {
            object?[] arguments = [text, 0f];
            Check(!(bool)ratioParser.Invoke(null, arguments)!, $"Invalid crop ratio accepted: {text}");
        }

        object?[] cropArguments = ["640:360:20:30", Rectangle.Empty];
        Check((bool)cropParser.Invoke(null, cropArguments)!, "Valid crop coordinates rejected");
        Check((Rectangle)cropArguments[1]! == new Rectangle(20, 30, 640, 360),
            "Crop coordinates parsed incorrectly");
        foreach (var text in new[] { "", "1:2:3", "0:360:0:0", "640:-1:0:0", "640:360:-1:0", "2147483648:360:0:0" })
        {
            object?[] arguments = [text, Rectangle.Empty];
            Check(!(bool)cropParser.Invoke(null, arguments)!, $"Invalid crop coordinates accepted: {text}");
        }
    }
}
