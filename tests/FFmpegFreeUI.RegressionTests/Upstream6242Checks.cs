using System.Drawing;
using System.Reflection;
using FFmpegFreeUI;
using LakeUI;

internal static partial class Program
{
    private static void TestHdrModelAvailability()
    {
        using (var parameters = new Form_v6_参数面板())
        {
            parameters.初始化默认参数();
            预设管理_v6.显示预设(new 预设数据_v6 { 输出容器 = "mkv", 自定义参数_音频滤镜 = "volume=0.5" }, parameters);
            var edited = 预设管理_v6.从面板创建预设(parameters);
            Check(edited.滤镜排序系统.Any(filter => filter.自定义滤镜内容 == "volume=0.5"),
                "Legacy audio-filter fixture migrates into the live ordered filters before repeated initialization");
            parameters.初始化默认参数();
            var state = 预设管理_v6.从面板创建预设(parameters);
            Check(state.输出容器 == edited.输出容器 && state.滤镜排序系统.Any(filter => filter.自定义滤镜内容 == "volume=0.5"),
                "One-time startup initialization does not erase later plugin/preset edits");
        }
        var catalog = (List<VmafModelDisplayItem>)StaticQuality("构建Vmaf模型显示项", Array.Empty<string>(), false);
        Check(catalog.Count(model => model.IsHdr) == 8 && catalog.Where(model => model.IsHdr).All(model => !model.IsRuntimeDetected),
            "Official HDR catalog must not imply runtime availability");
        Check(catalog.Count(model => model.IsNonNeg) == 4 && catalog.Where(model => model.UseCuda).All(model => !model.IsCudaAvailable),
            "Legacy non-NEG catalog keeps all four choices without claiming CUDA support");
        void MustReject(VmafModelDisplayItem model, string explanation)
        {
            try { StaticQuality("检查Vmaf模型可用性", model); throw new InvalidOperationException(explanation); }
            catch (TargetInvocationException error) when (error.InnerException is InvalidOperationException) { checks++; }
        }
        MustReject(catalog.First(model => model.IsHdr), "Undetected HDR must not reach FFmpeg");
        MustReject(catalog.First(model => model.UseCuda), "Missing CUDA filter must not reach FFmpeg");
        var scanned = (List<VmafModelDisplayItem>)StaticQuality("构建Vmaf模型显示项", new[] {
            "vmaf_v1.0.16_3d0h", "hdr_vmaf_v1.1.8_3d0h", "hdr_vmaf_v1.1.8_hfr_3d0h"
        }, true);
        Check(Auto(scanned, 1920, 1080, 24, out _, true).ModelValue == "hdr_vmaf_v1.1.8_3d0h",
            "PQ/HLG AUTO prefers a detected HDR model");
        Check(Auto(scanned, 1920, 1080, 60, out _, true).ModelValue == "hdr_vmaf_v1.1.8_hfr_3d0h",
            "HDR high frame rate selects the detected HFR model");
        Check(!Auto(scanned, 1920, 1080, 24, out _).IsHdr, "SDR AUTO does not silently use HDR calibration");
        var sdrOnly = (List<VmafModelDisplayItem>)StaticQuality("构建Vmaf模型显示项", new[] { "vmaf_v1.0.16_3d0h" }, false);
        Check(!Auto(sdrOnly, 1920, 1080, 24, out var warning, true).IsHdr && warning.Contains("警告") && warning.Contains("HDR"),
            "Catalog-only HDR falls back to detected SDR with an explicit warning");
        using var combo = new VmafModelComboBox();
        combo.ReplaceDisplayItems(catalog);
        var header = combo.Models.ToList().FindIndex(model => model.IsSectionHeader) + 1;
        combo.SelectedIndex = header;
        Check(header > 1 && combo.SelectedModel?.IsAuto == true, "Non-NEG section header is not a selectable model");
        combo.AddDisplayItem(new() { ModelValue = "local-hdr.json", IsLocal = true });
        Check(combo.SelectedModelValue == "local-hdr.json" &&
            combo.Models.ToList().FindIndex(model => model.IsLocal) < combo.Models.ToList().FindIndex(model => model.IsSectionHeader),
            "Local models remain selectable before the non-NEG warning section");
        var infoType = typeof(Form_v6_集成工具_质量评测).GetNestedType("视频流信息", BindingFlags.NonPublic)!;
        var info = Activator.CreateInstance(infoType, true)!;
        void SetColor(string name, string value) => infoType.GetProperty(name)!.SetValue(info, value);
        SetColor("色彩范围", "tv"); SetColor("色彩原色", "bt2020");
        SetColor("色彩传递", "smpte2084"); SetColor("色彩矩阵", "bt2020nc");
        Check((bool)StaticQuality("是Hdr视频", info), "PQ transfer is recognized as HDR");
        Check(((string)StaticQuality("构建Hdr色彩标记", info)).Contains("range=limited:color_primaries=bt2020:color_trc=smpte2084:colorspace=bt2020nc"),
            "HDR filtering retains the full input color tags");
        SetColor("色彩传递", "arib-std-b67");
        Check((bool)StaticQuality("是Hdr视频", info), "HLG transfer is recognized as HDR");
        SetColor("色彩矩阵", "");
        try { StaticQuality("构建Hdr色彩标记", info); throw new InvalidOperationException("Incomplete HDR tags must fail closed"); }
        catch (TargetInvocationException error) when (error.InnerException is InvalidOperationException) { checks++; }
        TestUnifiedSelectionColors();
    }

    private static void TestUnifiedSelectionColors()
    {
        var previousTheme = 设置_v6.实例对象.界面主题;
        using var form = new Form();
        using var opaque = new ModernComboBox { SelectionColor = Color.FromArgb(255, 1, 2, 3) };
        using var translucent = new ModernComboBox { SelectionColor = Color.FromArgb(40, 1, 2, 3) };
        form.Controls.AddRange([opaque, translucent]);
        try
        {
            foreach (var theme in new[] { 1, 2 })
            {
                设置_v6.实例对象.界面主题 = theme;
                界面主题_v6.刷新主题(true);
                界面主题_v6.应用当前主题到窗体(form);
                Check(opaque.SelectionColor == 界面配色_v6.获取选中背景(theme == 1), "Opaque selection color follows the selected theme");
                Check(translucent.SelectionColor.A == 40, "Theme conversion preserves translucent selection alpha");
            }
        }
        finally { 设置_v6.实例对象.界面主题 = previousTheme; 界面主题_v6.刷新主题(true); }
    }
}
