Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms
Imports FFmpegFreeUI.Ext.PluginSdk
Imports LakeUI

Friend Interface IExt参数面板状态接收器_v2
    Sub 通知状态已还原()
End Interface

''' <summary>Ext API v2.4 的页面入口和编码队列工具栏扩展实现。</summary>
Friend Module Ext插件页面与工具栏宿主_v2
    Private ReadOnly 同步锁 As New Object
    Private ReadOnly 页面目标表 As New Dictionary(Of String, 已注册页面目标)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly 工具栏目标表 As New Dictionary(Of String, 已注册工具栏目标)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly 页面扩展列表 As New List(Of 已注册页面扩展)
    Private ReadOnly 工具栏扩展列表 As New List(Of 已注册工具栏扩展)
    Private 页面目标下一序号 As Integer
    Private 工具栏目标下一序号 As Integer

    Friend Sub 注册页面目标(targetId As String,
                         displayName As String,
                         surfaceName As String,
                         tabListControl As Control,
                         targetPageObject As Object,
                         parameterSurfaceControl As Control,
                         configurePage As Action(Of Control))
        Dim id = 必需文本(targetId, NameOf(targetId))
        Dim tabList = TryCast(tabListControl, ModernTabListControl)
        Dim targetPage = TryCast(targetPageObject, ModernTabListControl.ModernTabPage)
        If tabList Is Nothing OrElse targetPage Is Nothing Then
            Throw New ArgumentException($"页面目标 {id} 没有提供有效的 ModernTabListControl 和 ModernTabPage")
        End If

        Dim area = 解析页面区域(surfaceName)
        Dim parameterSurface = TryCast(parameterSurfaceControl, Form_v6_参数面板)
        If area = ExtPluginPageEntryArea.ParameterPanelNavigation AndAlso parameterSurface Is Nothing Then
            Throw New ArgumentException($"参数面板页面目标 {id} 没有提供参数面板实例")
        End If

        Dim target As 已注册页面目标 = Nothing
        Dim registrations As List(Of 已注册页面扩展)
        SyncLock 同步锁
            If 页面目标表.TryGetValue(id, target) Then Exit Sub
            target = New 已注册页面目标 With {
                .TargetId = id,
                .DisplayName = If(displayName, "").Trim(),
                .Area = area,
                .RegistrationOrder = 页面目标下一序号,
                .TabList = tabList,
                .TargetPage = targetPage,
                .ParameterSurface = parameterSurface,
                .ConfigurePage = configurePage
            }
            页面目标下一序号 += 1
            页面目标表.Add(id, target)
            registrations = 页面扩展列表.
                Where(Function(item) String.Equals(item.Extension.TargetId, id, StringComparison.OrdinalIgnoreCase)).
                ToList()
        End SyncLock

        AddHandler tabList.Disposed, Sub() 移除页面目标(target)
        For Each registration In registrations
            在控件线程执行(tabList, Sub() 应用页面扩展(target, registration))
        Next
    End Sub

    Friend Sub 注册工具栏目标(targetId As String,
                           displayName As String,
                           toolbarControl As Control,
                           targetControl As Control,
                           refreshLayout As Action)
        Dim id = 必需文本(targetId, NameOf(targetId))
        If toolbarControl Is Nothing OrElse targetControl Is Nothing Then
            Throw New ArgumentException($"工具栏目标 {id} 没有提供有效控件")
        End If
        If targetControl.Parent IsNot toolbarControl Then
            Throw New InvalidOperationException($"工具栏目标 {id} 不在指定的工具栏容器中")
        End If

        Dim target As 已注册工具栏目标 = Nothing
        Dim registrations As List(Of 已注册工具栏扩展)
        SyncLock 同步锁
            If 工具栏目标表.TryGetValue(id, target) Then Exit Sub
            target = New 已注册工具栏目标 With {
                .TargetId = id,
                .DisplayName = If(displayName, "").Trim(),
                .RegistrationOrder = 工具栏目标下一序号,
                .Toolbar = toolbarControl,
                .TargetControl = targetControl,
                .RefreshLayout = refreshLayout
            }
            工具栏目标下一序号 += 1
            工具栏目标表.Add(id, target)
            registrations = 工具栏扩展列表.
                Where(Function(item) String.Equals(item.Extension.TargetId, id, StringComparison.OrdinalIgnoreCase)).
                ToList()
        End SyncLock

        AddHandler targetControl.Disposed, Sub() 移除工具栏目标(target)
        For Each registration In registrations
            在控件线程执行(toolbarControl, Sub() 应用工具栏扩展(target, registration))
        Next
    End Sub

    Friend Function 获取页面目标() As IReadOnlyCollection(Of ExtPluginPageTargetDescriptor)
        SyncLock 同步锁
            Return 页面目标表.Values.
                OrderBy(Function(item) item.Area).
                ThenBy(Function(item) item.RegistrationOrder).
                Select(Function(item) New ExtPluginPageTargetDescriptor(item.TargetId, item.DisplayName, item.Area)).
                ToArray()
        End SyncLock
    End Function

    Friend Function 获取工具栏目标() As IReadOnlyCollection(Of ExtPluginToolbarTargetDescriptor)
        SyncLock 同步锁
            Return 工具栏目标表.Values.
                OrderBy(Function(item) item.RegistrationOrder).
                Select(Function(item) New ExtPluginToolbarTargetDescriptor(item.TargetId, item.DisplayName)).
                ToArray()
        End SyncLock
    End Function

    Friend Function 注册页面扩展(pluginId As String, extension As ExtPluginPageExtension) As IDisposable
        If extension Is Nothing Then Throw New ArgumentNullException(NameOf(extension))
        Dim id = 必需文本(extension.Id, NameOf(extension.Id))
        Dim targetId = 必需文本(extension.TargetId, NameOf(extension.TargetId))
        If String.IsNullOrWhiteSpace(extension.Title) Then Throw New ArgumentException("插件页面标题不能为空")
        If extension.CreatePage Is Nothing Then Throw New ArgumentException("插件页面工厂不能为空")
        If Not [Enum].IsDefined(GetType(ExtPluginRelativePosition), extension.Position) Then
            Throw New ArgumentOutOfRangeException(NameOf(extension.Position))
        End If
        extension.Id = id
        extension.TargetId = targetId
        extension.Title = extension.Title.Trim()

        Dim registration As New 已注册页面扩展 With {.PluginId = pluginId, .Extension = extension}
        Dim target As 已注册页面目标 = Nothing
        SyncLock 同步锁
            If 页面扩展列表.Any(Function(item) String.Equals(item.PluginId, pluginId, StringComparison.OrdinalIgnoreCase) AndAlso
                                               String.Equals(item.Extension.Id, id, StringComparison.OrdinalIgnoreCase)) Then
                Throw New InvalidOperationException($"插件 {pluginId} 已注册页面入口 {id}")
            End If
            If Not 页面目标表.TryGetValue(targetId, target) Then
                Throw New InvalidOperationException($"宿主不提供页面目标 {targetId}；请先检查 PageEntries.AvailableTargets")
            End If
            页面扩展列表.Add(registration)
        End SyncLock

        Try
            在控件线程执行(target.TabList, Sub() 应用页面扩展(target, registration))
            Return New 界面扩展注销句柄(Sub() 注销页面扩展(registration))
        Catch
            SyncLock 同步锁
                页面扩展列表.Remove(registration)
            End SyncLock
            Throw
        End Try
    End Function

    Friend Function 注册工具栏扩展(pluginId As String, extension As ExtPluginToolbarControlExtension) As IDisposable
        If extension Is Nothing Then Throw New ArgumentNullException(NameOf(extension))
        Dim id = 必需文本(extension.Id, NameOf(extension.Id))
        Dim targetId = 必需文本(extension.TargetId, NameOf(extension.TargetId))
        If extension.CreateControl Is Nothing Then Throw New ArgumentException("插件工具栏控件工厂不能为空")
        If Not [Enum].IsDefined(GetType(ExtPluginRelativePosition), extension.Position) Then
            Throw New ArgumentOutOfRangeException(NameOf(extension.Position))
        End If
        extension.Id = id
        extension.TargetId = targetId

        Dim registration As New 已注册工具栏扩展 With {.PluginId = pluginId, .Extension = extension}
        Dim target As 已注册工具栏目标 = Nothing
        SyncLock 同步锁
            If 工具栏扩展列表.Any(Function(item) String.Equals(item.PluginId, pluginId, StringComparison.OrdinalIgnoreCase) AndAlso
                                                 String.Equals(item.Extension.Id, id, StringComparison.OrdinalIgnoreCase)) Then
                Throw New InvalidOperationException($"插件 {pluginId} 已注册工具栏控件 {id}")
            End If
            If Not 工具栏目标表.TryGetValue(targetId, target) Then
                Throw New InvalidOperationException($"宿主不提供工具栏目标 {targetId}；请先检查 EncodingQueueToolbar.AvailableTargets")
            End If
            工具栏扩展列表.Add(registration)
        End SyncLock

        Try
            在控件线程执行(target.Toolbar, Sub() 应用工具栏扩展(target, registration))
            Return New 界面扩展注销句柄(Sub() 注销工具栏扩展(registration))
        Catch
            SyncLock 同步锁
                工具栏扩展列表.Remove(registration)
            End SyncLock
            Throw
        End Try
    End Function

    Private Sub 应用页面扩展(target As 已注册页面目标, registration As 已注册页面扩展)
        If target.TabList.IsDisposed Then Exit Sub
        Dim key = 注册键(registration.PluginId, registration.Extension.Id)
        SyncLock 同步锁
            If target.Applied.ContainsKey(key) Then Exit Sub
        End SyncLock

        Dim context As New 插件页面上下文(registration, target)
        Dim pageControl As Control = Nothing
        Dim applied As 已应用页面扩展 = Nothing
        Dim stateRegistered As Boolean
        Try
            pageControl = registration.Extension.CreatePage.Invoke(context)
            If pageControl Is Nothing OrElse pageControl.IsDisposed Then
                Throw New InvalidOperationException($"插件页面 {registration.Extension.Id} 的工厂没有返回有效控件")
            End If
            If pageControl.Parent IsNot Nothing Then
                Throw New InvalidOperationException("插件页面工厂必须返回尚未加入其他容器的新控件")
            End If
            Dim pageForm = TryCast(pageControl, Form)
            If pageForm IsNot Nothing AndAlso pageForm.TopLevel Then
                pageForm.TopLevel = False
                pageForm.FormBorderStyle = FormBorderStyle.None
            End If
            pageControl.Dock = DockStyle.Fill
            target.ConfigurePage?.Invoke(pageControl)

            Dim page As New ModernTabListControl.ModernTabPage With {
                .Text = registration.Extension.Title.Trim(),
                .BoundControl = pageControl
            }
            applied = New 已应用页面扩展 With {
                .Registration = registration,
                .Context = context,
                .Page = page,
                .Control = pageControl
            }
            context.PageControl = pageControl
            SyncLock 同步锁
                target.Applied.Add(key, applied)
            End SyncLock
            If target.ParameterSurface IsNot Nothing Then
                Ext插件扩展宿主_v2.注册布局页面状态上下文(target.ParameterSurface, context)
                stateRegistered = True
            End If
            重排页面扩展(target, Nothing)
        Catch
            If applied IsNot Nothing Then
                SyncLock 同步锁
                    target.Applied.Remove(key)
                End SyncLock
                If stateRegistered Then
                    Ext插件扩展宿主_v2.注销布局页面状态上下文(target.ParameterSurface, context)
                End If
                target.TabList.Items.Remove(applied.Page)
                清理页面扩展(applied)
            End If
            pageControl?.Dispose()
            Throw
        End Try
    End Sub

    Private Sub 重排页面扩展(target As 已注册页面目标,
                         Optional removedPage As ModernTabListControl.ModernTabPage = Nothing)
        Dim selectedPage As ModernTabListControl.ModernTabPage = Nothing
        If target.TabList.SelectedIndex >= 0 AndAlso target.TabList.SelectedIndex < target.TabList.Items.Count Then
            selectedPage = target.TabList.Items(target.TabList.SelectedIndex)
        End If

        Dim applied As List(Of 已应用页面扩展)
        SyncLock 同步锁
            applied = target.Applied.Values.
                OrderBy(Function(item) item.Registration.Extension.Order).
                ThenBy(Function(item) item.Registration.PluginId, StringComparer.OrdinalIgnoreCase).
                ThenBy(Function(item) item.Registration.Extension.Id, StringComparer.OrdinalIgnoreCase).
                ToList()
        End SyncLock

        target.TabList.SuspendLayout()
        Try
            For Each item In applied
                target.TabList.Items.Remove(item.Page)
            Next
            If removedPage IsNot Nothing Then target.TabList.Items.Remove(removedPage)

            Dim targetIndex = target.TabList.Items.IndexOf(target.TargetPage)
            If targetIndex < 0 Then Throw New InvalidOperationException($"页面目标 {target.TargetId} 已从导航中移除")

            For Each item In applied.Where(
                Function(value) value.Registration.Extension.Position = ExtPluginRelativePosition.Before)
                target.TabList.Items.Insert(targetIndex, item.Page)
                targetIndex += 1
            Next

            Dim afterIndex = target.TabList.Items.IndexOf(target.TargetPage) + 1
            For Each item In applied.Where(
                Function(value) value.Registration.Extension.Position = ExtPluginRelativePosition.After)
                target.TabList.Items.Insert(afterIndex, item.Page)
                afterIndex += 1
            Next

            Dim selectedIndex = If(selectedPage Is Nothing, -1, target.TabList.Items.IndexOf(selectedPage))
            If selectedIndex < 0 Then selectedIndex = target.TabList.Items.IndexOf(target.TargetPage)
            If selectedIndex >= 0 Then target.TabList.SelectedIndex = selectedIndex
            target.TabList.Invalidate()
        Finally
            target.TabList.ResumeLayout(True)
        End Try
    End Sub

    Private Sub 注销页面扩展(registration As 已注册页面扩展)
        Dim removal As 已应用页面扩展 = Nothing
        Dim target As 已注册页面目标 = Nothing
        Dim key = 注册键(registration.PluginId, registration.Extension.Id)
        SyncLock 同步锁
            页面扩展列表.Remove(registration)
            If 页面目标表.TryGetValue(registration.Extension.TargetId, target) Then
                target.Applied.TryGetValue(key, removal)
                If removal IsNot Nothing Then target.Applied.Remove(key)
            End If
        End SyncLock
        If removal Is Nothing OrElse target Is Nothing Then Exit Sub

        If target.ParameterSurface IsNot Nothing Then
            Ext插件扩展宿主_v2.注销布局页面状态上下文(target.ParameterSurface, removal.Context)
        End If
        在控件线程执行(
            target.TabList,
            Sub()
                重排页面扩展(target, removal.Page)
                清理页面扩展(removal)
                removal.Control?.Dispose()
            End Sub)
    End Sub

    Private Sub 应用工具栏扩展(target As 已注册工具栏目标, registration As 已注册工具栏扩展)
        If target.Toolbar.IsDisposed OrElse target.TargetControl.IsDisposed Then Exit Sub
        Dim key = 注册键(registration.PluginId, registration.Extension.Id)
        SyncLock 同步锁
            If target.Applied.ContainsKey(key) Then Exit Sub
        End SyncLock

        Dim context As New 插件工具栏上下文(registration, target)
        Dim control As Control = Nothing
        Dim applied As 已应用工具栏扩展 = Nothing
        Try
            control = registration.Extension.CreateControl.Invoke(context)
            If control Is Nothing OrElse control.IsDisposed Then
                Throw New InvalidOperationException($"工具栏扩展 {registration.Extension.Id} 的工厂没有返回有效控件")
            End If
            Dim form = TryCast(control, Form)
            If form IsNot Nothing AndAlso form.TopLevel Then
                Throw New InvalidOperationException("工具栏扩展必须返回普通 Control 或非顶级窗体")
            End If
            If control.Parent IsNot Nothing Then
                Throw New InvalidOperationException("工具栏扩展工厂必须返回尚未加入其他容器的新控件")
            End If
            control.Dock = DockStyle.None
            control.Margin = Padding.Empty
            control.Height = context.RecommendedHeight
            context.ExtensionControl = control

            applied = New 已应用工具栏扩展 With {
                .Registration = registration,
                .Context = context,
                .Control = control
            }
            SyncLock 同步锁
                target.Applied.Add(key, applied)
            End SyncLock
            Dim slot = 获取或创建工具栏插入槽(target, registration.Extension.Position)
            slot.Controls.Add(control)
            重排工具栏插入槽(target, registration.Extension.Position)
            target.RefreshLayout?.Invoke()
        Catch
            If applied IsNot Nothing Then
                SyncLock 同步锁
                    target.Applied.Remove(key)
                End SyncLock
                control?.Parent?.Controls.Remove(control)
                清理工具栏扩展(applied)
                重排工具栏插入槽(target, registration.Extension.Position)
                清理空工具栏插入槽(target, registration.Extension.Position)
                target.RefreshLayout?.Invoke()
            End If
            control?.Dispose()
            Throw
        End Try
    End Sub

    Private Function 获取或创建工具栏插入槽(target As 已注册工具栏目标,
                                      position As ExtPluginRelativePosition) As FlowLayoutPanel
        Dim current = If(position = ExtPluginRelativePosition.Before, target.BeforeSlot, target.AfterSlot)
        If current IsNot Nothing AndAlso Not current.IsDisposed Then Return current

        Dim slot As New FlowLayoutPanel With {
            .Name = $"ExtToolbarSlot_{target.TargetId.Replace("."c, "_"c)}_{position}",
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BackColor = Color.Transparent,
            .Dock = target.TargetControl.Dock,
            .FlowDirection = FlowDirection.LeftToRight,
            .Margin = Padding.Empty,
            .Padding = Padding.Empty,
            .WrapContents = False
        }
        target.Toolbar.Controls.Add(slot)
        If position = ExtPluginRelativePosition.Before Then
            target.BeforeSlot = slot
        Else
            target.AfterSlot = slot
        End If
        重排工具栏原生插入槽(target.Toolbar)
        AddHandler slot.SizeChanged, Sub() target.RefreshLayout?.Invoke()
        Return slot
    End Function

    ''' <summary>
    ''' 统一重建 DockStyle.Left 的 Z 顺序，避免靠近最右侧按钮的插入槽抢占 DockStyle.Fill 状态区。
    ''' WinForms 的左停靠视觉顺序与 Controls 索引相反，因此非左停靠控件保持在低索引。
    ''' </summary>
    Private Sub 重排工具栏原生插入槽(toolbar As Control)
        If toolbar Is Nothing OrElse toolbar.IsDisposed Then Exit Sub

        Dim targets As List(Of 已注册工具栏目标)
        SyncLock 同步锁
            targets = 工具栏目标表.Values.
                Where(Function(item) item.Toolbar Is toolbar).
                ToList()
        End SyncLock
        Dim slots = targets.
            SelectMany(Function(item) New Control() {item.BeforeSlot, item.AfterSlot}).
            Where(Function(control) control IsNot Nothing AndAlso Not control.IsDisposed).
            ToList()
        Dim nativeLeft = toolbar.Controls.Cast(Of Control)().
            Where(Function(control) control.Dock = DockStyle.Left AndAlso
                                          Not slots.Any(Function(slot) slot Is control)).
            OrderByDescending(Function(control) toolbar.Controls.GetChildIndex(control)).
            ToList()
        Dim desiredLeft As New List(Of Control)
        For Each nativeControl In nativeLeft
            Dim nativeTarget = targets.FirstOrDefault(Function(item) item.TargetControl Is nativeControl)
            If nativeTarget?.BeforeSlot IsNot Nothing AndAlso Not nativeTarget.BeforeSlot.IsDisposed Then
                desiredLeft.Add(nativeTarget.BeforeSlot)
            End If
            desiredLeft.Add(nativeControl)
            If nativeTarget?.AfterSlot IsNot Nothing AndAlso Not nativeTarget.AfterSlot.IsDisposed Then
                desiredLeft.Add(nativeTarget.AfterSlot)
            End If
        Next
        Dim nonLeft = toolbar.Controls.Cast(Of Control)().
            Where(Function(control) control.Dock <> DockStyle.Left).
            OrderBy(Function(control) toolbar.Controls.GetChildIndex(control)).
            ToList()

        toolbar.SuspendLayout()
        Try
            For index = 0 To nonLeft.Count - 1
                toolbar.Controls.SetChildIndex(nonLeft(index), index)
            Next
            For index = 0 To desiredLeft.Count - 1
                Dim desiredIndex = nonLeft.Count + desiredLeft.Count - index - 1
                toolbar.Controls.SetChildIndex(desiredLeft(index), desiredIndex)
            Next
        Finally
            toolbar.ResumeLayout(True)
        End Try
    End Sub

    Private Sub 重排工具栏插入槽(target As 已注册工具栏目标,
                             position As ExtPluginRelativePosition)
        Dim slot = If(position = ExtPluginRelativePosition.Before, target.BeforeSlot, target.AfterSlot)
        If slot Is Nothing OrElse slot.IsDisposed Then Exit Sub
        Dim controls As List(Of Control)
        SyncLock 同步锁
            controls = target.Applied.Values.
                Where(Function(item) item.Registration.Extension.Position = position).
                OrderBy(Function(item) item.Registration.Extension.Order).
                ThenBy(Function(item) item.Registration.PluginId, StringComparer.OrdinalIgnoreCase).
                ThenBy(Function(item) item.Registration.Extension.Id, StringComparer.OrdinalIgnoreCase).
                Select(Function(item) item.Control).
                ToList()
        End SyncLock

        slot.SuspendLayout()
        Try
            For index = 0 To controls.Count - 1
                Dim control = controls(index)
                If control.Parent IsNot slot Then slot.Controls.Add(control)
                slot.Controls.SetChildIndex(control, index)
            Next
        Finally
            slot.ResumeLayout(True)
        End Try
    End Sub

    Private Sub 注销工具栏扩展(registration As 已注册工具栏扩展)
        Dim removal As 已应用工具栏扩展 = Nothing
        Dim target As 已注册工具栏目标 = Nothing
        Dim key = 注册键(registration.PluginId, registration.Extension.Id)
        SyncLock 同步锁
            工具栏扩展列表.Remove(registration)
            If 工具栏目标表.TryGetValue(registration.Extension.TargetId, target) Then
                target.Applied.TryGetValue(key, removal)
                If removal IsNot Nothing Then target.Applied.Remove(key)
            End If
        End SyncLock
        If removal Is Nothing OrElse target Is Nothing Then Exit Sub

        在控件线程执行(
            target.Toolbar,
            Sub()
                清理工具栏扩展(removal)
                removal.Control?.Dispose()
                重排工具栏插入槽(target, registration.Extension.Position)
                清理空工具栏插入槽(target, registration.Extension.Position)
                target.RefreshLayout?.Invoke()
            End Sub)
    End Sub

    Private Sub 清理空工具栏插入槽(target As 已注册工具栏目标,
                               position As ExtPluginRelativePosition)
        Dim slot = If(position = ExtPluginRelativePosition.Before, target.BeforeSlot, target.AfterSlot)
        If slot Is Nothing OrElse slot.IsDisposed OrElse slot.Controls.Count > 0 Then Exit Sub
        slot.Dispose()
        If position = ExtPluginRelativePosition.Before Then
            target.BeforeSlot = Nothing
        Else
            target.AfterSlot = Nothing
        End If
        重排工具栏原生插入槽(target.Toolbar)
    End Sub

    Private Sub 移除页面目标(target As 已注册页面目标)
        Dim applied As List(Of 已应用页面扩展)
        SyncLock 同步锁
            页面目标表.Remove(target.TargetId)
            applied = target.Applied.Values.ToList()
            target.Applied.Clear()
        End SyncLock
        For Each item In applied
            If target.ParameterSurface IsNot Nothing Then
                Ext插件扩展宿主_v2.注销布局页面状态上下文(target.ParameterSurface, item.Context)
            End If
            清理页面扩展(item)
            item.Control?.Dispose()
        Next
    End Sub

    Private Sub 移除工具栏目标(target As 已注册工具栏目标)
        Dim applied As List(Of 已应用工具栏扩展)
        SyncLock 同步锁
            工具栏目标表.Remove(target.TargetId)
            applied = target.Applied.Values.ToList()
            target.Applied.Clear()
        End SyncLock
        For Each item In applied
            清理工具栏扩展(item)
            item.Control?.Dispose()
        Next
        target.BeforeSlot?.Dispose()
        target.AfterSlot?.Dispose()
        重排工具栏原生插入槽(target.Toolbar)
    End Sub

    Private Sub 清理页面扩展(applied As 已应用页面扩展)
        If applied Is Nothing OrElse applied.CleanupCalled Then Exit Sub
        applied.CleanupCalled = True
        Try
            applied.Registration.Extension.Cleanup?.Invoke(applied.Context)
        Catch ex As Exception
            Debug.WriteLine($"[FFmpegFreeUI Plugin/Warning] {applied.Registration.PluginId}: 清理布局页面失败：{ex}")
        End Try
    End Sub

    Private Sub 清理工具栏扩展(applied As 已应用工具栏扩展)
        If applied Is Nothing OrElse applied.CleanupCalled Then Exit Sub
        applied.CleanupCalled = True
        Try
            applied.Registration.Extension.Cleanup?.Invoke(applied.Context)
        Catch ex As Exception
            Debug.WriteLine($"[FFmpegFreeUI Plugin/Warning] {applied.Registration.PluginId}: 清理工具栏扩展失败：{ex}")
        End Try
    End Sub

    Private Function 解析页面区域(value As String) As ExtPluginPageEntryArea
        Select Case If(value, "").Trim().ToLowerInvariant()
            Case "main"
                Return ExtPluginPageEntryArea.MainNavigation
            Case "parameters"
                Return ExtPluginPageEntryArea.ParameterPanelNavigation
            Case Else
                Throw New ArgumentException($"未知的插件页面区域：{value}")
        End Select
    End Function

    Private Function 必需文本(value As String, parameterName As String) As String
        Dim result = If(value, "").Trim()
        If result = "" Then Throw New ArgumentException("ID 不能为空", parameterName)
        Return result
    End Function

    Private Function 注册键(pluginId As String, extensionId As String) As String
        Return pluginId & ":" & extensionId
    End Function

    Private Sub 在控件线程执行(control As Control, action As Action)
        If control Is Nothing OrElse control.IsDisposed OrElse action Is Nothing Then Exit Sub
        If control.IsHandleCreated AndAlso control.InvokeRequired Then
            control.Invoke(action)
        Else
            action()
        End If
    End Sub

    Private NotInheritable Class 已注册页面目标
        Public Property TargetId As String = ""
        Public Property DisplayName As String = ""
        Public Property Area As ExtPluginPageEntryArea
        Public Property RegistrationOrder As Integer
        Public Property TabList As ModernTabListControl
        Public Property TargetPage As ModernTabListControl.ModernTabPage
        Public Property ParameterSurface As Form_v6_参数面板
        Public Property ConfigurePage As Action(Of Control)
        Public ReadOnly Applied As New Dictionary(Of String, 已应用页面扩展)(StringComparer.OrdinalIgnoreCase)
    End Class

    Private NotInheritable Class 已注册工具栏目标
        Public Property TargetId As String = ""
        Public Property DisplayName As String = ""
        Public Property RegistrationOrder As Integer
        Public Property Toolbar As Control
        Public Property TargetControl As Control
        Public Property RefreshLayout As Action
        Public Property BeforeSlot As FlowLayoutPanel
        Public Property AfterSlot As FlowLayoutPanel
        Public ReadOnly Applied As New Dictionary(Of String, 已应用工具栏扩展)(StringComparer.OrdinalIgnoreCase)
    End Class

    Private NotInheritable Class 已注册页面扩展
        Public Property PluginId As String = ""
        Public Property Extension As ExtPluginPageExtension
    End Class

    Private NotInheritable Class 已注册工具栏扩展
        Public Property PluginId As String = ""
        Public Property Extension As ExtPluginToolbarControlExtension
    End Class

    Private NotInheritable Class 已应用页面扩展
        Public Property Registration As 已注册页面扩展
        Public Property Context As 插件页面上下文
        Public Property Page As ModernTabListControl.ModernTabPage
        Public Property Control As Control
        Public Property CleanupCalled As Boolean
    End Class

    Private NotInheritable Class 已应用工具栏扩展
        Public Property Registration As 已注册工具栏扩展
        Public Property Context As 插件工具栏上下文
        Public Property Control As Control
        Public Property CleanupCalled As Boolean
    End Class

    Private NotInheritable Class 插件页面上下文
        Implements IExtPluginPageContext, IExt参数面板状态接收器_v2

        Private ReadOnly _target As 已注册页面目标
        Private _sessionStateJson As String = "{}"

        Public Sub New(registration As 已注册页面扩展, target As 已注册页面目标)
            PluginId = registration.PluginId
            ExtensionId = registration.Extension.Id
            TargetId = target.TargetId
            Area = target.Area
            Position = registration.Extension.Position
            _target = target
        End Sub

        Public Property PageControl As Control Implements IExtPluginPageContext.PageControl
        Public ReadOnly Property PluginId As String Implements IExtPluginPageContext.PluginId
        Public ReadOnly Property ExtensionId As String Implements IExtPluginPageContext.ExtensionId
        Public ReadOnly Property TargetId As String Implements IExtPluginPageContext.TargetId
        Public ReadOnly Property Area As ExtPluginPageEntryArea Implements IExtPluginPageContext.Area
        Public ReadOnly Property Position As ExtPluginRelativePosition Implements IExtPluginPageContext.Position
        Public ReadOnly Property SupportsPresetState As Boolean Implements IExtPluginPageContext.SupportsPresetState
            Get
                Return _target.ParameterSurface IsNot Nothing
            End Get
        End Property

        Public Property StateJson As String Implements IExtPluginPageContext.StateJson
            Get
                If SupportsPresetState Then
                    Return Ext插件扩展宿主_v2.读取布局页面状态(_target.ParameterSurface, PluginId)
                End If
                Return _sessionStateJson
            End Get
            Set(value As String)
                If SupportsPresetState Then
                    Ext插件扩展宿主_v2.写入布局页面状态(_target.ParameterSurface, PluginId, value)
                Else
                    _sessionStateJson = Ext插件扩展宿主_v2.规范化布局状态Json(value)
                End If
            End Set
        End Property

        Public Event StateRestored As EventHandler Implements IExtPluginPageContext.StateRestored

        Public Sub RequestParameterRefresh() Implements IExtPluginPageContext.RequestParameterRefresh
            If _target.ParameterSurface Is Nothing Then Exit Sub
            在控件线程执行(_target.TabList, Sub() _target.ParameterSurface.请求刷新参数状态())
        End Sub

        Public Sub 通知状态已还原() Implements IExt参数面板状态接收器_v2.通知状态已还原
            在控件线程执行(_target.TabList, Sub() RaiseEvent StateRestored(Me, EventArgs.Empty))
        End Sub
    End Class

    Private NotInheritable Class 插件工具栏上下文
        Implements IExtPluginToolbarContext

        Private ReadOnly _target As 已注册工具栏目标

        Public Sub New(registration As 已注册工具栏扩展, target As 已注册工具栏目标)
            PluginId = registration.PluginId
            ExtensionId = registration.Extension.Id
            TargetId = target.TargetId
            Position = registration.Extension.Position
            _target = target
        End Sub

        Public ReadOnly Property PluginId As String Implements IExtPluginToolbarContext.PluginId
        Public ReadOnly Property ExtensionId As String Implements IExtPluginToolbarContext.ExtensionId
        Public ReadOnly Property TargetId As String Implements IExtPluginToolbarContext.TargetId
        Public ReadOnly Property Position As ExtPluginRelativePosition Implements IExtPluginToolbarContext.Position
        Public Property ExtensionControl As Control Implements IExtPluginToolbarContext.ExtensionControl
        Public ReadOnly Property ToolbarControl As Control Implements IExtPluginToolbarContext.ToolbarControl
            Get
                Return _target.Toolbar
            End Get
        End Property
        Public ReadOnly Property TargetControl As Control Implements IExtPluginToolbarContext.TargetControl
            Get
                Return _target.TargetControl
            End Get
        End Property
        Public ReadOnly Property RecommendedHeight As Integer Implements IExtPluginToolbarContext.RecommendedHeight
            Get
                Return Math.Max(1, _target.TargetControl.Height)
            End Get
        End Property
        Public ReadOnly Property DeviceDpi As Integer Implements IExtPluginToolbarContext.DeviceDpi
            Get
                Return _target.TargetControl.DeviceDpi
            End Get
        End Property
    End Class

    Private NotInheritable Class 界面扩展注销句柄
        Implements IDisposable

        Private _disposeAction As Action

        Public Sub New(disposeAction As Action)
            _disposeAction = disposeAction
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dim action = Threading.Interlocked.Exchange(_disposeAction, Nothing)
            action?.Invoke()
        End Sub
    End Class
End Module

Friend NotInheritable Class Ext插件页面入口注册表_v2
    Implements IExtPluginPageEntryRegistry

    Private ReadOnly _pluginId As String
    Private ReadOnly _track As Action(Of IDisposable)

    Public Sub New(pluginId As String, track As Action(Of IDisposable))
        _pluginId = pluginId
        _track = track
    End Sub

    Public ReadOnly Property AvailableTargets As IReadOnlyCollection(Of ExtPluginPageTargetDescriptor) Implements IExtPluginPageEntryRegistry.AvailableTargets
        Get
            Return Ext插件页面与工具栏宿主_v2.获取页面目标()
        End Get
    End Property

    Public Function RegisterPage(extension As ExtPluginPageExtension) As IDisposable Implements IExtPluginPageEntryRegistry.RegisterPage
        Dim registration = Ext插件页面与工具栏宿主_v2.注册页面扩展(_pluginId, extension)
        _track.Invoke(registration)
        Return registration
    End Function
End Class

Friend NotInheritable Class Ext编码队列工具栏注册表_v2
    Implements IExtPluginEncodingQueueToolbarRegistry

    Private ReadOnly _pluginId As String
    Private ReadOnly _track As Action(Of IDisposable)

    Public Sub New(pluginId As String, track As Action(Of IDisposable))
        _pluginId = pluginId
        _track = track
    End Sub

    Public ReadOnly Property AvailableTargets As IReadOnlyCollection(Of ExtPluginToolbarTargetDescriptor) Implements IExtPluginEncodingQueueToolbarRegistry.AvailableTargets
        Get
            Return Ext插件页面与工具栏宿主_v2.获取工具栏目标()
        End Get
    End Property

    Public Function RegisterControl(extension As ExtPluginToolbarControlExtension) As IDisposable Implements IExtPluginEncodingQueueToolbarRegistry.RegisterControl
        Dim registration = Ext插件页面与工具栏宿主_v2.注册工具栏扩展(_pluginId, extension)
        _track.Invoke(registration)
        Return registration
    End Function
End Class
