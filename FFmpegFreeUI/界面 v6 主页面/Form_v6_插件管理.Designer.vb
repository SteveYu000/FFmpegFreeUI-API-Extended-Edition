<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form_v6_插件管理
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表和运行时订阅。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                释放运行时资源()
                If components IsNot Nothing Then components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim ListColumn1 As LakeUI.UltraDetailListView.ListColumn = New LakeUI.UltraDetailListView.ListColumn()
        Dim ListColumn2 As LakeUI.UltraDetailListView.ListColumn = New LakeUI.UltraDetailListView.ListColumn()
        Dim ListColumn3 As LakeUI.UltraDetailListView.ListColumn = New LakeUI.UltraDetailListView.ListColumn()
        Dim ListColumn4 As LakeUI.UltraDetailListView.ListColumn = New LakeUI.UltraDetailListView.ListColumn()
        Dim ListColumn5 As LakeUI.UltraDetailListView.ListColumn = New LakeUI.UltraDetailListView.ListColumn()
        ModernPanel1 = New LakeUI.ModernPanel()
        插件设置布局 = New LakeUI.ModernPanel()
        P_插件设置内容 = New LakeUI.ModernPanel()
        P_插件设置标题栏 = New LakeUI.ModernPanel()
        HCL_插件设置标题 = New LakeUI.HtmlColorLabel()
        MB_返回插件详情 = New LakeUI.ModernButton()
        管理内容布局 = New LakeUI.ModernPanel()
        P_插件列表区域 = New LakeUI.ModernPanel()
        UDLV_插件列表 = New LakeUI.UltraDetailListView()
        P_空状态 = New LakeUI.ModernPanel()
        TLP_空状态 = New LakeUI.ModernPanel()
        L_空状态标题 = New LakeUI.HtmlColorLabel()
        L_空状态说明 = New LakeUI.HtmlColorLabel()
        L_插件目录路径 = New LakeUI.HtmlColorLabel()
        MB_空状态打开目录 = New LakeUI.ModernButton()
        MP_插件详情 = New LakeUI.ModernPanel()
        详情视图 = New 插件详情视图_v6()
        HCL_说明 = New LakeUI.HtmlColorLabel()
        管理工具栏 = New LakeUI.ModernPanel()
        HCL_概览 = New LakeUI.HtmlColorLabel()
        FLP_管理操作 = New LakeUI.ModernPanel()
        MB_打开目录 = New LakeUI.ModernButton()
        MB_刷新 = New LakeUI.ModernButton()
        MB_切换启用 = New LakeUI.ModernButton()
        MB_上移 = New LakeUI.ModernButton()
        MB_下移 = New LakeUI.ModernButton()
        MB_重启应用 = New LakeUI.ModernButton()
        HCL_页面标题 = New LakeUI.HtmlColorLabel()
        ModernPanel1.SuspendLayout()
        插件设置布局.SuspendLayout()
        P_插件设置标题栏.SuspendLayout()
        管理内容布局.SuspendLayout()
        P_插件列表区域.SuspendLayout()
        P_空状态.SuspendLayout()
        TLP_空状态.SuspendLayout()
        MP_插件详情.SuspendLayout()
        管理工具栏.SuspendLayout()
        FLP_管理操作.SuspendLayout()
        SuspendLayout()
        '
        ' ModernPanel1
        '
        ModernPanel1.BackColor = Color.Transparent
        ModernPanel1.BackColor1 = Color.Transparent
        ModernPanel1.BorderSize = 0
        ModernPanel1.Controls.Add(插件设置布局)
        ModernPanel1.Controls.Add(管理内容布局)
        ModernPanel1.Controls.Add(HCL_说明)
        ModernPanel1.Controls.Add(管理工具栏)
        ModernPanel1.Controls.Add(HCL_页面标题)
        ModernPanel1.Dock = DockStyle.Fill
        ModernPanel1.Location = New Point(0, 0)
        ModernPanel1.Name = "ModernPanel1"
        ModernPanel1.Padding = New Padding(20)
        ModernPanel1.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        ModernPanel1.Size = New Size(980, 680)
        ModernPanel1.TabIndex = 0
        '
        ' 插件设置布局
        '
        插件设置布局.BackColor = Color.Transparent
        插件设置布局.BackColor1 = Color.Transparent
        插件设置布局.BorderSize = 0
        插件设置布局.Controls.Add(P_插件设置内容)
        插件设置布局.Controls.Add(P_插件设置标题栏)
        插件设置布局.Dock = DockStyle.Fill
        插件设置布局.Location = New Point(20, 101)
        插件设置布局.Name = "插件设置布局"
        插件设置布局.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        插件设置布局.Size = New Size(940, 538)
        插件设置布局.TabIndex = 4
        插件设置布局.Visible = False
        '
        ' P_插件设置内容
        '
        P_插件设置内容.BackColor = Color.Transparent
        P_插件设置内容.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        P_插件设置内容.BorderRadius = 10
        P_插件设置内容.BorderSize = 0
        P_插件设置内容.Dock = DockStyle.Fill
        P_插件设置内容.Location = New Point(0, 54)
        P_插件设置内容.Name = "P_插件设置内容"
        P_插件设置内容.Padding = New Padding(20)
        P_插件设置内容.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        P_插件设置内容.Size = New Size(940, 484)
        P_插件设置内容.TabIndex = 1
        '
        ' P_插件设置标题栏
        '
        P_插件设置标题栏.BackColor = Color.Transparent
        P_插件设置标题栏.BackColor1 = Color.Transparent
        P_插件设置标题栏.BorderSize = 0
        P_插件设置标题栏.Controls.Add(HCL_插件设置标题)
        P_插件设置标题栏.Controls.Add(MB_返回插件详情)
        P_插件设置标题栏.Dock = DockStyle.Top
        P_插件设置标题栏.Location = New Point(0, 0)
        P_插件设置标题栏.Name = "P_插件设置标题栏"
        P_插件设置标题栏.Padding = New Padding(0, 10, 0, 10)
        P_插件设置标题栏.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        P_插件设置标题栏.Size = New Size(940, 54)
        P_插件设置标题栏.TabIndex = 0
        '
        ' HCL_插件设置标题
        '
        HCL_插件设置标题.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_插件设置标题.Dock = DockStyle.Fill
        HCL_插件设置标题.ForeColor = Color.FromArgb(CByte(140), CByte(255), CByte(255), CByte(255))
        HCL_插件设置标题.Location = New Point(145, 10)
        HCL_插件设置标题.Name = "HCL_插件设置标题"
        HCL_插件设置标题.Padding = New Padding(16, 0, 0, 0)
        HCL_插件设置标题.Size = New Size(795, 34)
        HCL_插件设置标题.TabIndex = 1
        HCL_插件设置标题.Text = "插件设置"
        HCL_插件设置标题.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.MiddleLeft
        '
        ' MB_返回插件详情
        '
        MB_返回插件详情.BackColor = Color.Transparent
        MB_返回插件详情.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_返回插件详情.BorderColor = Color.Transparent
        MB_返回插件详情.BorderRadius = 10
        MB_返回插件详情.BorderSize = 0
        MB_返回插件详情.Dock = DockStyle.Left
        MB_返回插件详情.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_返回插件详情.ForeColor = Color.CornflowerBlue
        MB_返回插件详情.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_返回插件详情.HoverBorderColor = Color.Transparent
        MB_返回插件详情.Location = New Point(0, 10)
        MB_返回插件详情.Margin = New Padding(0)
        MB_返回插件详情.Name = "MB_返回插件详情"
        MB_返回插件详情.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_返回插件详情.PressedBorderColor = Color.CornflowerBlue
        MB_返回插件详情.Size = New Size(145, 34)
        MB_返回插件详情.TabIndex = 0
        MB_返回插件详情.Text = "← 返回插件详情"
        '
        ' 管理内容布局
        '
        管理内容布局.BackColor = Color.Transparent
        管理内容布局.BackColor1 = Color.Transparent
        管理内容布局.BorderSize = 0
        管理内容布局.Controls.Add(P_插件列表区域)
        管理内容布局.Controls.Add(MP_插件详情)
        管理内容布局.Dock = DockStyle.Fill
        管理内容布局.Location = New Point(20, 101)
        管理内容布局.Name = "管理内容布局"
        管理内容布局.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        管理内容布局.Size = New Size(940, 538)
        管理内容布局.TabIndex = 3
        '
        ' P_插件列表区域
        '
        P_插件列表区域.BackColor = Color.Transparent
        P_插件列表区域.BackColor1 = Color.Transparent
        P_插件列表区域.BorderSize = 0
        P_插件列表区域.Controls.Add(UDLV_插件列表)
        P_插件列表区域.Controls.Add(P_空状态)
        P_插件列表区域.Dock = DockStyle.None
        P_插件列表区域.Location = New Point(0, 0)
        P_插件列表区域.Margin = New Padding(0, 0, 5, 0)
        P_插件列表区域.Name = "P_插件列表区域"
        P_插件列表区域.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        P_插件列表区域.Size = New Size(653, 538)
        P_插件列表区域.TabIndex = 0
        '
        ' UDLV_插件列表
        '
        UDLV_插件列表.AllowDragReorder = True
        UDLV_插件列表.BackgroundColor = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.BorderRadius = 10
        UDLV_插件列表.BorderSize = 0
        ListColumn1.Text = "顺序"
        ListColumn1.Width = 70
        ListColumn2.Text = "状态"
        ListColumn2.Width = 80
        ListColumn3.Text = "插件"
        ListColumn3.Width = 240
        ListColumn4.Text = "接口"
        ListColumn4.Width = 115
        ListColumn5.Text = "加载状态"
        ListColumn5.Width = 180
        UDLV_插件列表.Columns.Add(ListColumn1)
        UDLV_插件列表.Columns.Add(ListColumn2)
        UDLV_插件列表.Columns.Add(ListColumn3)
        UDLV_插件列表.Columns.Add(ListColumn4)
        UDLV_插件列表.Columns.Add(ListColumn5)
        UDLV_插件列表.Dock = DockStyle.Fill
        UDLV_插件列表.DragSelectZoneWidth = 300
        UDLV_插件列表.ForeColor = Color.Silver
        UDLV_插件列表.GroupBackColor = Color.FromArgb(CByte(36), CByte(36), CByte(36))
        UDLV_插件列表.GroupBorderColor = Color.Silver
        UDLV_插件列表.GroupForeColor = Color.Gainsboro
        UDLV_插件列表.GroupHeight = 35
        UDLV_插件列表.HeaderBackColor = Color.Transparent
        UDLV_插件列表.HeaderBorderColor = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.HeaderBorderWidth = 2
        UDLV_插件列表.HeaderForeColor = Color.DarkGray
        UDLV_插件列表.HeaderHeight = 40
        UDLV_插件列表.ItemCornerRadius = 10
        UDLV_插件列表.ItemPadding = New Padding(10, 6, 10, 6)
        UDLV_插件列表.ItemSelectedBackColor = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.Location = New Point(0, 0)
        UDLV_插件列表.MultiSelect = False
        UDLV_插件列表.Name = "UDLV_插件列表"
        UDLV_插件列表.Padding = New Padding(5, 0, 5, 5)
        UDLV_插件列表.ScrollBarThumbColor = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.ScrollBarThumbHoverColor = Color.FromArgb(CByte(120), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.ScrollBarTrackColor = Color.FromArgb(CByte(20), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.SelectionRectBorderColor = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.SelectionRectFillColor = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        UDLV_插件列表.Size = New Size(653, 538)
        UDLV_插件列表.TabIndex = 0
        '
        ' P_空状态
        '
        P_空状态.BackColor = Color.Transparent
        P_空状态.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        P_空状态.BorderRadius = 10
        P_空状态.BorderSize = 0
        P_空状态.Controls.Add(TLP_空状态)
        P_空状态.Dock = DockStyle.Fill
        P_空状态.Location = New Point(0, 0)
        P_空状态.Name = "P_空状态"
        P_空状态.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        P_空状态.Size = New Size(653, 538)
        P_空状态.TabIndex = 1
        P_空状态.Visible = False
        '
        ' TLP_空状态
        '
        TLP_空状态.BackColor = Color.Transparent
        TLP_空状态.BackColor1 = Color.Transparent
        TLP_空状态.BorderSize = 0
        TLP_空状态.Controls.Add(L_空状态标题)
        TLP_空状态.Controls.Add(L_空状态说明)
        TLP_空状态.Controls.Add(L_插件目录路径)
        TLP_空状态.Controls.Add(MB_空状态打开目录)
        TLP_空状态.Dock = DockStyle.Fill
        TLP_空状态.Location = New Point(0, 0)
        TLP_空状态.Name = "TLP_空状态"
        TLP_空状态.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        TLP_空状态.Size = New Size(653, 538)
        TLP_空状态.TabIndex = 0
        '
        ' L_空状态标题
        '
        L_空状态标题.AutoSizeMode = AutoSizeMode.GrowAndShrink
        L_空状态标题.BackColor = Color.Transparent
        L_空状态标题.Dock = DockStyle.None
        L_空状态标题.Font = New Font("Microsoft YaHei UI", 13.0F)
        L_空状态标题.ForeColor = Color.Silver
        L_空状态标题.Location = New Point(3, 144)
        L_空状态标题.Name = "L_空状态标题"
        L_空状态标题.Size = New Size(647, 44)
        L_空状态标题.TabIndex = 0
        L_空状态标题.Text = "没有找到插件"
        L_空状态标题.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.Center
        '
        ' L_空状态说明
        '
        L_空状态说明.AutoSizeMode = AutoSizeMode.GrowAndShrink
        L_空状态说明.BackColor = Color.Transparent
        L_空状态说明.Dock = DockStyle.None
        L_空状态说明.Font = New Font("Microsoft YaHei UI", 10.0F)
        L_空状态说明.ForeColor = Color.FromArgb(CByte(150), CByte(255), CByte(255), CByte(255))
        L_空状态说明.Location = New Point(3, 188)
        L_空状态说明.Name = "L_空状态说明"
        L_空状态说明.Size = New Size(647, 38)
        L_空状态说明.TabIndex = 1
        L_空状态说明.Text = "将 *.3fui.dll 拖放到本页，或放入插件目录后刷新。"
        L_空状态说明.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.Center
        '
        ' L_插件目录路径
        '
        L_插件目录路径.AutoSizeMode = AutoSizeMode.GrowAndShrink
        L_插件目录路径.BackColor = Color.Transparent
        L_插件目录路径.Dock = DockStyle.None
        L_插件目录路径.Font = New Font("Microsoft YaHei UI", 9.0F)
        L_插件目录路径.ForeColor = Color.FromArgb(CByte(110), CByte(255), CByte(255), CByte(255))
        L_插件目录路径.Location = New Point(3, 226)
        L_插件目录路径.Name = "L_插件目录路径"
        L_插件目录路径.Padding = New Padding(24, 0, 24, 0)
        L_插件目录路径.Size = New Size(647, 54)
        L_插件目录路径.TabIndex = 2
        L_插件目录路径.Text = "<程序目录>\Plugin"
        L_插件目录路径.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.Center
        '
        ' MB_空状态打开目录
        '
        MB_空状态打开目录.Anchor = AnchorStyles.None
        MB_空状态打开目录.BackColor = Color.Transparent
        MB_空状态打开目录.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_空状态打开目录.BorderColor = Color.Transparent
        MB_空状态打开目录.BorderRadius = 10
        MB_空状态打开目录.BorderSize = 0
        MB_空状态打开目录.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_空状态打开目录.ForeColor = Color.CornflowerBlue
        MB_空状态打开目录.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_空状态打开目录.HoverBorderColor = Color.Transparent
        MB_空状态打开目录.Location = New Point(261, 285)
        MB_空状态打开目录.Margin = New Padding(0)
        MB_空状态打开目录.Name = "MB_空状态打开目录"
        MB_空状态打开目录.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_空状态打开目录.PressedBorderColor = Color.CornflowerBlue
        MB_空状态打开目录.Size = New Size(130, 34)
        MB_空状态打开目录.TabIndex = 3
        MB_空状态打开目录.Text = "打开插件目录"
        '
        ' MP_插件详情
        '
        MP_插件详情.BackColor = Color.Transparent
        MP_插件详情.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MP_插件详情.BorderRadius = 10
        MP_插件详情.BorderSize = 0
        MP_插件详情.Controls.Add(详情视图)
        MP_插件详情.Dock = DockStyle.None
        MP_插件详情.Location = New Point(663, 0)
        MP_插件详情.Margin = New Padding(5, 0, 0, 0)
        MP_插件详情.Name = "MP_插件详情"
        MP_插件详情.Padding = New Padding(20)
        MP_插件详情.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        MP_插件详情.Size = New Size(277, 538)
        MP_插件详情.TabIndex = 1
        '
        ' 详情视图
        '
        详情视图.BackColor = Color.Transparent
        详情视图.Dock = DockStyle.Fill
        详情视图.Location = New Point(20, 20)
        详情视图.Margin = New Padding(0)
        详情视图.Name = "详情视图"
        详情视图.Size = New Size(237, 560)
        详情视图.TabIndex = 0
        详情视图.TabStop = False
        '
        ' HCL_说明
        '
        HCL_说明.AutoSize = True
        HCL_说明.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_说明.Dock = DockStyle.Bottom
        HCL_说明.ForeColor = Color.FromArgb(CByte(120), CByte(255), CByte(255), CByte(255))
        HCL_说明.Location = New Point(20, 639)
        HCL_说明.Name = "HCL_说明"
        HCL_说明.Padding = New Padding(0, 10, 0, 0)
        HCL_说明.Size = New Size(940, 21)
        HCL_说明.TabIndex = 4
        HCL_说明.Text = "可将 *.3fui.dll 拖放到本页安装；同一事件按列表从上到下依次处理，拖动或上移/下移立即生效。"
        HCL_说明.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.MiddleLeft
        '
        ' 管理工具栏
        '
        管理工具栏.BackColor = Color.Transparent
        管理工具栏.BackColor1 = Color.Transparent
        管理工具栏.BorderSize = 0
        管理工具栏.Controls.Add(HCL_概览)
        管理工具栏.Controls.Add(FLP_管理操作)
        管理工具栏.Dock = DockStyle.Top
        管理工具栏.Location = New Point(20, 47)
        管理工具栏.Name = "管理工具栏"
        管理工具栏.Padding = New Padding(0, 10, 0, 10)
        管理工具栏.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        管理工具栏.Size = New Size(940, 54)
        管理工具栏.TabIndex = 2
        '
        ' HCL_概览
        '
        HCL_概览.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_概览.Dock = DockStyle.None
        HCL_概览.ForeColor = Color.FromArgb(CByte(120), CByte(255), CByte(255), CByte(255))
        HCL_概览.Location = New Point(590, 10)
        HCL_概览.Name = "HCL_概览"
        HCL_概览.Padding = New Padding(15, 0, 0, 0)
        HCL_概览.Size = New Size(350, 34)
        HCL_概览.TabIndex = 1
        HCL_概览.Text = "0 个插件   0 个已启用   无需重启"
        HCL_概览.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.MiddleRight
        '
        ' FLP_管理操作
        '
        FLP_管理操作.BackColor = Color.Transparent
        FLP_管理操作.BackColor1 = Color.Transparent
        FLP_管理操作.BorderSize = 0
        FLP_管理操作.Controls.Add(MB_打开目录)
        FLP_管理操作.Controls.Add(MB_刷新)
        FLP_管理操作.Controls.Add(MB_切换启用)
        FLP_管理操作.Controls.Add(MB_上移)
        FLP_管理操作.Controls.Add(MB_下移)
        FLP_管理操作.Controls.Add(MB_重启应用)
        FLP_管理操作.Dock = DockStyle.None
        FLP_管理操作.Location = New Point(0, 10)
        FLP_管理操作.Margin = New Padding(0)
        FLP_管理操作.Name = "FLP_管理操作"
        FLP_管理操作.ScrollBarMode = LakeUI.ModernPanel.ScrollMode.None
        FLP_管理操作.Size = New Size(590, 34)
        FLP_管理操作.TabIndex = 0
        '
        ' MB_打开目录
        '
        MB_打开目录.BackColor = Color.Transparent
        MB_打开目录.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_打开目录.BorderColor = Color.Transparent
        MB_打开目录.BorderRadius = 10
        MB_打开目录.BorderSize = 0
        MB_打开目录.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_打开目录.ForeColor = Color.CornflowerBlue
        MB_打开目录.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_打开目录.HoverBorderColor = Color.Transparent
        MB_打开目录.Location = New Point(0, 0)
        MB_打开目录.Margin = New Padding(0, 0, 10, 0)
        MB_打开目录.Name = "MB_打开目录"
        MB_打开目录.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_打开目录.PressedBorderColor = Color.CornflowerBlue
        MB_打开目录.Size = New Size(130, 34)
        MB_打开目录.TabIndex = 0
        MB_打开目录.Text = "打开插件目录"
        '
        ' MB_刷新
        '
        MB_刷新.BackColor = Color.Transparent
        MB_刷新.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_刷新.BorderColor = Color.Transparent
        MB_刷新.BorderRadius = 10
        MB_刷新.BorderSize = 0
        MB_刷新.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_刷新.ForeColor = Color.CornflowerBlue
        MB_刷新.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_刷新.HoverBorderColor = Color.Transparent
        MB_刷新.Location = New Point(140, 0)
        MB_刷新.Margin = New Padding(0, 0, 10, 0)
        MB_刷新.Name = "MB_刷新"
        MB_刷新.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_刷新.PressedBorderColor = Color.CornflowerBlue
        MB_刷新.Size = New Size(70, 34)
        MB_刷新.TabIndex = 1
        MB_刷新.Text = "刷新"
        '
        ' MB_切换启用
        '
        MB_切换启用.BackColor = Color.Transparent
        MB_切换启用.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_切换启用.BorderColor = Color.Transparent
        MB_切换启用.BorderRadius = 10
        MB_切换启用.BorderSize = 0
        MB_切换启用.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_切换启用.ForeColor = Color.YellowGreen
        MB_切换启用.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_切换启用.HoverBorderColor = Color.Transparent
        MB_切换启用.Location = New Point(220, 0)
        MB_切换启用.Margin = New Padding(0, 0, 10, 0)
        MB_切换启用.Name = "MB_切换启用"
        MB_切换启用.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_切换启用.PressedBorderColor = Color.YellowGreen
        MB_切换启用.Size = New Size(100, 34)
        MB_切换启用.TabIndex = 2
        MB_切换启用.Text = "启用插件"
        '
        ' MB_上移
        '
        MB_上移.BackColor = Color.Transparent
        MB_上移.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_上移.BorderColor = Color.Transparent
        MB_上移.BorderRadius = 10
        MB_上移.BorderSize = 0
        MB_上移.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_上移.ForeColor = Color.CornflowerBlue
        MB_上移.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_上移.HoverBorderColor = Color.Transparent
        MB_上移.Location = New Point(330, 0)
        MB_上移.Margin = New Padding(0, 0, 10, 0)
        MB_上移.Name = "MB_上移"
        MB_上移.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_上移.PressedBorderColor = Color.CornflowerBlue
        MB_上移.Size = New Size(65, 34)
        MB_上移.TabIndex = 3
        MB_上移.Text = "上移"
        '
        ' MB_下移
        '
        MB_下移.BackColor = Color.Transparent
        MB_下移.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_下移.BorderColor = Color.Transparent
        MB_下移.BorderRadius = 10
        MB_下移.BorderSize = 0
        MB_下移.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_下移.ForeColor = Color.CornflowerBlue
        MB_下移.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_下移.HoverBorderColor = Color.Transparent
        MB_下移.Location = New Point(405, 0)
        MB_下移.Margin = New Padding(0, 0, 10, 0)
        MB_下移.Name = "MB_下移"
        MB_下移.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_下移.PressedBorderColor = Color.CornflowerBlue
        MB_下移.Size = New Size(65, 34)
        MB_下移.TabIndex = 4
        MB_下移.Text = "下移"
        '
        ' MB_重启应用
        '
        MB_重启应用.BackColor = Color.Transparent
        MB_重启应用.BackColor1 = Color.FromArgb(CByte(40), CByte(220), CByte(220), CByte(220))
        MB_重启应用.BorderColor = Color.Transparent
        MB_重启应用.BorderRadius = 10
        MB_重启应用.BorderSize = 0
        MB_重启应用.Font = New Font("Microsoft YaHei UI", 10.0F)
        MB_重启应用.ForeColor = Color.Goldenrod
        MB_重启应用.HoverBackColor1 = Color.FromArgb(CByte(60), CByte(220), CByte(220), CByte(220))
        MB_重启应用.HoverBorderColor = Color.Transparent
        MB_重启应用.Location = New Point(480, 0)
        MB_重启应用.Margin = New Padding(0)
        MB_重启应用.Name = "MB_重启应用"
        MB_重启应用.PressedBackColor1 = Color.FromArgb(CByte(80), CByte(220), CByte(220), CByte(220))
        MB_重启应用.PressedBorderColor = Color.Goldenrod
        MB_重启应用.Size = New Size(110, 34)
        MB_重启应用.TabIndex = 5
        MB_重启应用.Text = "重启并应用"
        '
        ' HCL_页面标题
        '
        HCL_页面标题.AutoSize = True
        HCL_页面标题.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_页面标题.Dock = DockStyle.Top
        HCL_页面标题.ForeColor = Color.FromArgb(CByte(120), CByte(255), CByte(255), CByte(255))
        HCL_页面标题.Location = New Point(20, 20)
        HCL_页面标题.Name = "HCL_页面标题"
        HCL_页面标题.Size = New Size(940, 27)
        HCL_页面标题.TabIndex = 0
        HCL_页面标题.Text = "<span style=""font-size:13; color:Silver"">插件管理</span>   查看插件状态、接口兼容性和事件处理顺序"
        HCL_页面标题.TextAlign = LakeUI.HtmlColorLabel.TextAlignEnum.MiddleLeft
        '
        ' Form_v6_插件管理
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        BackColor = Color.FromArgb(CByte(24), CByte(24), CByte(24))
        ClientSize = New Size(980, 680)
        Controls.Add(ModernPanel1)
        Font = New Font("Microsoft YaHei UI", 10.0F)
        ForeColor = Color.Silver
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(900, 580)
        Name = "Form_v6_插件管理"
        Text = "插件管理"
        ModernPanel1.ResumeLayout(False)
        ModernPanel1.PerformLayout()
        插件设置布局.ResumeLayout(False)
        P_插件设置标题栏.ResumeLayout(False)
        管理内容布局.ResumeLayout(False)
        P_插件列表区域.ResumeLayout(False)
        P_空状态.ResumeLayout(False)
        TLP_空状态.ResumeLayout(False)
        MP_插件详情.ResumeLayout(False)
        管理工具栏.ResumeLayout(False)
        管理工具栏.PerformLayout()
        FLP_管理操作.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Public WithEvents ModernPanel1 As LakeUI.ModernPanel
    Friend WithEvents 插件设置布局 As LakeUI.ModernPanel
    Friend WithEvents P_插件设置内容 As LakeUI.ModernPanel
    Friend WithEvents P_插件设置标题栏 As LakeUI.ModernPanel
    Friend WithEvents HCL_插件设置标题 As LakeUI.HtmlColorLabel
    Friend WithEvents MB_返回插件详情 As LakeUI.ModernButton
    Friend WithEvents 管理内容布局 As LakeUI.ModernPanel
    Friend WithEvents P_插件列表区域 As LakeUI.ModernPanel
    Friend WithEvents UDLV_插件列表 As LakeUI.UltraDetailListView
    Friend WithEvents P_空状态 As LakeUI.ModernPanel
    Friend WithEvents TLP_空状态 As LakeUI.ModernPanel
    Friend WithEvents L_空状态标题 As LakeUI.HtmlColorLabel
    Friend WithEvents L_空状态说明 As LakeUI.HtmlColorLabel
    Friend WithEvents L_插件目录路径 As LakeUI.HtmlColorLabel
    Friend WithEvents MB_空状态打开目录 As LakeUI.ModernButton
    Friend WithEvents MP_插件详情 As LakeUI.ModernPanel
    Friend WithEvents 详情视图 As 插件详情视图_v6
    Friend WithEvents HCL_说明 As LakeUI.HtmlColorLabel
    Friend WithEvents 管理工具栏 As LakeUI.ModernPanel
    Friend WithEvents HCL_概览 As LakeUI.HtmlColorLabel
    Friend WithEvents FLP_管理操作 As LakeUI.ModernPanel
    Friend WithEvents MB_打开目录 As LakeUI.ModernButton
    Friend WithEvents MB_刷新 As LakeUI.ModernButton
    Friend WithEvents MB_切换启用 As LakeUI.ModernButton
    Friend WithEvents MB_上移 As LakeUI.ModernButton
    Friend WithEvents MB_下移 As LakeUI.ModernButton
    Friend WithEvents MB_重启应用 As LakeUI.ModernButton
    Friend WithEvents HCL_页面标题 As LakeUI.HtmlColorLabel
End Class
