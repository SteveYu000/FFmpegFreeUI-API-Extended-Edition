using System.Drawing;
using System.Windows.Forms;
using FFmpegFreeUI.Ext.PluginSdk;
using LakeUI;

namespace FFmpegFreeUI.Ext.BackgroundTest;

/// <summary>同时验证普通插件页和插件管理设置页的 LakeUI 背景映射。</summary>
public sealed class BackgroundTestPlugin : IExtFFmpegFreeUIPlugin
{
    private readonly List<IDisposable> _registrations = new();

    public string Id => "sample.lakeui-background-test";
    public string DisplayName => "LakeUI 背景测试";

    public void Initialize(IExtFFmpegFreeUIHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        if (host.ApiVersion < new Version(2, 4))
        {
            throw new NotSupportedException("LakeUI 背景测试需要 Ext Plugin API v2.4 或更新版本。");
        }

        if (host.PageEntries.AvailableTargets.Any(target =>
                target.TargetId.Equals(ExtFFmpegFreeUIPageTargets.MainPluginManager,
                    StringComparison.OrdinalIgnoreCase)))
        {
            _registrations.Add(host.PageEntries.RegisterPage(new ExtPluginPageExtension(
                "background-main-page",
                ExtFFmpegFreeUIPageTargets.MainPluginManager,
                ExtPluginRelativePosition.After,
                "LakeUI 背景测试",
                _ => CreateBackgroundPage("左侧插件页面"))));
        }
        else
        {
            host.Log(ExtPluginLogLevel.Warning, "宿主没有插件管理页目标，跳过左侧测试入口。");
        }

        _registrations.Add(host.PluginSettings.RegisterPage(
            new ExtPluginSettingsPageExtension(_ => CreateSettingsPage())));
    }

    private static Control CreateSettingsPage()
    {
        // 故意使用不透明 UserControl 外壳，验证宿主能找到内部的 ModernPanel1。
        var wrapper = new UserControl
        {
            BackColor = Color.FromArgb(27, 27, 27),
            Dock = DockStyle.Fill
        };
        wrapper.Controls.Add(CreateBackgroundPage("插件管理中的设置页"));
        return wrapper;
    }

    private static ModernPanel CreateBackgroundPage(string pageName)
    {
        var normalBackground = Color.FromArgb(30, 30, 34);
        var ownBackground = Color.FromArgb(29, 70, 120);
        var root = new ModernPanel
        {
            Name = "ModernPanel1",
            Dock = DockStyle.Fill,
            BackColor = normalBackground,
            BackColor1 = normalBackground,
            BorderSize = 0,
            Size = new Size(700, 460)
        };

        HtmlColorLabel AddLabel(string name, string text, float fontSize, Color color)
        {
            var label = new HtmlColorLabel
            {
                Name = name,
                BackColor = Color.Transparent,
                BackgroundSource = root,
                Font = new Font("Microsoft YaHei UI", fontSize),
                ForeColor = color,
                Text = text,
                TextAlign = HtmlColorLabel.TextAlignEnum.MiddleLeft
            };
            root.Controls.Add(label);
            return label;
        }

        var title = AddLabel("Title", "LakeUI 背景测试", 18F, Color.White);
        var description = AddLabel("Description",
            $"{pageName}<br>此按钮只更改本插件页面的背景，不会修改 FFmpegFreeUI 的全局个性化设置。",
            10F, Color.Gainsboro);
        var state = AddLabel("BackgroundState", "插件专属背景：关闭（使用宿主映射或普通底色）",
            11F, Color.White);
        var diagnostics = AddLabel("MappingState", "切换前可观察宿主背景；切换后应显示蓝色插件背景。",
            9F, Color.LightGray);

        var toggle = new ModernButton
        {
            Name = "TogglePluginBackground",
            BackColor = Color.Transparent,
            BackColor1 = Color.FromArgb(80, 95, 155, 210),
            BackgroundSource = root,
            BorderRadius = 10,
            BorderSize = 0,
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = Color.White,
            HoverBackColor1 = Color.FromArgb(110, 95, 155, 210),
            PressedBackColor1 = Color.FromArgb(140, 95, 155, 210),
            Text = "启用插件专属背景"
        };
        root.Controls.Add(toggle);

        Control? inheritedSource = null;
        var inheritedBackColor = normalBackground;
        var inheritedBackColor1 = normalBackground;
        var ownBackgroundEnabled = false;
        toggle.Click += (_, _) =>
        {
            if (!ownBackgroundEnabled)
            {
                // 工厂返回后宿主才会设置 BackgroundSource，因此在首次点击时保存映射。
                inheritedSource = root.BackgroundSource;
                inheritedBackColor = root.BackColor;
                inheritedBackColor1 = root.BackColor1;
                root.BackColor = ownBackground;
                root.BackColor1 = ownBackground;
                root.BackgroundSource = null;
            }
            else
            {
                root.BackgroundSource = inheritedSource;
                root.BackColor = inheritedBackColor;
                root.BackColor1 = inheritedBackColor1;
            }

            ownBackgroundEnabled = !ownBackgroundEnabled;
            state.Text = ownBackgroundEnabled
                ? "插件专属背景：开启（蓝色，仅本页）"
                : "插件专属背景：关闭（使用宿主映射或普通底色）";
            toggle.Text = ownBackgroundEnabled ? "关闭插件专属背景" : "启用插件专属背景";
            diagnostics.Text = ownBackgroundEnabled
                ? "本页已断开宿主背景映射；另一个测试页面与全局设置不会改变。"
                : inheritedSource is null
                    ? "宿主当前没有为本页提供背景映射；显示普通深色底色。"
                    : "已恢复进入本页时的宿主背景映射。";
        };

        // LakeUI 子控件直接置于 GPU 根下；仅在尺寸变化时重新布局，不靠 Timer 重绘。
        root.SizeChanged += (_, _) => LayoutContent();
        LayoutContent();
        return root;

        void LayoutContent()
        {
            var scale = root.DeviceDpi / 96F;
            var margin = (int)Math.Round(28 * scale);
            var width = Math.Max(0, root.ClientSize.Width - margin * 2);
            Place(title, margin, (int)Math.Round(24 * scale), width, (int)Math.Round(52 * scale));
            Place(description, margin, (int)Math.Round(84 * scale), width, (int)Math.Round(76 * scale));
            Place(state, margin, (int)Math.Round(172 * scale), width, (int)Math.Round(44 * scale));
            Place(toggle, margin, (int)Math.Round(230 * scale),
                Math.Min(width, (int)Math.Round(260 * scale)), (int)Math.Round(44 * scale));
            Place(diagnostics, margin, (int)Math.Round(288 * scale), width, (int)Math.Round(60 * scale));
        }

        static void Place(Control control, int x, int y, int width, int height)
        {
            var bounds = new Rectangle(x, y, width, height);
            if (control.Bounds != bounds)
            {
                control.Bounds = bounds;
            }
        }
    }
}
