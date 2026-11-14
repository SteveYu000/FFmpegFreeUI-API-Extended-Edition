Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports FFmpegFreeUI.Ext.PluginSdk

' Official entry deliberately has no SDK types in its signatures or static fields.
Public NotInheritable Class Entry
    Public Shared ReadOnly Events As New List(Of String)
    Friend Shared ExtActive As Boolean
    Private Shared AddPanel As Object

    Public Shared Sub SetHost_AddCustomWinformPanel(callback As Object)
        AddPanel = callback
        Events.Add("callback:winforms")
    End Sub
    Public Shared Sub SetHost_AddCustomWpfPanel(callback As Object)
        Events.Add("callback:wpf")
    End Sub
    Public Shared Sub SetHost_AddMissionToQueueWithArgs(callback As Object)
        Events.Add("callback:args")
    End Sub
    Public Shared Sub SetHost_AddMissionToQueueWith3fuiFile(callback As Object)
        Events.Add("callback:preset")
    End Sub
    Public Shared Sub SetHost_MediaStreamVisualSelector(callback As Object)
        Events.Add("callback:streams")
    End Sub
    Public Shared Sub SetHost_SubscribeQueueEvents(callback As Object)
        Events.Add("callback:queue-events")
    End Sub

    Public Shared Sub Entry()
        Events.Add(If(ExtActive, "entry:ext", "entry:official"))
        If Environment.GetEnvironmentVariable("FFUI_LOADER_REQUIRE_SDK") = "1" Then RequireSdk()
        If Environment.GetEnvironmentVariable("FFUI_LOADER_SHOW_PAGE") <> "0" Then
            Dim page As New Panel With {.BackColor = Drawing.Color.FromArgb(36, 36, 36)}
            page.Controls.Add(New Label With {
                .Dock = DockStyle.Fill, .TextAlign = Drawing.ContentAlignment.MiddleCenter,
                .ForeColor = Drawing.Color.White,
                .Text = String.Join(Environment.NewLine, Events)})
            DirectCast(AddPanel, Action(Of String, Control)).Invoke("插件降级验证", page)
        End If
    End Sub

    <MethodImpl(MethodImplOptions.NoInlining)>
    Private Shared Sub RequireSdk()
        Events.Add(ExtFFmpegFreeUIPluginApi.Version.ToString())
    End Sub
End Class

Public NotInheritable Class ExtPlugin
    Implements IExtFFmpegFreeUIPlugin
    Public ReadOnly Property Id As String Implements IExtFFmpegFreeUIPlugin.Id
        Get
            Return "tests.plugin-loader-fallback"
        End Get
    End Property
    Public ReadOnly Property DisplayName As String Implements IExtFFmpegFreeUIPlugin.DisplayName
        Get
            Return "官方与 Ext 加载验证"
        End Get
    End Property
    Public Sub Initialize(host As IExtFFmpegFreeUIHost) Implements IExtFFmpegFreeUIPlugin.Initialize
        If Entry.Events.Count <> 6 Then Throw New InvalidOperationException("Official callbacks were not injected first")
        If Environment.GetEnvironmentVariable("FFUI_LOADER_FAIL_EXT") = "1" Then Throw New InvalidOperationException("Ext fixture initialization failure")
        Entry.ExtActive = True
        Entry.Events.Add("ext.initialize:" & host.ApiVersion.ToString())
    End Sub
End Class
