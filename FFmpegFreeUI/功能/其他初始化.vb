Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports LakeUI

Partial Public Class FormMain_v6
    Private Sub 绑定主页面选项卡()
        Dim 页面绑定 As (索引 As Integer, 页面 As Control, 面板 As ModernPanel)() = {
            (0, Form_v6_起始页面, Form_v6_起始页面.ModernPanel1),
            (1, Form_v6_编码队列, Form_v6_编码队列.ModernPanel1),
            (3, Form_v6_参数面板.私有界面_参数总览, Form_v6_参数面板.私有界面_参数总览.ModernPanel1),
            (4, Form_v6_参数面板, Form_v6_参数面板.ModernPanel1),
            (5, Form_v6_Agent, Form_v6_Agent.ModernPanel1),
            (6, Form_v6_社区_敬请期待, Form_v6_社区_敬请期待.ModernPanel1),
            (8, Form_v6_媒体信息, Form_v6_媒体信息.ModernPanel1),
            (9, Form_v6_调试播放器, Form_v6_调试播放器.ModernPanel1),
            (10, Form_v6_性能监控, Form_v6_性能监控.ModernPanel1),
            (11, Form_v6_集成工具, Form_v6_集成工具.ModernPanel1),
            (13, Form_v6_设置, Form_v6_设置.ModernPanel1),
            (14, Form_v6_支持者, Form_v6_支持者.ModernPanel1),
            (16, Form_v6_插件管理, Form_v6_插件管理.ModernPanel1)}
        For Each 绑定项 In 页面绑定
            ModernTabListControl1.Items(绑定项.索引).BoundControl = 绑定项.页面
            绑定选项卡(绑定项.面板)
        Next
    End Sub

    Private Sub 应用窗口样式()
        Select Case 设置_v6.实例对象.窗口样式
            Case 1
                DwmWindowStyle.SetDarkMode(Handle, True)
            Case 2
                ThisIsYourWindow1.Attach(Me)
                If SP_UnLock AndAlso 设置_v6.实例对象.SP_毛玻璃模式 > 0 Then
                    应用毛玻璃控件设置()
                    Dim 边距 = CInt(10 * DeviceDpi / 96)
                    For Each 面板 As ModernPanel In {Form_v6_起始页面.ModernPanel1, Form_v6_准备文件.ModernPanel1, Form_v6_社区_个人中心.ModernPanel1, Form_v6_媒体信息.ModernPanel1, Form_v6_调试播放器.ModernPanel1}
                        If 面板 IsNot Nothing AndAlso Not 面板.IsDisposed Then 面板.Padding = New Padding(边距, 边距, 面板.Padding.Right, 面板.Padding.Bottom)
                    Next
                End If
        End Select
    End Sub

    Public Sub 应用毛玻璃控件设置()
        If Not SP_UnLock OrElse 设置_v6.实例对象.窗口样式 <> 2 OrElse 设置_v6.实例对象.SP_毛玻璃模式 <= 0 Then Return
        For Each 选项卡 As ModernTabListControl In {ModernTabListControl1, Form_v6_参数面板.ModernTabListControl1, Form_v6_集成工具.ModernTabListControl1, Form_v6_设置.ModernTabListControl1}
            If 选项卡 Is Nothing OrElse 选项卡.IsDisposed Then Continue For
            选项卡.TabStripBackColor = Color.Transparent
            选项卡.ContentBackColor = Color.Transparent
        Next
        For Each 选项卡 As ModernTabControl In {Form_v6_参数面板.私有界面_自定义参数.ModernTabControl1, Form_v6_参数面板.私有界面_附加内容.ModernTabControl1}
            If 选项卡 Is Nothing OrElse 选项卡.IsDisposed Then Continue For
            选项卡.TabStripBackColor = Color.Transparent
            选项卡.ContentBackColor = Color.Transparent
        Next
    End Sub

    Private Sub 询问国内更新服务器()
        ' Extended Edition 使用自己的 GitHub Release 检查页，不启用上游更新器及其镜像服务器选择。
    End Sub

    Private Sub 绑定选项卡核心(选项卡的根面板容器 As ModernPanel)
        If 选项卡的根面板容器 Is Nothing OrElse Not SP_UnLock OrElse 设置_v6.实例对象.SP_毛玻璃模式 <= 0 Then Return
        选项卡的根面板容器.BackColor = Color.Transparent
        选项卡的根面板容器.BackColor1 = Color.Transparent
        选项卡的根面板容器.BackgroundSource = Me
    End Sub

    Private Sub 添加插件选项卡核心(选项卡标题 As String, 面板 As Control)
        Dim 标题 = If(选项卡标题, String.Empty).Trim()
        If String.IsNullOrEmpty(标题) OrElse 面板 Is Nothing Then Return
        面板.Dock = DockStyle.Fill
        Dim 选项卡 As ModernTabListControl.ModernTabPage = Nothing
        If 插件选项卡页.TryGetValue(标题, 选项卡) Then
            选项卡.BoundControl = 面板
        Else
            选项卡 = New ModernTabListControl.ModernTabPage With {.Text = 标题, .BoundControl = 面板}
            ModernTabListControl1.Items.Insert(获取插件选项卡插入位置核心(), 选项卡)
            插件选项卡页(标题) = 选项卡
        End If
        配置插件页面背景核心(面板)
    End Sub

    Private Function 获取插件选项卡插入位置核心() As Integer
        ' 插件管理是底部插件区域的固定首项；官方和 Ext 插件入口随后按注册顺序追加。
        Return ModernTabListControl1.Items.Count
    End Function

    Private Sub 配置插件页面背景核心(页面 As Control)
        Dim 根面板 = 查找可绑定背景映射的插件ModernPanel(页面)
        If 根面板 IsNot Nothing Then 绑定选项卡核心(根面板)
    End Sub

    Private Async Function 执行关闭流程Async(事件参数 As CancelEventArgs) As Task
        事件参数.Cancel = False
        If Not 退出确认已完成 AndAlso Not 确认退出方式(事件参数) Then Return
        If Not 退出里程碑检查已完成 Then
            事件参数.Cancel = True
            If 退出里程碑检查进行中 Then Return
            退出里程碑检查进行中 = True
            Try
                Await 用户使用统计_v6.退出时后台检查Async(Me)
            Catch
            Finally
                退出里程碑检查进行中 = False
                退出里程碑检查已完成 = True
            End Try
            BeginInvoke(Sub() Close())
            Return
        End If
        Dim 需要启动退出助手 = Not 重启助手已启动 AndAlso (重启请求待执行 OrElse 网络功能_v6_自动更新.有待安装更新)
        If 需要启动退出助手 AndAlso Not 确认首次使用PowerShell退出助手() Then
            重启请求待执行 = False
            退出确认已完成 = False
            事件参数.Cancel = True
            Return
        End If
        Try
            编码队列_v6.立即保存队列()
        Catch 异常 As Exception
            If ExOverlayMsgBox(Me, "保存编码队列失败：" & 异常.Message & vbCrLf & "仍要退出吗？", MsgBoxStyle.YesNo Or MsgBoxStyle.Critical, "队列保存失败") <> MsgBoxResult.Yes Then
                重启请求待执行 = False
                事件参数.Cancel = True
                退出确认已完成 = False
                Return
            End If
        End Try
        If 退出时清除所有任务 AndAlso 编码队列_v6.获取进行中任务数量() > 0 Then 编码队列_v6.停止所有进行中任务()
        端口监听_v6.停止客户端()
        If Form_v6_调试播放器.ffplayHandle <> IntPtr.Zero Then Form_v6_调试播放器.停止()
        网络功能_v6_自动更新.退出时取消下载()
        If 需要启动退出助手 Then
            Try
                If 网络功能_v6_自动更新.有待安装更新 Then
                    网络功能_v6_自动更新.启动已准备的更新助手(重启请求待执行)
                Else
                    启动重启助手()
                End If
                重启助手已启动 = True
                重启请求待执行 = False
                设置_v6.实例对象.插件管理_PowerShell重启说明已显示 = True
            Catch 异常 As Exception
                重启请求待执行 = False
                事件参数.Cancel = True
                ExOverlayMsgBox(Me, "无法启动退出助手：" & 异常.Message, MsgBoxStyle.Critical, "退出失败")
                Return
            End Try
        End If
    End Function

    Private Function 确认退出方式(事件参数 As CancelEventArgs) As Boolean
        Dim 进行中任务数量 = 编码队列_v6.获取进行中任务数量()
        退出时清除所有任务 = False
        If 进行中任务数量 > 0 Then
            If ExOverlayMsgBox(Me, $"当前仍有 {进行中任务数量} 个任务正在运行或暂停。确认退出吗？重启后队列会自动恢复，但无法重新连接这些进程的输入输出。", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "确认退出") <> MsgBoxResult.Yes Then
                重启请求待执行 = False
                事件参数.Cancel = True
                Return False
            End If
        End If
        退出确认已完成 = True
        Return True
    End Function

End Class
