Imports System.Diagnostics
Imports System.IO
Imports LakeUI

Public Class Form_v6_插件管理
    Inherits Form

    Public ReadOnly ModernPanel1 As New ModernPanel()
    Private WithEvents UDLV_插件列表 As New UltraDetailListView()
    Private WithEvents MB_切换启用 As New ModernButton()
    Private WithEvents MB_上移 As New ModernButton()
    Private WithEvents MB_下移 As New ModernButton()
    Private WithEvents MB_刷新 As New ModernButton()
    Private WithEvents MB_打开目录 As New ModernButton()
    Private WithEvents MB_重启应用 As New ModernButton()
    Private WithEvents MB_空状态打开目录 As New ModernButton()
    Private ReadOnly P_空状态 As New ModernPanel()
    Private ReadOnly HCL_页面标题 As New HtmlColorLabel()
    Private ReadOnly HCL_概览 As New HtmlColorLabel()
    Private ReadOnly 详情视图 As New 插件详情视图_v6()
    Private ReadOnly HCL_说明 As New HtmlColorLabel()
    Private ReadOnly 快照 As New Dictionary(Of String, 插件信息_v6)(StringComparer.OrdinalIgnoreCase)
    Private 正在填充列表 As Boolean
    Private 忽略管理器通知 As Boolean

    Public Sub New()
        MyBase.New()
        初始化界面()
        AddHandler 插件管理.插件列表已变化, AddressOf 插件列表变化
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then RemoveHandler 插件管理.插件列表已变化, AddressOf 插件列表变化
        MyBase.Dispose(disposing)
    End Sub

    Private Sub 初始化界面()
        SuspendLayout()
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        BackColor = Color.FromArgb(24, 24, 24)
        ClientSize = New Size(980, 680)
        Font = New Font("Microsoft YaHei UI", 10.0F)
        ForeColor = Color.Silver
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(900, 580)
        Name = NameOf(Form_v6_插件管理)
        Text = "插件管理"

        ModernPanel1.BackColor = Color.Transparent
        ModernPanel1.BackColor1 = Color.Transparent
        ModernPanel1.BorderSize = 0
        ModernPanel1.Dock = DockStyle.Fill
        ModernPanel1.Name = "ModernPanel1"
        ModernPanel1.Padding = New Padding(20)
        HCL_页面标题.AutoSize = True
        HCL_页面标题.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_页面标题.Dock = DockStyle.Top
        HCL_页面标题.ForeColor = Color.FromArgb(120, 255, 255, 255)
        HCL_页面标题.Text = "<span style=""font-size:13; color:Silver"">插件管理</span>   查看插件状态、接口兼容性和事件处理顺序"
        HCL_页面标题.TextAlign = HtmlColorLabel.TextAlignEnum.MiddleLeft

        Dim toolbar = 创建工具栏()
        Dim contentLayout = 创建内容布局()

        HCL_说明.AutoSize = True
        HCL_说明.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_说明.Dock = DockStyle.Bottom
        HCL_说明.ForeColor = Color.FromArgb(120, 255, 255, 255)
        HCL_说明.Padding = New Padding(0, 10, 0, 0)
        HCL_说明.Text = "可将 *.3fui.dll 拖放到本页安装；同一事件按列表从上到下依次处理，拖动或上移/下移立即生效。"
        HCL_说明.TextAlign = HtmlColorLabel.TextAlignEnum.MiddleLeft

        ModernPanel1.Controls.Add(contentLayout)
        ModernPanel1.Controls.Add(HCL_说明)
        ModernPanel1.Controls.Add(toolbar)
        ModernPanel1.Controls.Add(HCL_页面标题)
        Controls.Add(ModernPanel1)
        注册插件拖放目标(Me)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Private Function 创建工具栏() As Control
        Dim toolbar As New Panel With {
            .BackColor = Color.Transparent,
            .Dock = DockStyle.Top,
            .Height = 54,
            .Padding = New Padding(0, 10, 0, 10)
        }
        Dim actions As New FlowLayoutPanel With {
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BackColor = Color.Transparent,
            .Dock = DockStyle.Left,
            .FlowDirection = FlowDirection.LeftToRight,
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .WrapContents = False
        }
        配置按钮(MB_打开目录, "打开插件目录", 130, Color.CornflowerBlue)
        配置按钮(MB_刷新, "刷新", 70, Color.CornflowerBlue)
        配置按钮(MB_切换启用, "启用插件", 100, Color.YellowGreen)
        配置按钮(MB_上移, "上移", 65, Color.CornflowerBlue)
        配置按钮(MB_下移, "下移", 65, Color.CornflowerBlue)
        配置按钮(MB_重启应用, "重启并应用", 110, Color.Goldenrod)
        MB_重启应用.Margin = New Padding(0)
        actions.Controls.AddRange({MB_打开目录, MB_刷新, MB_切换启用, MB_上移, MB_下移, MB_重启应用})

        HCL_概览.AutoSizeMode = AutoSizeMode.GrowAndShrink
        HCL_概览.Dock = DockStyle.Fill
        HCL_概览.ForeColor = Color.FromArgb(120, 255, 255, 255)
        HCL_概览.Padding = New Padding(15, 0, 0, 0)
        HCL_概览.Text = "0 个插件   0 个已启用   无需重启"
        HCL_概览.TextAlign = HtmlColorLabel.TextAlignEnum.MiddleRight

        toolbar.Controls.Add(HCL_概览)
        toolbar.Controls.Add(actions)
        Return toolbar
    End Function

    Private Function 创建内容布局() As Control
        Dim contentLayout As New TableLayoutPanel With {
            .BackColor = Color.Transparent,
            .ColumnCount = 2,
            .Dock = DockStyle.Fill,
            .RowCount = 1
        }
        contentLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70.0F))
        contentLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30.0F))
        contentLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        Dim listBody As New Panel With {
            .BackColor = Color.Transparent,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0, 0, 5, 0)
        }
        UDLV_插件列表.AllowDragReorder = True
        UDLV_插件列表.BackgroundColor = Color.FromArgb(40, 220, 220, 220)
        UDLV_插件列表.BorderRadius = 10
        UDLV_插件列表.BorderSize = 0
        UDLV_插件列表.Dock = DockStyle.Fill
        UDLV_插件列表.DragSelectZoneWidth = 300
        UDLV_插件列表.ForeColor = Color.Silver
        UDLV_插件列表.GroupBackColor = Color.FromArgb(36, 36, 36)
        UDLV_插件列表.GroupBorderColor = Color.Silver
        UDLV_插件列表.GroupForeColor = Color.Gainsboro
        UDLV_插件列表.GroupHeight = 35
        UDLV_插件列表.HeaderBackColor = Color.Transparent
        UDLV_插件列表.HeaderBorderColor = Color.FromArgb(80, 220, 220, 220)
        UDLV_插件列表.HeaderBorderWidth = 2
        UDLV_插件列表.HeaderForeColor = Color.DarkGray
        UDLV_插件列表.HeaderHeight = 40
        UDLV_插件列表.ItemCornerRadius = 10
        UDLV_插件列表.ItemPadding = New Padding(10, 6, 10, 6)
        UDLV_插件列表.ItemSelectedBackColor = Color.FromArgb(40, 220, 220, 220)
        UDLV_插件列表.MultiSelect = False
        UDLV_插件列表.Padding = New Padding(5, 0, 5, 5)
        UDLV_插件列表.ScrollBarThumbColor = Color.FromArgb(40, 220, 220, 220)
        UDLV_插件列表.ScrollBarThumbHoverColor = Color.FromArgb(120, 220, 220, 220)
        UDLV_插件列表.ScrollBarTrackColor = Color.FromArgb(20, 220, 220, 220)
        UDLV_插件列表.SelectionRectBorderColor = Color.FromArgb(80, 220, 220, 220)
        UDLV_插件列表.SelectionRectFillColor = Color.FromArgb(40, 220, 220, 220)
        添加列("顺序", 70)
        添加列("状态", 80)
        添加列("插件", 240)
        添加列("接口", 115)
        添加列("加载状态", 180)

        配置空状态()
        listBody.Controls.Add(UDLV_插件列表)
        listBody.Controls.Add(P_空状态)

        Dim detailPanel As New ModernPanel With {
            .BackColor = Color.Transparent,
            .BackColor1 = Color.FromArgb(40, 220, 220, 220),
            .BorderRadius = 10,
            .BorderSize = 0,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(5, 0, 0, 0),
            .Padding = New Padding(20),
            .ScrollBarMode = ModernPanel.ScrollMode.Vertical
        }
        detailPanel.Controls.Add(详情视图)
        contentLayout.Controls.Add(listBody, 0, 0)
        contentLayout.Controls.Add(detailPanel, 1, 0)
        Return contentLayout
    End Function

    Private Shared Sub 配置按钮(button As ModernButton, text As String, width As Integer, foreColor As Color)
        button.BackColor = Color.Transparent
        button.BackColor1 = Color.FromArgb(40, 220, 220, 220)
        button.BorderColor = Color.Transparent
        button.BorderRadius = 10
        button.BorderSize = 0
        button.Font = New Font("Microsoft YaHei UI", 10.0F)
        button.ForeColor = foreColor
        button.HoverBackColor1 = Color.FromArgb(60, 220, 220, 220)
        button.HoverBorderColor = Color.Transparent
        button.Margin = New Padding(0, 0, 10, 0)
        button.PressedBackColor1 = Color.FromArgb(80, 220, 220, 220)
        button.PressedBorderColor = foreColor
        button.Size = New Size(width, 34)
        button.Text = text
    End Sub

    Private Shared Function 创建文本标签(text As String, fontSize As Single, color As Color) As Label
        Return New Label With {
            .AutoEllipsis = True,
            .BackColor = Color.Transparent,
            .Dock = DockStyle.Fill,
            .Font = New Font("Microsoft YaHei UI", fontSize),
            .ForeColor = color,
            .Text = text,
            .TextAlign = ContentAlignment.MiddleLeft
        }
    End Function

    Private Sub 配置空状态()
        P_空状态.BackColor = Color.Transparent
        P_空状态.BackColor1 = Color.FromArgb(40, 220, 220, 220)
        P_空状态.BorderRadius = 10
        P_空状态.BorderSize = 0
        P_空状态.Dock = DockStyle.Fill
        P_空状态.Visible = False

        Dim emptyLayout As New TableLayoutPanel With {
            .BackColor = Color.Transparent,
            .ColumnCount = 1,
            .Dock = DockStyle.Fill,
            .RowCount = 6
        }
        emptyLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        emptyLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 45.0F))
        emptyLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        emptyLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38.0F))
        emptyLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 54.0F))
        emptyLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        emptyLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 55.0F))

        Dim emptyTitle = 创建文本标签("没有找到插件", 13.0F, Color.Silver)
        emptyTitle.TextAlign = ContentAlignment.MiddleCenter
        Dim emptyDescription = 创建文本标签("将 *.3fui.dll 拖放到本页，或放入插件目录后刷新。", 10.0F, Color.FromArgb(150, 255, 255, 255))
        emptyDescription.TextAlign = ContentAlignment.MiddleCenter
        Dim pathLabel = 创建文本标签(插件管理.插件文件夹路径, 9.0F, Color.FromArgb(110, 255, 255, 255))
        pathLabel.Padding = New Padding(24, 0, 24, 0)
        pathLabel.TextAlign = ContentAlignment.MiddleCenter
        配置按钮(MB_空状态打开目录, "打开插件目录", 130, Color.CornflowerBlue)
        MB_空状态打开目录.Anchor = AnchorStyles.None
        MB_空状态打开目录.Margin = New Padding(0)

        emptyLayout.Controls.Add(emptyTitle, 0, 1)
        emptyLayout.Controls.Add(emptyDescription, 0, 2)
        emptyLayout.Controls.Add(pathLabel, 0, 3)
        emptyLayout.Controls.Add(MB_空状态打开目录, 0, 4)
        P_空状态.Controls.Add(emptyLayout)
    End Sub

    Private Sub 添加列(text As String, width As Integer)
        UDLV_插件列表.Columns.Add(New UltraDetailListView.ListColumn With {.Text = text, .Width = width})
    End Sub

    Private Sub Form_v6_插件管理_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        刷新列表(重新扫描:=False)
        调整列宽()
    End Sub

    Private Sub 插件列表变化(sender As Object, e As EventArgs)
        If 忽略管理器通知 OrElse IsDisposed Then Exit Sub
        If InvokeRequired Then
            BeginInvoke(Sub() 刷新列表(重新扫描:=False))
        Else
            刷新列表(重新扫描:=False)
        End If
    End Sub

    Private Sub 刷新列表(重新扫描 As Boolean)
        Dim selectedKey = 获取选中插件键()
        If 重新扫描 Then
            忽略管理器通知 = True
            Try
                插件管理.刷新插件目录()
            Finally
                忽略管理器通知 = False
            End Try
        End If

        Dim plugins = 插件管理.获取插件列表()
        快照.Clear()
        For Each plugin In plugins
            快照(plugin.插件键) = plugin
        Next
        更新概览(plugins)

        正在填充列表 = True
        ModernPanel1.SuspendLayout()
        UDLV_插件列表.BeginUpdate()
        Try
            UDLV_插件列表.Items.Clear()
            For index = 0 To plugins.Count - 1
                UDLV_插件列表.Items.Add(创建列表项(plugins(index), index + 1))
            Next

            Dim isEmpty = plugins.Count = 0
            UDLV_插件列表.Visible = Not isEmpty
            P_空状态.Visible = isEmpty
            If isEmpty Then
                UDLV_插件列表.SelectedIndex = -1
                P_空状态.BringToFront()
            Else
                Dim targetIndex = plugins.FindIndex(Function(item) String.Equals(item.插件键, selectedKey, StringComparison.OrdinalIgnoreCase))
                UDLV_插件列表.SelectedIndex = If(targetIndex >= 0, targetIndex, 0)
                UDLV_插件列表.BringToFront()
            End If
        Finally
            UDLV_插件列表.EndUpdate()
            正在填充列表 = False
            ModernPanel1.ResumeLayout(performLayout:=True)
        End Try
        显示选中插件详情()
        调整列宽()
    End Sub

    Private Sub 更新概览(plugins As IReadOnlyCollection(Of 插件信息_v6))
        Dim enabledCount = 0
        Dim pendingCount = 0
        For Each plugin In plugins
            If plugin.已启用 Then enabledCount += 1
            If plugin.等待重启 Then pendingCount += 1
        Next
        Dim restartText = If(
            pendingCount = 0,
            "<span style=""color:DarkGray"">无需重启</span>",
            $"<span style=""color:Goldenrod"">{pendingCount} 项待重启</span>")
        HCL_概览.Text = $"<span style=""color:Silver"">{plugins.Count} 个插件</span>   <span style=""color:YellowGreen"">{enabledCount} 个已启用</span>   {restartText}"
    End Sub

    Private Function 创建列表项(plugin As 插件信息_v6, displayOrder As Integer) As UltraDetailListView.ListItem
        Dim enabledText = If(plugin.已启用, "启用", "禁用")
        If plugin.等待重启 Then enabledText &= " *"
        Return New UltraDetailListView.ListItem(
            New UltraDetailListView.ListSubItem(displayOrder.ToString()),
            New UltraDetailListView.ListSubItem(enabledText),
            New UltraDetailListView.ListSubItem(空值替代(plugin.显示名称)),
            New UltraDetailListView.ListSubItem(接口类型文本(plugin.接口类型)),
            New UltraDetailListView.ListSubItem(空值替代(plugin.加载状态))
        ) With {.Tag = plugin.插件键}
    End Function

    Private Shared Function 空值替代(value As String) As String
        Return If(String.IsNullOrWhiteSpace(value), "-", value.Trim())
    End Function

    Private Shared Function 接口类型文本(value As 插件接口类型_v6) As String
        Select Case value
            Case 插件接口类型_v6.官方API
                Return "官方 API"
            Case 插件接口类型_v6.ExtAPI
                Return "Ext API"
            Case 插件接口类型_v6.官方与Ext
                Return "官方 + Ext"
            Case Else
                Return "未识别"
        End Select
    End Function

    Private Function 获取选中插件键() As String
        Dim item = UDLV_插件列表.SelectedItem
        Return If(TryCast(item?.Tag, String), "")
    End Function

    Private Function 获取选中插件() As 插件信息_v6
        Dim key = 获取选中插件键()
        Dim plugin As 插件信息_v6 = Nothing
        If key <> "" AndAlso 快照.TryGetValue(key, plugin) Then Return plugin
        Return Nothing
    End Function

    Private Sub 显示选中插件详情()
        Dim plugin = 获取选中插件()
        If plugin Is Nothing Then
            MB_切换启用.Text = "启用插件"
            MB_切换启用.ForeColor = Color.YellowGreen
            MB_切换启用.Enabled = False
            MB_上移.Enabled = False
            MB_下移.Enabled = False
            Dim emptyData As New 插件详情显示数据_v6 With {
                .状态文本 = If(快照.Count = 0, "暂无插件", "未选择"),
                .状态颜色 = Color.FromArgb(150, 175, 195),
                .名称 = If(快照.Count = 0, "插件目录为空", "请选择一个插件"),
                .文件名 = "-",
                .信息行 = 创建详情信息行("-", "-", "-", "-", "-", "-", "-"),
                .文件路径 = 插件管理.插件文件夹路径
            }
            Dim emptyStateConfigError = 插件管理.获取配置错误()
            If Not String.IsNullOrWhiteSpace(emptyStateConfigError) Then
                设置详情消息(emptyData, "配置异常", emptyStateConfigError, True)
            ElseIf 快照.Count = 0 Then
                设置详情消息(emptyData, "安装插件", "将 *.3fui.dll 拖放到本页，或放入左侧所示目录后刷新。", False)
            Else
                设置详情消息(emptyData, "使用提示", "从左侧选择插件后，可查看接口、版本和加载信息。", False)
            End If
            详情视图.显示详情(emptyData)
            Exit Sub
        End If

        MB_切换启用.Text = If(plugin.已启用, "停用插件", "启用插件")
        MB_切换启用.ForeColor = If(plugin.已启用, Color.IndianRed, Color.YellowGreen)
        MB_切换启用.Enabled = True
        MB_上移.Enabled = UDLV_插件列表.SelectedIndex > 0
        MB_下移.Enabled = UDLV_插件列表.SelectedIndex >= 0 AndAlso UDLV_插件列表.SelectedIndex < UDLV_插件列表.Items.Count - 1

        Dim usesExt = plugin.接口类型 = 插件接口类型_v6.ExtAPI OrElse plugin.接口类型 = 插件接口类型_v6.官方与Ext
        Dim pluginData As New 插件详情显示数据_v6 With {
            .状态文本 = If(plugin.等待重启, "等待重启", If(plugin.已启用, "已启用", "已停用")),
            .状态颜色 = If(plugin.等待重启, Color.FromArgb(235, 180, 105), If(plugin.已启用, Color.FromArgb(130, 210, 155), Color.FromArgb(165, 175, 185))),
            .名称 = 空值替代(plugin.显示名称),
            .文件名 = 空值替代(plugin.文件名),
            .信息行 = 创建详情信息行(
                接口类型文本(plugin.接口类型),
                空值替代(plugin.插件版本),
                空值替代(plugin.加载状态),
                If(usesExt, 空值替代(plugin.ExtSDK程序集版本), "不适用"),
                If(usesExt, 空值替代(plugin.ExtAPI最低版本), "不适用"),
                If(plugin.Ext插件标识.Count = 0, "-", String.Join("、", plugin.Ext插件标识)),
                空值替代(plugin.程序集版本)),
            .文件路径 = 空值替代(plugin.文件路径)
        }

        Dim errors As New List(Of String)
        If Not String.IsNullOrWhiteSpace(plugin.加载错误) Then errors.Add($"加载错误：{plugin.加载错误}")
        If Not String.IsNullOrWhiteSpace(plugin.元数据错误) Then errors.Add($"元数据错误：{plugin.元数据错误}")
        Dim configError = 插件管理.获取配置错误()
        If Not String.IsNullOrWhiteSpace(configError) Then errors.Add($"配置错误：{configError}")
        If errors.Count > 0 Then
            设置详情消息(pluginData, "需要处理", String.Join(Environment.NewLine, errors), True)
        ElseIf plugin.等待重启 Then
            Dim pendingMessage = If(plugin.有待安装更新, "插件更新已经暂存，重启应用后会在加载插件前完成替换。", "启用状态已经保存，重启应用后生效。")
            设置详情消息(pluginData, "等待应用", pendingMessage, False, warning:=True)
        Else
            设置详情消息(pluginData, "运行状态", "未发现加载错误。处理顺序的调整会立即用于下一次插件事件。", False)
        End If
        详情视图.显示详情(pluginData)
    End Sub

    Private Shared Function 创建详情信息行(interfaceType As String,
                                          pluginVersion As String,
                                          loadStatus As String,
                                          sdkVersion As String,
                                          apiVersion As String,
                                          pluginId As String,
                                          assemblyVersion As String) As IReadOnlyList(Of KeyValuePair(Of String, String))
        Return New List(Of KeyValuePair(Of String, String)) From {
            New KeyValuePair(Of String, String)("接口类型", interfaceType),
            New KeyValuePair(Of String, String)("插件版本", pluginVersion),
            New KeyValuePair(Of String, String)("加载状态", loadStatus),
            New KeyValuePair(Of String, String)("Ext SDK 引用", sdkVersion),
            New KeyValuePair(Of String, String)("最低 Ext API", apiVersion),
            New KeyValuePair(Of String, String)("Ext 插件 ID", pluginId),
            New KeyValuePair(Of String, String)("程序集版本", assemblyVersion)
        }
    End Function

    Private Shared Sub 设置详情消息(data As 插件详情显示数据_v6, title As String, message As String, isError As Boolean, Optional warning As Boolean = False)
        data.消息标题 = title
        data.消息内容 = message
        If isError Then
            data.消息标题颜色 = Color.IndianRed
            data.消息文字颜色 = Color.FromArgb(230, 195, 195)
            data.消息背景颜色 = Color.FromArgb(40, 205, 92, 92)
        ElseIf warning Then
            data.消息标题颜色 = Color.Goldenrod
            data.消息文字颜色 = Color.FromArgb(230, 220, 200)
            data.消息背景颜色 = Color.FromArgb(35, 218, 165, 32)
        Else
            data.消息标题颜色 = Color.FromArgb(140, 255, 255, 255)
            data.消息文字颜色 = Color.Silver
            data.消息背景颜色 = Color.FromArgb(20, 220, 220, 220)
        End If
    End Sub

    Private Sub MB_切换启用_Click(sender As Object, e As EventArgs) Handles MB_切换启用.Click
        Dim plugin = 获取选中插件()
        If plugin Is Nothing Then Exit Sub
        Try
            忽略管理器通知 = True
            Try
                插件管理.设置插件启用状态(plugin.插件键, Not plugin.已启用)
            Finally
                忽略管理器通知 = False
            End Try
            刷新列表(重新扫描:=False)
        Catch ex As Exception
            ExOverlayMsgBox(FormMain_v6, $"保存插件状态失败：{ex.Message}", MsgBoxStyle.Critical, "插件管理")
            Exit Sub
        End Try

        Dim result = ExOverlayMsgBox(
            FormMain_v6,
            "插件的启用状态已经保存。由于官方插件没有卸载协议，需重启程序才能安全生效。",
            {"立即重启", "稍后"},
            "需要重启",
            MsgBoxStyle.Question,
            1)
        If result = 0 Then FormMain_v6.请求重启应用()
    End Sub

    Private Sub MB_上移_Click(sender As Object, e As EventArgs) Handles MB_上移.Click
        移动选中项(-1)
    End Sub

    Private Sub MB_下移_Click(sender As Object, e As EventArgs) Handles MB_下移.Click
        移动选中项(1)
    End Sub

    Private Sub 移动选中项(direction As Integer)
        Dim index = UDLV_插件列表.SelectedIndex
        Dim target = index + direction
        If index < 0 OrElse target < 0 OrElse target >= UDLV_插件列表.Items.Count Then Exit Sub
        正在填充列表 = True
        Try
            Dim moving = UDLV_插件列表.Items(index)
            UDLV_插件列表.Items.RemoveAt(index)
            UDLV_插件列表.Items.Insert(target, moving)
            UDLV_插件列表.SelectedIndex = target
        Finally
            正在填充列表 = False
        End Try
        保存当前顺序()
    End Sub

    Private Sub UDLV_插件列表_ItemOrderChanged(sender As Object, e As EventArgs) Handles UDLV_插件列表.ItemOrderChanged
        If Not 正在填充列表 Then 保存当前顺序()
    End Sub

    Private Sub 保存当前顺序()
        Try
            Dim keys = UDLV_插件列表.Items.Select(Function(item) If(TryCast(item.Tag, String), "")).ToList()
            忽略管理器通知 = True
            Try
                插件管理.保存插件处理顺序(keys)
            Finally
                忽略管理器通知 = False
            End Try
            刷新列表(重新扫描:=False)
        Catch ex As Exception
            ExOverlayMsgBox(FormMain_v6, $"保存插件顺序失败：{ex.Message}", MsgBoxStyle.Critical, "插件管理")
        End Try
    End Sub

    Private Sub MB_刷新_Click(sender As Object, e As EventArgs) Handles MB_刷新.Click
        Try
            刷新列表(重新扫描:=True)
            ExFloatingTip(UDLV_插件列表, "插件目录已刷新；新加入的插件将在重启后加载", 1800)
        Catch ex As Exception
            ExOverlayMsgBox(FormMain_v6, $"刷新插件目录失败：{ex.Message}", MsgBoxStyle.Critical, "插件管理")
        End Try
    End Sub

    Private Sub MB_打开目录_Click(sender As Object, e As EventArgs) Handles MB_打开目录.Click, MB_空状态打开目录.Click
        Try
            Directory.CreateDirectory(插件管理.插件文件夹路径)
            Dim startInfo As New ProcessStartInfo With {.FileName = "explorer.exe", .UseShellExecute = True}
            startInfo.ArgumentList.Add(插件管理.插件文件夹路径)
            Process.Start(startInfo)
        Catch ex As Exception
            ExOverlayMsgBox(FormMain_v6, $"打开插件目录失败：{ex.Message}", MsgBoxStyle.Critical, "插件管理")
        End Try
    End Sub

    Private Sub MB_重启应用_Click(sender As Object, e As EventArgs) Handles MB_重启应用.Click
        Dim result = ExOverlayMsgBox(FormMain_v6, "确定要重启 FFmpegFreeUI 以应用插件变更吗？", {"重启", "取消"}, "重启应用", MsgBoxStyle.Question, 1)
        If result = 0 Then FormMain_v6.请求重启应用()
    End Sub

    Private Sub UDLV_插件列表_SelectedIndexChanged(sender As Object, e As EventArgs) Handles UDLV_插件列表.SelectedIndexChanged
        If Not 正在填充列表 Then 显示选中插件详情()
    End Sub

    Private Sub 注册插件拖放目标(target As Control)
        target.AllowDrop = True
        AddHandler target.DragEnter, AddressOf 插件拖放_DragEnter
        AddHandler target.DragDrop, AddressOf 插件拖放_DragDrop
        For Each child As Control In target.Controls
            注册插件拖放目标(child)
        Next
    End Sub

    Private Sub 插件拖放_DragEnter(sender As Object, e As DragEventArgs)
        Dim files = 获取拖放文件(e.Data)
        If files.Any(Function(file) 插件管理.是插件程序集文件(file)) Then e.Effect = DragDropEffects.Copy
    End Sub

    Private Sub 插件拖放_DragDrop(sender As Object, e As DragEventArgs)
        Dim files = 获取拖放文件(e.Data)
        If Not files.Any(Function(file) 插件管理.是插件程序集文件(file)) Then Exit Sub

        Dim existing = files.
            Where(Function(file) 插件管理.是插件程序集文件(file) AndAlso 是不同位置的已安装文件(file)).
            Select(Function(file) Path.GetFileName(file)).
            Distinct(StringComparer.OrdinalIgnoreCase).
            ToList()
        Dim replaceExisting = False
        If existing.Count > 0 Then
            Dim choice = ExOverlayMsgBox(
                FormMain_v6,
                $"插件目录中已有同名文件：{文件名摘要(existing)}{Environment.NewLine}已加载的插件会先暂存更新，并在重启时安全替换。",
                {"替换已有插件", "仅安装新插件", "取消"},
                "安装插件",
                MsgBoxStyle.Question,
                2)
            If choice = 2 OrElse choice < 0 Then Exit Sub
            replaceExisting = choice = 0
        End If

        Dim installResult As 插件安装结果_v6
        Try
            installResult = 插件管理.安装插件文件(files, replaceExisting)
            If installResult.有变更 Then 刷新列表(重新扫描:=True)
        Catch ex As Exception
            ExOverlayMsgBox(FormMain_v6, $"安装插件失败：{ex.Message}", MsgBoxStyle.Critical, "插件管理")
            Exit Sub
        End Try

        Dim summary = 创建安装结果摘要(installResult)
        If installResult.有变更 Then
            Dim choice = ExOverlayMsgBox(
                FormMain_v6,
                summary & Environment.NewLine & Environment.NewLine & "插件需要重启后才会加载，是否立即重启？",
                {"立即重启", "稍后"},
                "插件已安装",
                If(installResult.错误.Count > 0, MsgBoxStyle.Exclamation, MsgBoxStyle.Question),
                1)
            If choice = 0 Then FormMain_v6.请求重启应用()
        Else
            ExOverlayMsgBox(FormMain_v6, summary, If(installResult.错误.Count > 0, MsgBoxStyle.Critical, MsgBoxStyle.Information), "未安装插件")
        End If
    End Sub

    Private Shared Function 获取拖放文件(data As IDataObject) As List(Of String)
        If data Is Nothing OrElse Not data.GetDataPresent(DataFormats.FileDrop) Then Return New List(Of String)
        Dim values = TryCast(data.GetData(DataFormats.FileDrop), String())
        If values Is Nothing Then Return New List(Of String)
        Return values.Where(Function(value) Not String.IsNullOrWhiteSpace(value)).ToList()
    End Function

    Private Shared Function 是不同位置的已安装文件(source As String) As Boolean
        Try
            Dim target = Path.Combine(插件管理.插件文件夹路径, Path.GetFileName(source))
            Return File.Exists(target) AndAlso Not String.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)
        Catch
            Return False
        End Try
    End Function

    Private Shared Function 创建安装结果摘要(result As 插件安装结果_v6) As String
        Dim parts As New List(Of String)
        If result.已安装文件.Count > 0 Then parts.Add($"已复制 {result.已安装文件.Count} 个新插件：{文件名摘要(result.已安装文件)}")
        If result.待重启替换文件.Count > 0 Then parts.Add($"已暂存 {result.待重启替换文件.Count} 个插件更新：{文件名摘要(result.待重启替换文件)}")
        If result.已跳过文件.Count > 0 Then parts.Add($"已跳过 {result.已跳过文件.Count} 个已有文件：{文件名摘要(result.已跳过文件)}")
        If result.无效文件.Count > 0 Then parts.Add($"不是 *.3fui.dll，已忽略：{文件名摘要(result.无效文件)}")
        If result.错误.Count > 0 Then parts.Add("失败：" & 文件名摘要(result.错误))
        If parts.Count = 0 Then parts.Add("拖放内容中没有可安装的插件文件。")
        Return String.Join(Environment.NewLine, parts)
    End Function

    Private Shared Function 文件名摘要(files As IEnumerable(Of String)) As String
        Dim values = files.Where(Function(value) Not String.IsNullOrWhiteSpace(value)).Take(4).ToList()
        Dim total = files.Count(Function(value) Not String.IsNullOrWhiteSpace(value))
        Dim text = String.Join("、", values)
        If total > values.Count Then text &= $" 等 {total} 个"
        Return text
    End Function

    Private Sub UDLV_插件列表_SizeChanged(sender As Object, e As EventArgs) Handles UDLV_插件列表.SizeChanged
        调整列宽()
    End Sub

    Private Sub 调整列宽()
        If UDLV_插件列表.Columns.Count < 5 OrElse UDLV_插件列表.ClientSize.Width <= 0 Then Exit Sub
        Dim fixedWidth = 70 + 80 + 115 + 180
        Dim available = UDLV_插件列表.ClientSize.Width - UDLV_插件列表.Padding.Left - UDLV_插件列表.Padding.Right - 50
        UDLV_插件列表.Columns(2).Width = Math.Max(160, available - fixedWidth)
    End Sub
End Class
