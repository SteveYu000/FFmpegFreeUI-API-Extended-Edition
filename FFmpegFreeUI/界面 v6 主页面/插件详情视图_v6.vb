Option Strict On
Option Explicit On

Imports System.Drawing.Drawing2D

Friend NotInheritable Class 插件详情显示数据_v6
    Public Property 状态文本 As String = "未选择"
    Public Property 状态颜色 As Color = Color.FromArgb(150, 175, 195)
    Public Property 名称 As String = "请选择一个插件"
    Public Property 文件名 As String = "-"
    Public Property 信息行 As IReadOnlyList(Of KeyValuePair(Of String, String)) = Array.Empty(Of KeyValuePair(Of String, String))()
    Public Property 文件路径 As String = "-"
    Public Property 消息标题 As String = "使用提示"
    Public Property 消息内容 As String = "从左侧选择插件后，可查看接口、版本和加载信息。"
    Public Property 消息标题颜色 As Color = Color.FromArgb(140, 255, 255, 255)
    Public Property 消息文字颜色 As Color = Color.Silver
    Public Property 消息背景颜色 As Color = Color.FromArgb(20, 220, 220, 220)
End Class

''' <summary>
''' 插件详情的单表面绘制控件。与 AgentRoom 相同，滚动时只移动和重绘一个控件，
''' 避免大量透明 Label 在 ModernPanel 中逐个重排、重绘造成的闪烁和文字拖影。
''' </summary>
Friend NotInheritable Class 插件详情视图_v6
    Inherits Control

    Private ReadOnly 标题字体 As New Font("Microsoft YaHei UI", 13.0F, FontStyle.Regular)
    Private ReadOnly 名称字体 As New Font("Microsoft YaHei UI", 12.0F, FontStyle.Regular)
    Private ReadOnly 正文字体 As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Regular)
    Private 数据 As New 插件详情显示数据_v6
    Private 信息行高度 As Integer() = Array.Empty(Of Integer)()
    Private 名称高度 As Integer
    Private 路径高度 As Integer
    Private 消息框高度 As Integer
    Private 正在更新高度 As Boolean

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or
                 ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        DoubleBuffered = True
        BackColor = Color.Transparent
        Dock = DockStyle.Top
        Margin = Padding.Empty
        TabStop = False
        更新布局测量()
    End Sub

    Public Sub 显示详情(value As 插件详情显示数据_v6)
        数据 = If(value, New 插件详情显示数据_v6)
        更新布局测量()
        Invalidate()
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        If Not 正在更新高度 Then 更新布局测量()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim width = Math.Max(1, ClientSize.Width)
        Dim y = 0
        Dim headerHeight = 缩放(40)
        Dim badgeWidth = Math.Min(Math.Max(缩放(72), 测量单行宽度(数据.状态文本, 正文字体) + 缩放(20)), Math.Max(缩放(72), width \ 2))
        Dim badgeHeight = 缩放(28)
        Dim badgeRect As New Rectangle(Math.Max(0, width - badgeWidth), Math.Max(0, (headerHeight - badgeHeight) \ 2), badgeWidth, badgeHeight)

        绘制文本(e.Graphics, "插件详情", 标题字体, New Rectangle(0, 0, Math.Max(0, badgeRect.Left - 缩放(8)), headerHeight), Color.Silver, TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        绘制圆角矩形(e.Graphics, badgeRect, 缩放(9), Color.FromArgb(30, 数据.状态颜色))
        绘制文本(e.Graphics, 数据.状态文本, 正文字体, badgeRect, 数据.状态颜色, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        y += headerHeight

        绘制文本(e.Graphics, 数据.名称, 名称字体, New Rectangle(0, y, width, 名称高度), Color.Silver, TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.VerticalCenter)
        y += 名称高度
        Dim fileHeight = 缩放(28)
        绘制文本(e.Graphics, 数据.文件名, 正文字体, New Rectangle(0, y, width, fileHeight), Color.FromArgb(140, 255, 255, 255), TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        y += fileHeight

        Dim separatorHeight = 缩放(12)
        Using separatorPen As New Pen(Color.FromArgb(80, 220, 220, 220), Math.Max(1.0F, CSng(DeviceDpi) / 96.0F))
            e.Graphics.DrawLine(separatorPen, 0, y + separatorHeight \ 2, width, y + separatorHeight \ 2)
        End Using
        y += separatorHeight

        Dim rows = If(数据.信息行, Array.Empty(Of KeyValuePair(Of String, String))())
        Dim captionWidth = Math.Max(缩放(86), CInt(width * 0.46F))
        For index = 0 To rows.Count - 1
            Dim rowHeight = If(index < 信息行高度.Length, 信息行高度(index), 缩放(32))
            绘制文本(e.Graphics, rows(index).Key, 正文字体, New Rectangle(0, y, captionWidth, rowHeight), Color.FromArgb(140, 255, 255, 255), TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            绘制文本(e.Graphics, rows(index).Value, 正文字体, New Rectangle(captionWidth, y, Math.Max(0, width - captionWidth), rowHeight), Color.Silver, TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.VerticalCenter)
            y += rowHeight
        Next

        Dim sectionTitleHeight = 缩放(30)
        绘制文本(e.Graphics, "文件位置", 正文字体, New Rectangle(0, y, width, sectionTitleHeight), Color.FromArgb(140, 255, 255, 255), TextFormatFlags.VerticalCenter)
        y += sectionTitleHeight
        绘制文本(e.Graphics, 数据.文件路径, 正文字体, New Rectangle(0, y, width, 路径高度), Color.Silver, TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis)
        y += 路径高度

        绘制文本(e.Graphics, 数据.消息标题, 正文字体, New Rectangle(0, y, width, sectionTitleHeight), 数据.消息标题颜色, TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        y += sectionTitleHeight
        Dim messageRect As New Rectangle(0, y, width, 消息框高度)
        绘制圆角矩形(e.Graphics, messageRect, 缩放(8), 数据.消息背景颜色)
        messageRect.Inflate(-缩放(12), -缩放(9))
        绘制文本(e.Graphics, 数据.消息内容, 正文字体, messageRect, 数据.消息文字颜色, TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            标题字体.Dispose()
            名称字体.Dispose()
            正文字体.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub 更新布局测量()
        Dim width = Math.Max(缩放(120), ClientSize.Width)
        名称高度 = 测量多行高度(数据.名称, 名称字体, width, 缩放(38), 缩放(62))

        Dim rows = If(数据.信息行, Array.Empty(Of KeyValuePair(Of String, String))())
        Dim valueWidth = Math.Max(缩放(70), width - Math.Max(缩放(86), CInt(width * 0.46F)))
        ReDim 信息行高度(Math.Max(0, rows.Count - 1))
        If rows.Count = 0 Then 信息行高度 = Array.Empty(Of Integer)()
        For index = 0 To rows.Count - 1
            信息行高度(index) = 测量多行高度(rows(index).Value, 正文字体, valueWidth, 缩放(32), 缩放(54))
        Next

        路径高度 = 测量多行高度(数据.文件路径, 正文字体, width, 缩放(58), 缩放(100))
        消息框高度 = 测量多行高度(数据.消息内容, 正文字体, Math.Max(缩放(40), width - 缩放(24)), 缩放(70), 缩放(220)) + 缩放(18)

        Dim nextHeight = 缩放(40 + 28 + 12 + 30 + 30 + 8) + 名称高度 + 路径高度 + 消息框高度 + 信息行高度.Sum()
        nextHeight = Math.Max(nextHeight, 缩放(560))
        If Height <> nextHeight Then
            正在更新高度 = True
            Try
                Height = nextHeight
            Finally
                正在更新高度 = False
            End Try
        End If
    End Sub

    Private Function 测量多行高度(text As String, font As Font, width As Integer, minimum As Integer, maximum As Integer) As Integer
        Dim proposedSize As New Size(Math.Max(1, width), 10000)
        Dim measured = TextRenderer.MeasureText(If(text, "-"), font, proposedSize, TextFormatFlags.NoPrefix Or TextFormatFlags.WordBreak Or TextFormatFlags.TextBoxControl)
        Return Math.Max(minimum, Math.Min(maximum, measured.Height + 缩放(2)))
    End Function

    Private Shared Function 测量单行宽度(text As String, font As Font) As Integer
        Return TextRenderer.MeasureText(If(text, ""), font, Size.Empty, TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine).Width
    End Function

    Private Shared Sub 绘制文本(graphics As Graphics, text As String, font As Font, bounds As Rectangle, color As Color, flags As TextFormatFlags)
        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then Exit Sub
        TextRenderer.DrawText(graphics, If(text, ""), font, bounds, color, flags Or TextFormatFlags.NoPrefix Or TextFormatFlags.PreserveGraphicsClipping)
    End Sub

    Private Shared Sub 绘制圆角矩形(graphics As Graphics, bounds As Rectangle, radius As Integer, color As Color)
        If bounds.Width <= 0 OrElse bounds.Height <= 0 OrElse color.A = 0 Then Exit Sub
        Dim safeRadius = Math.Max(1, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) \ 2))
        Dim diameter = safeRadius * 2
        Using path As New GraphicsPath()
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90)
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90)
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90)
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90)
            path.CloseFigure()
            Using brush As New SolidBrush(color)
                graphics.FillPath(brush, path)
            End Using
        End Using
    End Sub

    Private Function 缩放(value As Integer) As Integer
        Return Math.Max(1, CInt(Math.Round(value * CDbl(DeviceDpi) / 96.0R)))
    End Function
End Class
