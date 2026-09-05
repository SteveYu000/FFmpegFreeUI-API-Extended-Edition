Imports System.Diagnostics
Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports LakeUI

Public Class Form_v6_插件管理
    Private Const 页面标题文本 As String = "<span style=""font-size:13; color:Silver"">插件管理</span>   查看插件状态、接口兼容性和事件处理顺序"
    Private Const 设置页面标题文本 As String = "<span style=""font-size:13; color:Silver"">插件管理</span>   插件设置"

    Private ReadOnly 快照 As New Dictionary(Of String, 插件信息_v6)(StringComparer.OrdinalIgnoreCase)
    Private 当前插件设置页 As Control
    Private 当前设置页插件标识 As String = ""
    Private 正在填充列表 As Boolean
    Private 忽略管理器通知 As Boolean
    Private 已订阅插件列表变化 As Boolean

    Public Sub New()
        InitializeComponent()
        配置LakeUI5渲染结构()
        If 是设计器宿主() Then
            初始化设计器预览()
            Return
        End If

        L_插件目录路径.Text = 插件管理.插件文件夹路径
        注册插件拖放目标(Me)
        AddHandler 插件管理.插件列表已变化, AddressOf 插件列表变化
        已订阅插件列表变化 = True
    End Sub

    ''' <summary>
    ''' LakeUI 5 的控件各自拥有 HWND 与交换链。这里把插件管理页的容器全部接入同一条
    ''' GPU 背景依赖链，并使用单一的绝对布局所有者，避免 WinForms 透明布局容器与
    ''' ModernPanel 的 presenter 同时争用子控件几何。
    ''' </summary>
    Private Sub 配置LakeUI5渲染结构()
        插件设置布局.BackgroundSource = ModernPanel1
        P_插件设置标题栏.BackgroundSource = 插件设置布局
        P_插件设置内容.BackgroundSource = 插件设置布局
        MB_返回插件详情.BackgroundSource = P_插件设置标题栏
        HCL_插件设置标题.BackgroundSource = P_插件设置标题栏

        管理工具栏.BackgroundSource = ModernPanel1
        FLP_管理操作.BackgroundSource = 管理工具栏
        HCL_概览.BackgroundSource = 管理工具栏
        For Each button In {MB_打开目录, MB_刷新, MB_切换启用, MB_上移, MB_下移, MB_重启应用}
            button.BackgroundSource = FLP_管理操作
        Next

        管理内容布局.BackgroundSource = ModernPanel1
        P_插件列表区域.BackgroundSource = 管理内容布局
        UDLV_插件列表.BackgroundSource = P_插件列表区域
        P_空状态.BackgroundSource = P_插件列表区域
        TLP_空状态.BackgroundSource = P_空状态
        L_空状态标题.BackgroundSource = TLP_空状态
        L_空状态说明.BackgroundSource = TLP_空状态
        L_插件目录路径.BackgroundSource = TLP_空状态
        MB_空状态打开目录.BackgroundSource = TLP_空状态

        MP_插件详情.BackgroundSource = 管理内容布局
        详情视图.BackgroundSource = MP_插件详情
        HCL_说明.BackgroundSource = ModernPanel1
        HCL_页面标题.BackgroundSource = ModernPanel1

        AddHandler 管理内容布局.SizeChanged, AddressOf 页面布局尺寸变化
        AddHandler 管理工具栏.SizeChanged, AddressOf 页面布局尺寸变化
        AddHandler TLP_空状态.SizeChanged, AddressOf 页面布局尺寸变化
        AddHandler Me.DpiChangedAfterParent, AddressOf 页面Dpi变化
        更新页面布局()
    End Sub

    Private Sub 页面布局尺寸变化(sender As Object, e As EventArgs)
        更新页面布局()
    End Sub

    Private Sub 页面Dpi变化(sender As Object, e As DpiChangedEventArgs)
        更新页面布局()
    End Sub

    Private Sub 更新页面布局()
        更新管理双栏布局()
        更新管理工具栏布局()
        更新空状态布局()
    End Sub

    Private Sub 更新管理双栏布局()
        Dim width = 管理内容布局.ClientSize.Width
        Dim height = 管理内容布局.ClientSize.Height
        If width <= 0 OrElse height <= 0 Then Return

        Dim gap = 缩放界面值(10)
        Dim minimumListWidth = 缩放界面值(470)
        Dim minimumDetailWidth = 缩放界面值(280)
        Dim preferredDetailWidth = Math.Max(minimumDetailWidth, CInt(Math.Round(width * 0.31R)))
        Dim detailWidth = Math.Min(preferredDetailWidth, Math.Max(minimumDetailWidth, width - gap - minimumListWidth))
        detailWidth = Math.Min(detailWidth, Math.Max(1, width - gap - 1))
        Dim listWidth = Math.Max(1, width - gap - detailWidth)

        P_插件列表区域.SetBounds(0, 0, listWidth, height)
        MP_插件详情.SetBounds(listWidth + gap, 0, detailWidth, height)
    End Sub

    Private Sub 更新管理工具栏布局()
        Dim clientWidth = 管理工具栏.ClientSize.Width
        Dim clientHeight = 管理工具栏.ClientSize.Height
        If clientWidth <= 0 OrElse clientHeight <= 0 Then Return

        Dim gap = 缩放界面值(10)
        Dim x = 0
        Dim buttons = {MB_打开目录, MB_刷新, MB_切换启用, MB_上移, MB_下移, MB_重启应用}
        For Each button In buttons
            button.Location = New Point(x, 0)
            x += button.Width + gap
        Next
        If buttons.Length > 0 Then x -= gap

        FLP_管理操作.SetBounds(0, 管理工具栏.Padding.Top, Math.Max(1, x), Math.Max(1, clientHeight - 管理工具栏.Padding.Vertical))
        Dim overviewLeft = Math.Min(clientWidth, x + gap)
        HCL_概览.SetBounds(overviewLeft,
                          管理工具栏.Padding.Top,
                          Math.Max(1, clientWidth - overviewLeft),
                          Math.Max(1, clientHeight - 管理工具栏.Padding.Vertical))
    End Sub

    Private Sub 更新空状态布局()
        Dim width = TLP_空状态.ClientSize.Width
        Dim height = TLP_空状态.ClientSize.Height
        If width <= 0 OrElse height <= 0 Then Return

        Dim titleHeight = 缩放界面值(44)
        Dim descriptionHeight = 缩放界面值(38)
        Dim pathHeight = 缩放界面值(54)
        Dim buttonHeight = MB_空状态打开目录.Height
        Dim gap = 缩放界面值(5)
        Dim totalHeight = titleHeight + descriptionHeight + pathHeight + buttonHeight + gap
        Dim y = Math.Max(0, (height - totalHeight) \ 2)

        L_空状态标题.SetBounds(0, y, width, titleHeight)
        y += titleHeight
        L_空状态说明.SetBounds(0, y, width, descriptionHeight)
        y += descriptionHeight
        L_插件目录路径.SetBounds(0, y, width, pathHeight)
        y += pathHeight + gap
        MB_空状态打开目录.Location = New Point(Math.Max(0, (width - MB_空状态打开目录.Width) \ 2), y)
    End Sub

    Private Function 缩放界面值(value As Integer) As Integer
        Return Math.Max(1, CInt(Math.Round(value * CDbl(DeviceDpi) / 96.0R)))
    End Function

    Private Shared Function 是设计器宿主() As Boolean
        If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then Return True

        Dim processName = Process.GetCurrentProcess().ProcessName
        Return processName.Equals("devenv", StringComparison.OrdinalIgnoreCase) OrElse
               processName.StartsWith("DesignToolsServer", StringComparison.OrdinalIgnoreCase) OrElse
               processName.StartsWith("XDesProc", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub 初始化设计器预览()
        L_插件目录路径.Text = "<程序目录>\Plugin"
        HCL_概览.Text = "<span style=""color:Silver"">0 个插件</span>   <span style=""color:YellowGreen"">0 个已启用</span>   <span style=""color:DarkGray"">设计器预览</span>"
    End Sub

    Private Sub 释放运行时资源()
        关闭当前插件设置页()
        If 已订阅插件列表变化 Then
            RemoveHandler 插件管理.插件列表已变化, AddressOf 插件列表变化
            已订阅插件列表变化 = False
        End If
    End Sub

    Private Sub Form_v6_插件管理_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If 是设计器宿主() Then Exit Sub
        刷新列表(重新扫描:=False)
        调整列宽()
    End Sub

    Friend Sub 提交切页首帧()
        If Not Visible OrElse IsDisposed OrElse 是设计器宿主() Then Return
        更新页面布局()
        ' 页面切换是明确的同步视觉边界。LakeUI 5 要求父 surface 先于子 surface 提交；
        ' 由主导航完成 BoundControl 切换后，再一次性按外到内提交，避免已显示但尚未
        ' 提交的独立 HWND 短暂呈现黑色。
        OuterToInnerRefreshScheduler.RequestFull(ModernPanel1, invalidateChildren:=True, immediate:=True)
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
        Dim detailPlugin As 插件信息_v6 = Nothing
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
                targetIndex = If(targetIndex >= 0, targetIndex, 0)
                UDLV_插件列表.SelectedIndex = targetIndex
                detailPlugin = plugins(targetIndex)
                UDLV_插件列表.BringToFront()
            End If
        Finally
            UDLV_插件列表.EndUpdate()
            正在填充列表 = False
            ModernPanel1.ResumeLayout(performLayout:=True)
        End Try
        If 插件设置布局 IsNot Nothing AndAlso 插件设置布局.Visible AndAlso 当前设置页插件标识 <> "" AndAlso
           Not plugins.Any(Function(item) item.Ext设置页插件标识.Contains(当前设置页插件标识, StringComparer.OrdinalIgnoreCase)) Then
            显示插件管理视图()
        ElseIf 插件设置布局 Is Nothing OrElse Not 插件设置布局.Visible Then
            显示选中插件详情(detailPlugin)
        End If
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

    Private Sub 显示选中插件详情(Optional preferredPlugin As 插件信息_v6 = Nothing)
        Dim plugin = If(preferredPlugin, 获取选中插件())
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
            .文件路径 = 空值替代(plugin.文件路径),
            .设置入口可用 = plugin.已加载 AndAlso plugin.Ext设置页插件标识.Count > 0
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

    Private Sub 详情视图_设置入口点击(sender As Object, e As EventArgs) Handles 详情视图.设置入口点击
        Dim plugin = 获取选中插件()
        If plugin Is Nothing OrElse Not plugin.已加载 Then Exit Sub
        Dim pluginId = plugin.Ext设置页插件标识.FirstOrDefault()
        If String.IsNullOrWhiteSpace(pluginId) Then Exit Sub

        Dim page As Control = Nothing
        Try
            page = 插件管理.创建Ext插件设置页(pluginId)
            显示插件设置页(plugin, pluginId, page)
        Catch ex As Exception
            page?.Dispose()
            ExOverlayMsgBox(FormMain_v6, $"打开插件设置失败：{ex.Message}", MsgBoxStyle.Critical, "插件设置")
        End Try
    End Sub

    Private Sub 显示插件设置页(plugin As 插件信息_v6, pluginId As String, page As Control)
        If plugin Is Nothing Then Throw New ArgumentNullException(NameOf(plugin))
        If page Is Nothing OrElse page.IsDisposed Then Throw New ArgumentException("插件设置页无效", NameOf(page))

        关闭当前插件设置页()
        page.Dock = DockStyle.Fill
        page.Margin = Padding.Empty
        P_插件设置内容.Controls.Add(page)
        当前插件设置页 = page
        当前设置页插件标识 = pluginId

        Dim displayName = WebUtility.HtmlEncode(空值替代(plugin.显示名称))
        HCL_插件设置标题.Text = $"<span style=""font-size:12; color:Silver"">{displayName}</span>   插件设置"
        HCL_页面标题.Text = 设置页面标题文本
        管理工具栏.Visible = False
        管理内容布局.Visible = False
        HCL_说明.Visible = False
        插件设置布局.Visible = True
        插件设置布局.BringToFront()
        page.Focus()
    End Sub

    Private Sub MB_返回插件详情_Click(sender As Object, e As EventArgs) Handles MB_返回插件详情.Click
        显示插件管理视图()
    End Sub

    Private Sub 显示插件管理视图()
        关闭当前插件设置页()
        If 插件设置布局 IsNot Nothing Then 插件设置布局.Visible = False
        If 管理内容布局 IsNot Nothing Then
            管理内容布局.Visible = True
            管理内容布局.BringToFront()
        End If
        If 管理工具栏 IsNot Nothing Then 管理工具栏.Visible = True
        HCL_说明.Visible = True
        HCL_页面标题.Text = 页面标题文本
        显示选中插件详情()
    End Sub

    Private Sub 关闭当前插件设置页()
        Dim page = 当前插件设置页
        当前插件设置页 = Nothing
        当前设置页插件标识 = ""
        If page Is Nothing Then Exit Sub
        If page.Parent Is P_插件设置内容 Then P_插件设置内容.Controls.Remove(page)
        If Not page.IsDisposed Then page.Dispose()
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
            "插件的启用状态已经保存，需重启程序才能生效。",
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
