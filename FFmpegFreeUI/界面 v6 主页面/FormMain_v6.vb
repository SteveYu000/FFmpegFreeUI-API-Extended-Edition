Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports LakeUI

Public Class FormMain_v6
    Private ReadOnly 插件选项卡页 As New Dictionary(Of String, ModernTabListControl.ModernTabPage)(StringComparer.CurrentCultureIgnoreCase)
    Private 插件主导航目标已注册 As Boolean
    Private 退出确认已完成 As Boolean = False
    Private 退出时清除所有任务 As Boolean = True
    Private 退出里程碑检查进行中 As Boolean = False
    Private 退出里程碑检查已完成 As Boolean = False
    Private 重启请求待执行 As Boolean = False
    Private 重启助手已启动 As Boolean = False

    Private Sub FormMain_v6_Load(sender As Object, e As EventArgs) Handles Me.Load
        UI同步上下文 = Threading.SynchronizationContext.Current
        设置_v6.启动时读取SP解锁器()
        设置_v6.启动时加载设置()
        界面主题_v6.初始化()
        界面主题_v6.应用窗口圆角设置()
        网络功能.启动时后台获取SPAgent端点()

        设置_v6.加载SP自定义图标()
        设置_v6.加载SP自定义起始页顶栏背景图()
        设置_v6.加载SP自定义背景图()

        Me.ModernTabListControl1.Items(1).BoundControl = Form_v6_起始页面
        绑定选项卡(Form_v6_起始页面.ModernPanel1)
        Me.ModernTabListControl1.Items(2).BoundControl = Form_v6_编码队列
        绑定选项卡(Form_v6_编码队列.ModernPanel1)
        Me.ModernTabListControl1.Items(4).BoundControl = Form_v6_准备文件
        绑定选项卡(Form_v6_准备文件.ModernPanel1)
        Me.ModernTabListControl1.Items(5).BoundControl = Form_v6_参数面板
        绑定选项卡(Form_v6_参数面板.ModernPanel1)
        Me.ModernTabListControl1.Items(6).BoundControl = Form_v6_Agent
        绑定选项卡(Form_v6_Agent.ModernPanel1)
        'Me.ModernTabListControl1.Items(7).BoundControl = Form_v6_社区_个人中心
        '绑定选项卡(Form_v6_社区_个人中心.ModernPanel1)
        Me.ModernTabListControl1.Items(9).BoundControl = Form_v6_媒体信息
        绑定选项卡(Form_v6_媒体信息.ModernPanel1)
        Me.ModernTabListControl1.Items(10).BoundControl = Form_v6_调试播放器
        绑定选项卡(Form_v6_调试播放器.ModernPanel1)
        Me.ModernTabListControl1.Items(11).BoundControl = Form_v6_性能监控
        绑定选项卡(Form_v6_性能监控.ModernPanel1)
        Me.ModernTabListControl1.Items(12).BoundControl = Form_v6_集成工具
        绑定选项卡(Form_v6_集成工具.ModernPanel1)
        Me.ModernTabListControl1.Items(14).BoundControl = Form_v6_设置
        绑定选项卡(Form_v6_设置.ModernPanel1)
        Me.ModernTabListControl1.Items(15).BoundControl = Form_v6_支持者
        绑定选项卡(Form_v6_支持者.ModernPanel1)
        Me.ModernTabListControl1.Items(17).BoundControl = Form_v6_插件管理
        绑定选项卡(Form_v6_插件管理.ModernPanel1)

        Select Case 设置_v6.实例对象.窗口样式
            Case 1
                DwmWindowStyle.SetDarkMode(Me.Handle, True)
            Case 2
                Me.ThisIsYourWindow1.Attach(Me)
                If Not SP_UnLock Then Exit Select
                Select Case 设置_v6.实例对象.SP_毛玻璃模式
                    Case > 0
                        ModernTabListControl1.TabStripBackColor = Color.Transparent
                        ModernTabListControl1.ContentBackColor = Color.Transparent
                        Form_v6_参数面板.ModernTabListControl1.TabStripBackColor = Color.Transparent
                        Form_v6_参数面板.ModernTabListControl1.ContentBackColor = Color.Transparent
                        Form_v6_集成工具.ModernTabListControl1.TabStripBackColor = Color.Transparent
                        Form_v6_集成工具.ModernTabListControl1.ContentBackColor = Color.Transparent
                        Form_v6_设置.ModernTabListControl1.TabStripBackColor = Color.Transparent
                        Form_v6_设置.ModernTabListControl1.ContentBackColor = Color.Transparent
                        Form_v6_参数面板.私有界面_自定义参数.ModernTabControl1.TabStripBackColor = Color.Transparent
                        Form_v6_参数面板.私有界面_自定义参数.ModernTabControl1.ContentBackColor = Color.Transparent
                        Form_v6_参数面板.私有界面_附加内容.ModernTabControl1.TabStripBackColor = Color.Transparent
                        Form_v6_参数面板.私有界面_附加内容.ModernTabControl1.ContentBackColor = Color.Transparent

                        Form_v6_起始页面.ModernPanel1.Padding = New Padding(10 * DeviceDpi / 96, 10 * DeviceDpi / 96, Form_v6_起始页面.ModernPanel1.Padding.Right, Form_v6_起始页面.ModernPanel1.Padding.Bottom)
                        Form_v6_准备文件.ModernPanel1.Padding = New Padding(10 * DeviceDpi / 96, 10 * DeviceDpi / 96, Form_v6_准备文件.ModernPanel1.Padding.Right, Form_v6_准备文件.ModernPanel1.Padding.Bottom)

                        Form_v6_社区_个人中心.ModernPanel1.Padding = New Padding(10 * DeviceDpi / 96, 10 * DeviceDpi / 96, Form_v6_社区_个人中心.ModernPanel1.Padding.Right, Form_v6_社区_个人中心.ModernPanel1.Padding.Bottom)

                        Form_v6_媒体信息.ModernPanel1.Padding = New Padding(10 * DeviceDpi / 96, 10 * DeviceDpi / 96, Form_v6_媒体信息.ModernPanel1.Padding.Right, Form_v6_媒体信息.ModernPanel1.Padding.Bottom)
                        Form_v6_调试播放器.ModernPanel1.Padding = New Padding(10 * DeviceDpi / 96, 10 * DeviceDpi / 96, Form_v6_调试播放器.ModernPanel1.Padding.Right, Form_v6_调试播放器.ModernPanel1.Padding.Bottom)
                End Select
        End Select

        Me.ModernTabListControl1.SelectedIndex = 1
        Me.ModernTextBox1.Parent = Me.ModernTabListControl1

        其他初始化.执行()

        确保注册插件主导航目标()
        Form_v6_参数面板.确保注册插件参数面板目录()
        Form_v6_编码队列.确保注册插件工具栏目标()
        插件管理.启动时加载插件()
        If 设置_v6.实例对象.是否监听端口 Then 端口监听_v6.启动客户端()

    End Sub

    Private Sub FormMain_v6_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.ModernTabListControl1.Focus()
        Application.DoEvents()
        启动参数响应_v6.处理首次启动参数()

        网络功能.启动时检查新版本()
        网络功能.获取新闻列表()

        If 设置_v6.实例对象.启用性能计数器 = 0 Then
            MainAppUsageCounter.Start()
            PrecisionTimer1.Start()
        End If

        检查并询问加载未处理任务缓存()
        用户使用统计_v6.启动时后台检查(Me)
    End Sub

    Sub 绑定选项卡(选项卡的根面板容器 As ModernPanel)
        绑定选项卡核心(选项卡的根面板容器)
    End Sub

    Public Sub 请求重启应用()
        If IsDisposed OrElse Disposing OrElse 重启助手已启动 Then Exit Sub
        If Not 设置_v6.实例对象.插件管理_PowerShell重启说明已显示 Then
            Dim result = ExOverlayMsgBox(
                Me,
                "为确保单实例程序能够可靠重启，FFmpegFreeUI 会在后台以隐藏窗口方式运行 Windows PowerShell，等待当前进程完全退出后再启动新实例。" &
                Environment.NewLine & Environment.NewLine &
                "部分杀毒软件可能会对隐藏运行 PowerShell 这一行为弹出警告或误报，这是正常现象。该重启过程不会下载或运行外部脚本。",
                {"了解并继续", "取消重启"},
                "首次使用重启功能",
                MsgBoxStyle.Information,
                1)
            If result <> 0 Then Exit Sub

            设置_v6.实例对象.插件管理_PowerShell重启说明已显示 = True
            Try
                设置_v6.后台保存设置()
            Catch
                ' 正常退出时还会再次保存；提示记录写入失败不应阻止本次重启。
            End Try
        End If
        重启请求待执行 = True
        Close()
    End Sub

    Public Sub 添加插件选项卡(选项卡标题 As String, 面板 As Control)
        添加插件选项卡核心(选项卡标题, 面板)
    End Sub

    Public Sub 配置插件页面背景(页面 As Control)
        配置插件页面背景核心(页面)
    End Sub

    Private Sub 确保注册插件主导航目标()
        If 插件主导航目标已注册 Then Exit Sub
        插件主导航目标已注册 = True

        Dim targets As (Id As String, DisplayName As String, Index As Integer)() = {
            (Ext插件页面目标_v2.主导航_起始页面, "起始页面", 1),
            (Ext插件页面目标_v2.主导航_编码队列, "编码队列", 2),
            (Ext插件页面目标_v2.主导航_准备文件, "准备文件", 4),
            (Ext插件页面目标_v2.主导航_参数面板, "参数面板", 5),
            (Ext插件页面目标_v2.主导航_Agent, "Agent 智能体", 6),
            (Ext插件页面目标_v2.主导航_Studios, "3FUI Studios", 7),
            (Ext插件页面目标_v2.主导航_媒体信息, "ffprobe 媒体信息", 9),
            (Ext插件页面目标_v2.主导航_调试播放器, "ffplay 调试播放器", 10),
            (Ext插件页面目标_v2.主导航_性能监控, "性能监控", 11),
            (Ext插件页面目标_v2.主导航_集成工具, "集成工具", 12),
            (Ext插件页面目标_v2.主导航_软件设置, "软件设置", 14),
            (Ext插件页面目标_v2.主导航_支持者, "支持者", 15),
            (Ext插件页面目标_v2.主导航_插件管理, "插件管理", 17)
        }
        For Each target In targets
            Ext插件扩展桥接_v2.注册插件页面目标(
                target.Id,
                target.DisplayName,
                "main",
                ModernTabListControl1,
                ModernTabListControl1.Items(target.Index),
                configurePage:=AddressOf 配置插件页面背景)
        Next
    End Sub

    Private Function 获取插件选项卡插入位置() As Integer
        ' 官方插件入口位于侧栏末尾；插件管理是该区域的固定首项，后注册的插件依次追加。
        Return ModernTabListControl1.Items.Count
    End Function

    Private Function 查找可绑定背景映射的插件ModernPanel(根控件 As Control) As ModernPanel
        If 根控件 Is Nothing Then Return Nothing

        Dim 根控件类型 As Type = 根控件.GetType()
        While 根控件类型 IsNot Nothing
            Dim 字段 = 根控件类型.GetField("ModernPanel1", BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
            If 字段 IsNot Nothing Then
                Dim 面板 = TryCast(字段.GetValue(根控件), ModernPanel)
                If 插件ModernPanel可绑定背景映射(面板) Then Return 面板
            End If

            Dim 属性 = 根控件类型.GetProperty("ModernPanel1", BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
            If 属性 IsNot Nothing Then
                Dim 面板 = TryCast(属性.GetValue(根控件), ModernPanel)
                If 插件ModernPanel可绑定背景映射(面板) Then Return 面板
            End If

            根控件类型 = 根控件类型.BaseType
        End While

        Return 查找子控件中的插件ModernPanel(根控件)
    End Function

    Private Function 查找子控件中的插件ModernPanel(控件 As Control) As ModernPanel
        If 控件 Is Nothing Then Return Nothing

        Dim 面板 = TryCast(控件, ModernPanel)
        If 插件ModernPanel可绑定背景映射(面板) Then Return 面板

        For Each 子控件 As Control In 控件.Controls
            Dim 子面板 = 查找子控件中的插件ModernPanel(子控件)
            If 子面板 IsNot Nothing Then Return 子面板
        Next

        Return Nothing
    End Function

    Private Function 插件ModernPanel可绑定背景映射(面板 As ModernPanel) As Boolean
        Return 面板 IsNot Nothing AndAlso
               String.Equals(面板.Name, "ModernPanel1", StringComparison.Ordinal) AndAlso
               面板.Dock = DockStyle.Fill
    End Function

    Private Sub ModernTabListControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ModernTabListControl1.SelectedIndexChanged
        Dim selectedControl As Control = Nothing
        If ModernTabListControl1.SelectedIndex >= 0 AndAlso
           ModernTabListControl1.SelectedIndex < ModernTabListControl1.Items.Count Then
            selectedControl = ModernTabListControl1.Items(ModernTabListControl1.SelectedIndex).BoundControl
        End If

        If selectedControl Is Form_v6_Agent Then
            Form_v6_性能监控.停止()
            Form_v6_Agent.检查并刷新模型列表()
        ElseIf selectedControl Is Form_v6_性能监控 Then
            Form_v6_性能监控.开始()
        Else
            Form_v6_性能监控.停止()
        End If

        If selectedControl Is Form_v6_插件管理 Then Form_v6_插件管理.提交切页首帧()
    End Sub

    <CodeAnalysis.SuppressMessage("Performance", "CA1861:不要将常量数组作为参数", Justification:="<挂起>")>
    Private Async Sub FormMain_v6_Closing(sender As Object, e As CancelEventArgs) Handles Me.FormClosing
        Await 执行关闭流程Async(e)
    End Sub

    Private Shared Sub 启动重启助手()
        Dim executablePath = System.Windows.Forms.Application.ExecutablePath
        If String.IsNullOrWhiteSpace(executablePath) OrElse Not File.Exists(executablePath) Then
            Throw New FileNotFoundException("找不到当前程序文件", executablePath)
        End If

        Dim command =
            $"$p = Get-Process -Id {Environment.ProcessId} -ErrorAction SilentlyContinue; " &
            "if ($null -ne $p) { $p.WaitForExit() }; " &
            $"Start-Process -FilePath {转换为PowerShell字符串(executablePath)} -WorkingDirectory {转换为PowerShell字符串(System.Windows.Forms.Application.StartupPath)}"
        Dim encodedCommand = Convert.ToBase64String(Encoding.Unicode.GetBytes(command))
        Dim powershellPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell\v1.0\powershell.exe")
        If Not File.Exists(powershellPath) Then powershellPath = "powershell.exe"

        Dim startInfo As New ProcessStartInfo With {
            .FileName = powershellPath,
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .WindowStyle = ProcessWindowStyle.Hidden
        }
        startInfo.ArgumentList.Add("-NoLogo")
        startInfo.ArgumentList.Add("-NoProfile")
        startInfo.ArgumentList.Add("-NonInteractive")
        startInfo.ArgumentList.Add("-WindowStyle")
        startInfo.ArgumentList.Add("Hidden")
        startInfo.ArgumentList.Add("-EncodedCommand")
        startInfo.ArgumentList.Add(encodedCommand)
        Dim helper = Process.Start(startInfo)
        If helper Is Nothing Then Throw New InvalidOperationException("重启助手进程未能启动")
        helper.Dispose()
    End Sub

    Private Shared Function 转换为PowerShell字符串(value As String) As String
        Return "'" & If(value, "").Replace("'", "''") & "'"
    End Function

    <CodeAnalysis.SuppressMessage("Performance", "CA1861:不要将常量数组作为参数", Justification:="<挂起>")>
    Private Sub 检查并询问加载未处理任务缓存()
        检查并询问加载未处理任务缓存核心()
    End Sub

    Private Sub PrecisionTimer1_Tick(sender As Object, e As EventArgs) Handles PrecisionTimer1.Tick
        Dim t1 As String = "<Title>"
        t1 &= $"   |   CPU {MainAppUsageCounter.GetCpuUsagePercent():F1}%"
        t1 &= $"   |   RAM {MainAppUsageCounter.GetActivePrivateWorkingSetBytes() / 1024 / 1024:F0}M / {MainAppUsageCounter.GetCommitSizeBytes() / 1024 / 1024:F0}M"
        t1 &= $"   |   GPU {MainAppUsageCounter.GetGpuUsagePercent():F1}% {MainAppUsageCounter.GetGpuDedicatedMemoryBytes() / 1024 / 1024:F0}M + {MainAppUsageCounter.GetGpuSharedMemoryBytes() / 1024 / 1024:F0}M"
        Me.ThisIsYourWindow1.TitleTextPrivateProtocol = t1
    End Sub
End Class
