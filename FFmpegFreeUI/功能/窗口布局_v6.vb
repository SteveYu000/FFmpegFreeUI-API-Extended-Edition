Imports System.Runtime.CompilerServices

''' <summary>宿主窗口的尺寸记忆及功能子窗口的首次布局，不参与页面绘制或插件合同。</summary>
Friend NotInheritable Class 窗口布局_v6
    Private Shared ReadOnly 子窗口表 As New ConditionalWeakTable(Of Form, 子窗口状态)()

    Private Class 子窗口状态
        Public Property 父窗口 As Form
    End Class

    Private Sub New()
    End Sub

    Friend Shared Function 获取顶层窗口(control As Control) As Form
        Dim current = control
        While current IsNot Nothing
            Dim form = TryCast(current, Form)
            If form IsNot Nothing AndAlso form.TopLevel Then Return If(form.IsDisposed, Nothing, form)
            current = current.Parent
        End While
        Return Nothing
    End Function

    Friend Shared Function 限制窗口大小(期望大小 As Size, 最小大小 As Size, 工作区大小 As Size) As Size
        Dim maxWidth = Math.Max(1, 工作区大小.Width)
        Dim maxHeight = Math.Max(1, 工作区大小.Height)
        Return New Size(Math.Clamp(期望大小.Width, Math.Clamp(最小大小.Width, 1, maxWidth), maxWidth),
                        Math.Clamp(期望大小.Height, Math.Clamp(最小大小.Height, 1, maxHeight), maxHeight))
    End Function

    Friend Shared Function 还原窗口大小(逻辑大小 As Size, dpi As Integer, 最小大小 As Size, 工作区大小 As Size) As Size
        Dim scale = If(dpi > 0, dpi, 96) / 96.0R
        ' 先按工作区截断再转为 Integer，容忍损坏配置中的极大尺寸。
        Dim desired As New Size(CInt(Math.Round(Math.Clamp(逻辑大小.Width * scale, 1, Math.Max(1, 工作区大小.Width)))),
                                CInt(Math.Round(Math.Clamp(逻辑大小.Height * scale, 1, Math.Max(1, 工作区大小.Height)))))
        Return 限制窗口大小(desired, 最小大小, 工作区大小)
    End Function

    Friend Shared Sub 恢复主窗口(window As Form)
        Dim settings = 设置_v6.实例对象
        If settings.窗口布局 <> 1 OrElse settings.上次窗口宽度 <= 0 OrElse settings.上次窗口高度 <= 0 Then Return
        ' 显式 fullscreen 启动参数优先于记忆布局。
        If window.WindowState <> FormWindowState.Normal Then Return
        Dim area = Screen.FromControl(window).WorkingArea
        Dim targetSize = 还原窗口大小(New Size(settings.上次窗口宽度, settings.上次窗口高度), window.DeviceDpi, window.MinimumSize, area.Size)
        限制最小尺寸(window, area.Size)
        window.StartPosition = FormStartPosition.Manual
        window.Bounds = New Rectangle(area.Left + (area.Width - targetSize.Width) \ 2, area.Top + (area.Height - targetSize.Height) \ 2, targetSize.Width, targetSize.Height)
        If settings.上次窗口最大化 Then window.WindowState = FormWindowState.Maximized
    End Sub

    Friend Shared Sub 记录主窗口(window As Form, 最近非最小化状态 As FormWindowState)
        If 设置_v6.实例对象.窗口布局 <> 1 Then Return
        Dim bounds = If(window.WindowState = FormWindowState.Normal, window.Bounds, window.RestoreBounds)
        保存正常窗口尺寸(bounds.Size, window.DeviceDpi, 最近非最小化状态 = FormWindowState.Maximized)
    End Sub

    Friend Shared Sub 保存正常窗口尺寸(大小 As Size, dpi As Integer, 最大化 As Boolean)
        If 设置_v6.实例对象.窗口布局 <> 1 OrElse 大小.Width <= 0 OrElse 大小.Height <= 0 Then Return
        Dim scale = 96.0R / If(dpi > 0, dpi, 96)
        设置_v6.实例对象.上次窗口宽度 = Math.Max(1, CInt(Math.Round(大小.Width * scale)))
        设置_v6.实例对象.上次窗口高度 = Math.Max(1, CInt(Math.Round(大小.Height * scale)))
        设置_v6.实例对象.上次窗口最大化 = 最大化
    End Sub

    Friend Shared Sub 准备子窗口(window As Form, 父控件 As Control)
        Dim owner = 获取顶层窗口(父控件)
        If window Is Nothing OrElse window.IsDisposed OrElse owner Is Nothing OrElse owner Is window Then Return
        Dim state = 子窗口表.GetValue(window,
            Function(form)
                ' 注册在窗体自身的 Load 之后：字体、DPI 和 LakeUI Attach 已完成，再设置最终尺寸。
                AddHandler form.Load, AddressOf 子窗口加载
                Return New 子窗口状态()
            End Function)
        state.父窗口 = owner
        应用子窗口尺寸(window, owner)
    End Sub

    Private Shared Sub 子窗口加载(sender As Object, e As EventArgs)
        Dim window = TryCast(sender, Form)
        Dim state As 子窗口状态 = Nothing
        If window IsNot Nothing AndAlso 子窗口表.TryGetValue(window, state) Then 应用子窗口尺寸(window, state.父窗口)
    End Sub

    Private Shared Sub 应用子窗口尺寸(window As Form, owner As Form)
        If owner Is Nothing OrElse owner.IsDisposed Then Return
        Dim area = Screen.FromControl(owner).WorkingArea
        Dim basis = If(owner.WindowState = FormWindowState.Minimized, owner.RestoreBounds, owner.Bounds)
        If owner.WindowState = FormWindowState.Maximized Then basis = Rectangle.Intersect(basis, area)
        Dim targetSize = 限制窗口大小(basis.Size, Size.Empty, area.Size)
        Dim position As New Point(Math.Clamp(basis.Left + (basis.Width - targetSize.Width) \ 2, area.Left, area.Right - targetSize.Width),
                                  Math.Clamp(basis.Top + (basis.Height - targetSize.Height) \ 2, area.Top, area.Bottom - targetSize.Height))
        window.SuspendLayout()
        Try
            window.WindowState = FormWindowState.Normal
            window.StartPosition = FormStartPosition.Manual
            限制最小尺寸(window, targetSize)
            ' 先移动到父窗口所在显示器，让 DPI 切换完成后再应用父窗口的实际像素尺寸。
            window.Location = position
            window.Size = targetSize
        Finally
            window.ResumeLayout(True)
        End Try
    End Sub

    Private Shared Sub 限制最小尺寸(window As Form, 最大大小 As Size)
        Dim minimum = window.MinimumSize
        If minimum.Width > 最大大小.Width OrElse minimum.Height > 最大大小.Height Then
            window.MinimumSize = New Size(Math.Min(minimum.Width, 最大大小.Width), Math.Min(minimum.Height, 最大大小.Height))
        End If
    End Sub
End Class
