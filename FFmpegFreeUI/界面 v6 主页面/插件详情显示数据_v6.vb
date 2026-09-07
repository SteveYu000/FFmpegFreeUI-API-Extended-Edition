Option Strict On
Option Explicit On

''' <summary>插件管理页传递给详情视图的纯显示数据。</summary>
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
    Public Property 设置入口可用 As Boolean
End Class
