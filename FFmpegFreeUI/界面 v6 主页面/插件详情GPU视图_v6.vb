Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Numerics
Imports LakeUI
Imports Vortice.Direct2D1

''' <summary>
''' LakeUI 5 单 HWND 插件详情视图。文字、设置入口与滚动条在同一 GPU surface 中绘制，
''' 滚动时只改变本控件的业务偏移，不移动子 HWND，也不依赖 GDI 透明背景合成。
''' </summary>
Friend NotInheritable Class 插件详情GPU视图_v6
    Inherits Control
    Implements D3D_IGpuRenderable, D3D_IGpuInvalidationSource, D3D_IBackgroundSourceProvider, V5_IGpuPresentationSource

    Private ReadOnly 标题字体 As New Font("Microsoft YaHei UI", 13.0F, FontStyle.Regular)
    Private ReadOnly 名称字体 As New Font("Microsoft YaHei UI", 12.0F, FontStyle.Regular)
    Private ReadOnly 正文字体 As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Regular)
    Private 数据 As New 插件详情显示数据_v6
    Private 信息行高度 As Integer() = Array.Empty(Of Integer)()
    Private 名称高度 As Integer
    Private 路径高度 As Integer
    Private 消息框高度 As Integer
    Private 内容总高度 As Integer
    Private 滚动偏移 As Integer
    Private 背景来源 As Control
    Private 设置按钮区域 As RectangleF
    Private 滚动条滑块区域 As RectangleF
    Private 设置按钮悬停 As Boolean
    Private 设置按钮按下 As Boolean
    Private 正在拖动滚动条 As Boolean
    Private 拖动起点Y As Integer
    Private 拖动起点偏移 As Integer

    Public Event 设置入口点击 As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or
                 ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor Or
                 ControlStyles.Selectable, True)
        DoubleBuffered = True
        BackColor = Color.Transparent
        Dock = DockStyle.Fill
        Margin = Padding.Empty
        TabStop = False
        AccessibleName = "插件详情"
        更新布局测量()
    End Sub

#Region "背景源"
    <Category("LakeUI"),
     Description("GPU 背景采样源。"),
     DefaultValue(GetType(Control), Nothing), Browsable(True)>
    Public Property BackgroundSource As Control
        Get
            Return 背景来源
        End Get
        Set(value As Control)
            If 背景来源 Is value Then Return
            背景来源 = D3D_BackgroundPenetration.SetBackgroundSource(Me, 背景来源, value)
            请求GPU渲染()
        End Set
    End Property

    Public Function TryGetBackgroundSource(ByRef source As Control) As Boolean Implements D3D_IBackgroundSourceProvider.TryGetBackgroundSource
        source = 背景来源
        Return source IsNot Nothing
    End Function

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        If 背景来源 IsNot Nothing Then Return
        MyBase.OnPaintBackground(e)
    End Sub
#End Region

    Public Sub 显示详情(value As 插件详情显示数据_v6)
        数据 = If(value, New 插件详情显示数据_v6())
        AccessibleDescription = If(数据.设置入口可用, "当前插件提供设置页", "当前插件未提供设置页")
        TabStop = 数据.设置入口可用
        If Not 数据.设置入口可用 Then
            设置按钮悬停 = False
            设置按钮按下 = False
        End If
        更新布局测量()
        请求GPU渲染()
    End Sub

    Public Function GetRenderBounds() As Rectangle Implements D3D_IGpuInvalidationSource.GetRenderBounds
        Return New Rectangle(Point.Empty, Size)
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If Not D3D_PaintBridge.PaintRenderable(e, Me, Me) Then
            MyBase.OnPaint(e)
            绘制设计时预览(e.Graphics)
        End If
    End Sub

    Public Sub RenderGpu(context As D3D_PaintContext) Implements D3D_IGpuRenderable.RenderGpu
        If context Is Nothing OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then Return

        Dim fullBounds As New RectangleF(0, 0, ClientSize.Width, ClientSize.Height)
        If Not context.DrawBackgroundSource(Me, 背景来源, fullBounds) Then
            context.FillRectangle(fullBounds, Color.FromArgb(48, 48, 48))
        End If

        Dim headerHeight = 缩放(40)
        Dim contentWidth = 获取内容宽度()
        绘制固定标题栏(context, contentWidth, headerHeight)

        Dim viewportHeight = Math.Max(0, ClientSize.Height - headerHeight)
        If viewportHeight > 0 Then
            Using context.PushClip(New RectangleF(0, headerHeight, contentWidth, viewportHeight))
                绘制滚动内容(context, contentWidth, headerHeight - 滚动偏移)
            End Using
        End If
        绘制滚动条(context, headerHeight)
    End Sub

    Private Sub 绘制固定标题栏(context As D3D_PaintContext, width As Integer, headerHeight As Integer)
        Dim badgeHeight = 缩放(28)
        Dim badgeWidth = Math.Min(Math.Max(缩放(72), 测量单行宽度(数据.状态文本, 正文字体) + 缩放(20)), Math.Max(缩放(72), width \ 2))
        Dim badgeRect As New RectangleF(Math.Max(0, width - badgeWidth), Math.Max(0, (headerHeight - badgeHeight) / 2.0F), badgeWidth, badgeHeight)

        Dim buttonSize = 缩放(30)
        Dim measuredTitleWidth = 测量单行宽度("插件详情", 标题字体)
        Dim preferredLeft = measuredTitleWidth + 缩放(10)
        Dim maximumLeft = Math.Max(0, CInt(badgeRect.Left) - 缩放(8) - buttonSize)
        Dim buttonLeft = Math.Min(preferredLeft, maximumLeft)
        设置按钮区域 = New RectangleF(buttonLeft, Math.Max(0, (headerHeight - buttonSize) / 2.0F), buttonSize, buttonSize)

        context.DrawText("插件详情", 标题字体, Color.Silver,
                         New RectangleF(0, 0, Math.Max(0, 设置按钮区域.Left - 缩放(4)), headerHeight),
                         TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        context.FillRoundedRectangle(badgeRect, 缩放(9), Color.FromArgb(30, 数据.状态颜色))
        context.DrawText(数据.状态文本, 正文字体, 数据.状态颜色, badgeRect,
                         TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        绘制设置按钮(context)
    End Sub

    Private Sub 绘制设置按钮(context As D3D_PaintContext)
        Dim backgroundColor As Color
        If Not 数据.设置入口可用 Then
            backgroundColor = Color.FromArgb(115, 10, 10, 10)
        ElseIf 设置按钮按下 Then
            backgroundColor = Color.FromArgb(80, 220, 220, 220)
        ElseIf 设置按钮悬停 Then
            backgroundColor = Color.FromArgb(60, 220, 220, 220)
        Else
            backgroundColor = Color.FromArgb(36, 220, 220, 220)
        End If
        context.FillRoundedRectangle(设置按钮区域, 缩放(8), backgroundColor)

        Dim iconColor = If(数据.设置入口可用, Color.CornflowerBlue, Color.FromArgb(105, 115, 120))
        Dim brush = context.Compositor.BrushCache.GetSolidBrush(context.DeviceContext, iconColor, context.DeviceGeneration)
        Dim size = Math.Min(设置按钮区域.Width, 设置按钮区域.Height)
        Dim centerX = 设置按钮区域.Left + 设置按钮区域.Width / 2.0F
        Dim centerY = 设置按钮区域.Top + 设置按钮区域.Height / 2.0F
        Dim outerRadius = size * 0.29F
        Dim toothInnerRadius = size * 0.22F
        Dim ringRadius = size * 0.19F
        Dim hubRadius = size * 0.065F
        Dim stroke = Math.Max(1.4F, size * 0.052F)
        For index = 0 To 7
            Dim angle = index * Math.PI / 4.0R
            context.DeviceContext.DrawLine(
                New Vector2(centerX + CSng(Math.Cos(angle) * toothInnerRadius), centerY + CSng(Math.Sin(angle) * toothInnerRadius)),
                New Vector2(centerX + CSng(Math.Cos(angle) * outerRadius), centerY + CSng(Math.Sin(angle) * outerRadius)),
                brush,
                stroke)
        Next
        context.DeviceContext.DrawEllipse(New Ellipse(New Vector2(centerX, centerY), ringRadius, ringRadius), brush, stroke)
        context.DeviceContext.DrawEllipse(New Ellipse(New Vector2(centerX, centerY), hubRadius, hubRadius), brush, stroke)
    End Sub

    Private Sub 绘制滚动内容(context As D3D_PaintContext, width As Integer, startY As Integer)
        Dim y = startY
        context.DrawText(数据.名称, 名称字体, Color.Silver, New RectangleF(0, y, width, 名称高度),
                         TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
        y += 名称高度

        Dim fileHeight = 缩放(28)
        context.DrawText(数据.文件名, 正文字体, Color.FromArgb(140, 255, 255, 255), New RectangleF(0, y, width, fileHeight),
                         TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        y += fileHeight

        Dim separatorHeight = 缩放(12)
        context.FillRectangle(New RectangleF(0, y + separatorHeight / 2.0F, width, Math.Max(1.0F, CSng(DeviceDpi) / 96.0F)), Color.FromArgb(80, 220, 220, 220))
        y += separatorHeight

        Dim rows = If(数据.信息行, Array.Empty(Of KeyValuePair(Of String, String))())
        Dim captionWidth = Math.Max(缩放(86), CInt(width * 0.46F))
        For index = 0 To rows.Count - 1
            Dim rowHeight = If(index < 信息行高度.Length, 信息行高度(index), 缩放(32))
            context.DrawText(rows(index).Key, 正文字体, Color.FromArgb(140, 255, 255, 255), New RectangleF(0, y, captionWidth, rowHeight),
                             TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            context.DrawText(rows(index).Value, 正文字体, Color.Silver, New RectangleF(captionWidth, y, Math.Max(0, width - captionWidth), rowHeight),
                             TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
            y += rowHeight
        Next

        Dim sectionTitleHeight = 缩放(30)
        context.DrawText("文件位置", 正文字体, Color.FromArgb(140, 255, 255, 255), New RectangleF(0, y, width, sectionTitleHeight),
                         TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
        y += sectionTitleHeight
        context.DrawText(数据.文件路径, 正文字体, Color.Silver, New RectangleF(0, y, width, 路径高度),
                         TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        y += 路径高度

        context.DrawText(数据.消息标题, 正文字体, 数据.消息标题颜色, New RectangleF(0, y, width, sectionTitleHeight),
                         TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        y += sectionTitleHeight
        Dim messageRect As New RectangleF(0, y, width, 消息框高度)
        context.FillRoundedRectangle(messageRect, 缩放(8), 数据.消息背景颜色)
        messageRect.Inflate(-缩放(12), -缩放(9))
        context.DrawText(数据.消息内容, 正文字体, 数据.消息文字颜色, messageRect,
                         TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
    End Sub

    Private Sub 绘制滚动条(context As D3D_PaintContext, headerHeight As Integer)
        Dim maximum = 最大滚动偏移()
        If maximum <= 0 Then
            滚动条滑块区域 = RectangleF.Empty
            Return
        End If

        Dim barWidth = 缩放(8)
        Dim margin = 缩放(3)
        Dim trackTop = headerHeight + margin
        Dim trackHeight = Math.Max(1, ClientSize.Height - trackTop - margin)
        Dim trackRect As New RectangleF(ClientSize.Width - barWidth, trackTop, barWidth, trackHeight)
        Dim viewportHeight = Math.Max(1, ClientSize.Height - headerHeight)
        Dim thumbHeight = Math.Max(缩放(30), CInt(Math.Round(trackHeight * Math.Min(1.0R, viewportHeight / CDbl(Math.Max(1, 内容总高度))))))
        Dim travel = Math.Max(0, trackHeight - thumbHeight)
        Dim thumbTop = trackTop + If(maximum = 0, 0, CSng(travel * (滚动偏移 / CDbl(maximum))))
        滚动条滑块区域 = New RectangleF(trackRect.Left, thumbTop, barWidth, thumbHeight)

        context.FillRoundedRectangle(trackRect, barWidth / 2.0F, Color.FromArgb(30, 220, 220, 220))
        context.FillRoundedRectangle(滚动条滑块区域, barWidth / 2.0F, Color.FromArgb(105, 220, 220, 220))
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        更新布局测量()
        请求GPU渲染()
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        If 最大滚动偏移() <= 0 OrElse e.Delta = 0 Then Return
        Dim steps = Math.Max(1, Math.Abs(e.Delta) \ SystemInformation.MouseWheelScrollDelta)
        Dim direction = If(e.Delta > 0, -1, 1)
        设置滚动偏移(滚动偏移 + direction * steps * 缩放(48))
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If 正在拖动滚动条 Then
            Dim maximum = 最大滚动偏移()
            Dim trackHeight = Math.Max(1.0F, ClientSize.Height - 缩放(40) - 缩放(6))
            Dim travel = Math.Max(1.0F, trackHeight - 滚动条滑块区域.Height)
            设置滚动偏移(拖动起点偏移 + CInt(Math.Round((e.Y - 拖动起点Y) * maximum / travel)))
            Return
        End If

        Dim hover = 数据.设置入口可用 AndAlso 设置按钮区域.Contains(e.Location)
        If hover <> 设置按钮悬停 Then
            设置按钮悬停 = hover
            Cursor = If(hover, Cursors.Hand, Cursors.Default)
            请求GPU渲染()
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If 正在拖动滚动条 Then Return
        If 设置按钮悬停 OrElse 设置按钮按下 Then
            设置按钮悬停 = False
            设置按钮按下 = False
            Cursor = Cursors.Default
            请求GPU渲染()
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button <> MouseButtons.Left Then Return
        If 数据.设置入口可用 AndAlso 设置按钮区域.Contains(e.Location) Then
            设置按钮按下 = True
            Focus()
            请求GPU渲染()
            Return
        End If
        If Not 滚动条滑块区域.IsEmpty AndAlso 滚动条滑块区域.Contains(e.Location) Then
            正在拖动滚动条 = True
            拖动起点Y = e.Y
            拖动起点偏移 = 滚动偏移
            Capture = True
        ElseIf 最大滚动偏移() > 0 AndAlso e.X >= ClientSize.Width - 缩放(12) Then
            设置滚动偏移(滚动偏移 + If(e.Y < 滚动条滑块区域.Top, -1, 1) * Math.Max(缩放(64), ClientSize.Height - 缩放(80)))
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button <> MouseButtons.Left Then Return
        If 正在拖动滚动条 Then
            正在拖动滚动条 = False
            Capture = False
        End If
        If 设置按钮按下 Then
            Dim shouldOpen = 数据.设置入口可用 AndAlso 设置按钮区域.Contains(e.Location)
            设置按钮按下 = False
            请求GPU渲染()
            If shouldOpen Then RaiseEvent 设置入口点击(Me, EventArgs.Empty)
        End If
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        If 数据.设置入口可用 AndAlso (e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.Space) Then
            RaiseEvent 设置入口点击(Me, EventArgs.Empty)
            e.Handled = True
            Return
        End If
        MyBase.OnKeyDown(e)
    End Sub

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        If Not Visible Then D3D_BackgroundPenetration.UnregisterConsumer(Me, 背景来源)
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        D3D_BackgroundPenetration.UnregisterConsumer(Me, 背景来源)
        MyBase.OnHandleDestroyed(e)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            D3D_BackgroundPenetration.UnregisterConsumer(Me, 背景来源)
            标题字体.Dispose()
            名称字体.Dispose()
            正文字体.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub 更新布局测量()
        Dim width = Math.Max(缩放(120), 获取内容宽度())
        名称高度 = 测量多行高度(数据.名称, 名称字体, width, 缩放(38), 缩放(62))

        Dim rows = If(数据.信息行, Array.Empty(Of KeyValuePair(Of String, String))())
        Dim valueWidth = Math.Max(缩放(70), width - Math.Max(缩放(86), CInt(width * 0.46F)))
        If rows.Count = 0 Then
            信息行高度 = Array.Empty(Of Integer)()
        Else
            ReDim 信息行高度(rows.Count - 1)
            For index = 0 To rows.Count - 1
                信息行高度(index) = 测量多行高度(rows(index).Value, 正文字体, valueWidth, 缩放(32), 缩放(54))
            Next
        End If

        路径高度 = 测量多行高度(数据.文件路径, 正文字体, width, 缩放(58), 缩放(100))
        消息框高度 = 测量多行高度(数据.消息内容, 正文字体, Math.Max(缩放(40), width - 缩放(24)), 缩放(70), 缩放(220)) + 缩放(18)
        内容总高度 = 名称高度 + 缩放(28 + 12 + 30 + 30 + 8) + 路径高度 + 消息框高度 + 信息行高度.Sum()
        滚动偏移 = Math.Min(滚动偏移, 最大滚动偏移())
    End Sub

    Private Function 获取内容宽度() As Integer
        Return Math.Max(1, ClientSize.Width - 缩放(14))
    End Function

    Private Function 最大滚动偏移() As Integer
        Return Math.Max(0, 内容总高度 - Math.Max(0, ClientSize.Height - 缩放(40)))
    End Function

    Private Sub 设置滚动偏移(value As Integer)
        Dim nextValue = Math.Max(0, Math.Min(最大滚动偏移(), value))
        If nextValue = 滚动偏移 Then Return
        滚动偏移 = nextValue
        请求GPU渲染()
    End Sub

    Private Sub 请求GPU渲染()
        If IsDisposed Then Return
        D3D_RenderCore.RequestRender(Me, New Rectangle(Point.Empty, Size))
    End Sub

    Private Function 测量多行高度(text As String, font As Font, width As Integer, minimum As Integer, maximum As Integer) As Integer
        Dim proposedSize As New Size(Math.Max(1, width), 10000)
        Dim measured = TextRenderer.MeasureText(If(text, "-"), font, proposedSize,
                                                TextFormatFlags.NoPrefix Or TextFormatFlags.WordBreak Or TextFormatFlags.TextBoxControl)
        Return Math.Max(minimum, Math.Min(maximum, measured.Height + 缩放(2)))
    End Function

    Private Shared Function 测量单行宽度(text As String, font As Font) As Integer
        Return TextRenderer.MeasureText(If(text, ""), font, Size.Empty,
                                        TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine).Width
    End Function

    ''' <summary>仅用于 VS 设计器或 V5 设备不可用时的静态预览；正常运行不进入此路径。</summary>
    Private Sub 绘制设计时预览(graphics As Graphics)
        If graphics Is Nothing OrElse ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then Return
        graphics.Clear(Color.FromArgb(48, 48, 48))
        TextRenderer.DrawText(graphics, "插件详情", 标题字体,
                              New Rectangle(0, 0, Math.Max(1, ClientSize.Width - 缩放(90)), 缩放(40)),
                              Color.Silver,
                              TextFormatFlags.NoPrefix Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        TextRenderer.DrawText(graphics, 数据.名称, 名称字体,
                              New Rectangle(0, 缩放(40), ClientSize.Width, Math.Max(1, 名称高度)),
                              Color.Silver,
                              TextFormatFlags.NoPrefix Or TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis)
    End Sub

    Private Function 缩放(value As Integer) As Integer
        Return Math.Max(1, CInt(Math.Round(value * CDbl(DeviceDpi) / 96.0R)))
    End Function
End Class
