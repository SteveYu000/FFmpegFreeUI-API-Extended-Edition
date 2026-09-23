Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports LakeUI

''' <summary>下载扩展版 Release，并在正常退出后交给独立的 PowerShell 进程替换文件。</summary>
Friend NotInheritable Class 网络功能_v6_自动更新

    Friend NotInheritable Class 更新文件
        Friend ReadOnly Property 文件名 As String
        Friend ReadOnly Property 下载地址 As Uri
        Friend ReadOnly Property 长度 As Long
        Friend ReadOnly Property SHA256 As String

        Friend Sub New(文件名 As String, 下载地址 As Uri, 长度 As Long, SHA256 As String)
            Me.文件名 = 文件名
            Me.下载地址 = 下载地址
            Me.长度 = 长度
            Me.SHA256 = SHA256
        End Sub
    End Class

    Friend NotInheritable Class 待安装更新
        Friend ReadOnly Property 版本号 As String
        Friend ReadOnly Property 暂存目录 As String
        Friend ReadOnly Property 文件列表 As IReadOnlyList(Of 更新文件)

        Friend Sub New(版本号 As String, 暂存目录 As String, 文件列表 As IReadOnlyList(Of 更新文件))
            Me.版本号 = 版本号
            Me.暂存目录 = 暂存目录
            Me.文件列表 = 文件列表
        End Sub
    End Class

    Private Shared ReadOnly HTTP As New HttpClient With {.Timeout = Timeout.InfiniteTimeSpan}
    Private Shared ReadOnly 必需文件名 As String() = {
        "FFmpegFreeUI.exe",
        "FFmpegFreeUI.Ext.PluginHost.dll",
        "FFmpegFreeUI.Ext.PluginSdk.dll"
    }
    Private Shared 下载取消源 As CancellationTokenSource
    Private Shared 下载进行中 As Boolean
    Private Shared 已准备更新 As 待安装更新

    Private Sub New()
    End Sub

    Friend Shared ReadOnly Property 正在下载 As Boolean
        Get
            Return 下载进行中
        End Get
    End Property

    Friend Shared ReadOnly Property 有待安装更新 As Boolean
        Get
            Return 已准备更新 IsNot Nothing
        End Get
    End Property

    Friend Shared ReadOnly Property 应保持卡片状态 As Boolean
        Get
            Return 下载进行中 OrElse 已准备更新 IsNot Nothing
        End Get
    End Property

    Friend Shared Async Sub 开始下载(云端版本号 As String)
        If 下载进行中 OrElse 已准备更新 IsNot Nothing Then Return
        下载进行中 = True
        下载取消源 = New CancellationTokenSource(TimeSpan.FromMinutes(30))
        Dim 暂存目录 = Path.Combine(Application.StartupPath, ".update-staging", Guid.NewGuid().ToString("N"))
        Dim 进度 As New Progress(Of Integer)(
            Sub(百分比)
                If Not 下载进行中 OrElse 已准备更新 IsNot Nothing OrElse
                   FormMain_v6.IsDisposed OrElse FormMain_v6.Disposing Then Return
                Form_v6_起始页面.显示更新下载状态(
                    "正在下载更新",
                    $"{百分比}% · {云端版本号}",
                    Color.DeepSkyBlue,
                    True)
            End Sub)

        Form_v6_起始页面.显示更新下载状态("正在准备下载", 云端版本号, Color.DeepSkyBlue, True)
        Try
            Dim 文件列表 = Await 获取发行文件Async(云端版本号, 下载取消源.Token)
            Dim 更新 = Await Task.Run(
                Async Function()
                    Return Await 下载并校验Async(云端版本号, 暂存目录, 文件列表, 进度, 下载取消源.Token)
                End Function)
            已准备更新 = 更新
            下载进行中 = False
            Form_v6_起始页面.显示更新下载状态(
                "更新已下载",
                "关闭程序时安装 " & 云端版本号,
                Color.LimeGreen,
                True)
        Catch ex As OperationCanceledException
            下载进行中 = False
            安全删除暂存目录(暂存目录)
            If Not FormMain_v6.IsDisposed AndAlso Not FormMain_v6.Disposing Then
                Form_v6_起始页面.显示更新下载状态("下载已中断", "点击重试", Color.Orange, True)
            End If
        Catch ex As Exception
            下载进行中 = False
            安全删除暂存目录(暂存目录)
            Debug.WriteLine($"下载更新失败：{ex}")
            If Not FormMain_v6.IsDisposed AndAlso Not FormMain_v6.Disposing Then
                Form_v6_起始页面.显示更新下载状态("下载失败", "点击重试", Color.Orange, True)
                ExOverlayMsgBox(FormMain_v6, ex.Message, MsgBoxStyle.Exclamation, "更新下载失败")
            End If
        Finally
            下载进行中 = False
            下载取消源.Dispose()
            下载取消源 = Nothing
        End Try
    End Sub

    Friend Shared Sub 退出时取消下载()
        下载取消源?.Cancel()
    End Sub

    Friend Shared Sub 启动已准备的更新助手(重启应用 As Boolean)
        Dim 更新 = 已准备更新
        If 更新 Is Nothing Then Return
        Dim 脚本 = 创建安装脚本(更新, Application.StartupPath, Environment.ProcessId, 重启应用)
        FormMain_v6.启动隐藏PowerShell助手(脚本)
    End Sub

    Friend Shared Sub 检查上次更新结果()
        Dim 结果路径 = Path.Combine(Application.StartupPath, ".update-result.json")
        If Not File.Exists(结果路径) Then Return
        Try
            Using 文档 = JsonDocument.Parse(File.ReadAllText(结果路径, Encoding.UTF8))
                Dim 根 = 文档.RootElement
                Dim 状态 = 根.GetProperty("status").GetString()
                Dim 版本 = 根.GetProperty("version").GetString()
                Dim 详情 = 根.GetProperty("detail").GetString()
                If String.Equals(状态, "success", StringComparison.Ordinal) Then
                    ExOverlayMsgBox(FormMain_v6, $"已更新到 {版本}。", MsgBoxStyle.Information, "更新完成")
                Else
                    ExOverlayMsgBox(FormMain_v6, $"更新 {版本} 失败。{vbCrLf}{vbCrLf}{详情}", MsgBoxStyle.Exclamation, "更新失败")
                End If
            End Using
            File.Delete(结果路径)
        Catch ex As Exception
            Debug.WriteLine($"读取更新结果失败：{ex.Message}")
        End Try
    End Sub

    Private Shared Async Function 获取发行文件Async(云端版本号 As String, cancellationToken As CancellationToken) As Task(Of IReadOnlyList(Of 更新文件))
        Dim 地址 = $"https://api.github.com/repos/{网络功能_v6_软件版本检查.GitHub仓库拥有者}/{网络功能_v6_软件版本检查.GitHub仓库名称}/releases/tags/{Uri.EscapeDataString(云端版本号)}"
        Using request As New HttpRequestMessage(HttpMethod.Get, 地址)
            request.Headers.UserAgent.ParseAdd("FFmpegFreeUI-API-Extended-Edition")
            request.Headers.Accept.ParseAdd("application/vnd.github+json")
            Using response = Await HTTP.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(False)
                response.EnsureSuccessStatusCode()
                Dim 内容 = Await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(False)
                Return 解析发行文件(内容, 云端版本号)
            End Using
        End Using
    End Function

    ''' <summary>只接受同一扩展版 Release 的三个必需文件及 GitHub 提供的 SHA-256。</summary>
    Friend Shared Function 解析发行文件(json As String, 云端版本号 As String) As IReadOnlyList(Of 更新文件)
        Using 文档 = JsonDocument.Parse(json)
            Dim 根 = 文档.RootElement
            If Not String.Equals(根.GetProperty("tag_name").GetString(), 云端版本号, StringComparison.Ordinal) Then
                Throw New InvalidDataException("Release 版本与检查结果不一致。")
            End If
            Dim 资源 = 根.GetProperty("assets")
            Dim 结果 As New List(Of 更新文件)
            For Each 文件名 In 必需文件名
                Dim 匹配 As JsonElement? = Nothing
                For Each 候选 In 资源.EnumerateArray()
                    If Not String.Equals(候选.GetProperty("name").GetString(), 文件名, StringComparison.Ordinal) Then Continue For
                    If 匹配.HasValue Then Throw New InvalidDataException($"Release 中有重复文件：{文件名}")
                    匹配 = 候选
                Next
                If Not 匹配.HasValue Then Throw New InvalidDataException($"Release 缺少 {文件名}，无法自动更新。")

                Dim 条目 = 匹配.Value
                Dim 长度 = 条目.GetProperty("size").GetInt64()
                Dim 摘要 = 条目.GetProperty("digest").GetString()
                Dim url = 条目.GetProperty("browser_download_url").GetString()
                Dim 地址 As Uri = Nothing
                Dim 地址前缀 = $"/{网络功能_v6_软件版本检查.GitHub仓库拥有者}/{网络功能_v6_软件版本检查.GitHub仓库名称}/releases/download/"
                If 长度 <= 0 OrElse String.IsNullOrWhiteSpace(摘要) OrElse
                   Not 摘要.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) OrElse
                   摘要.Length <> 71 OrElse
                   Not 摘要.Substring(7).All(Function(c) Uri.IsHexDigit(c)) OrElse
                   Not Uri.TryCreate(url, UriKind.Absolute, 地址) OrElse
                   地址.Scheme <> Uri.UriSchemeHttps OrElse
                   Not String.Equals(地址.Host, "github.com", StringComparison.OrdinalIgnoreCase) OrElse
                   Not 地址.AbsolutePath.StartsWith(地址前缀, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidDataException($"{文件名} 的下载地址、长度或 SHA-256 不合法。")
                End If
                结果.Add(New 更新文件(文件名, 地址, 长度, 摘要.Substring(7).ToUpperInvariant()))
            Next
            Return 结果
        End Using
    End Function

    Private Shared Async Function 下载并校验Async(
        云端版本号 As String,
        暂存目录 As String,
        文件列表 As IReadOnlyList(Of 更新文件),
        进度 As IProgress(Of Integer),
        cancellationToken As CancellationToken) As Task(Of 待安装更新)

        Directory.CreateDirectory(暂存目录)
        Dim 总长度 = 文件列表.Sum(Function(文件) 文件.长度)
        Dim 已下载 As Long = 0
        Dim 已报告 As Integer = -1
        For Each 文件 In 文件列表
            cancellationToken.ThrowIfCancellationRequested()
            Dim 输出路径 = Path.Combine(暂存目录, 文件.文件名)
            Using request As New HttpRequestMessage(HttpMethod.Get, 文件.下载地址)
                request.Headers.UserAgent.ParseAdd("FFmpegFreeUI-API-Extended-Edition")
                Using response = Await HTTP.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(False)
                    response.EnsureSuccessStatusCode()
                    If response.Content.Headers.ContentLength.HasValue AndAlso response.Content.Headers.ContentLength.Value <> 文件.长度 Then
                        Throw New InvalidDataException($"{文件.文件名} 的响应长度与 Release 记录不符。")
                    End If
                    Using 输入 = Await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(False)
                        Using 输出 As New FileStream(输出路径, FileMode.CreateNew, FileAccess.Write, FileShare.None, 131072, True)
                            Dim 缓冲(131071) As Byte
                            Dim 本文件已下载 As Long = 0
                            While True
                                Dim 读取字节 = Await 输入.ReadAsync(缓冲.AsMemory(), cancellationToken).ConfigureAwait(False)
                                If 读取字节 = 0 Then Exit While
                                本文件已下载 += 读取字节
                                If 本文件已下载 > 文件.长度 Then Throw New InvalidDataException($"{文件.文件名} 超过预期长度。")
                                Await 输出.WriteAsync(缓冲.AsMemory(0, 读取字节), cancellationToken).ConfigureAwait(False)
                                Dim 百分比 = CInt(Math.Min(100L, (已下载 + 本文件已下载) * 100L \ 总长度))
                                If 百分比 <> 已报告 Then
                                    已报告 = 百分比
                                    进度.Report(百分比)
                                End If
                            End While
                            If 本文件已下载 <> 文件.长度 Then Throw New InvalidDataException($"{文件.文件名} 下载不完整。")
                        End Using
                    End Using
                End Using
            End Using
            Using 流 = File.OpenRead(输出路径)
                Dim 实际摘要 = Convert.ToHexString(Await SHA256.HashDataAsync(流, cancellationToken).ConfigureAwait(False))
                If Not String.Equals(实际摘要, 文件.SHA256, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidDataException($"{文件.文件名} 的 SHA-256 校验失败。")
                End If
            End Using
            已下载 += 文件.长度
        Next

        Dim 主程序路径 = Path.Combine(暂存目录, "FFmpegFreeUI.exe")
        Dim 文件版本 = 版本号.获取外部程序文件版本号(主程序路径)
        If Not String.Equals(文件版本.TrimStart("v"c), 云端版本号.TrimStart("v"c), StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException($"下载的主程序版本 {文件版本} 与 Release {云端版本号} 不一致。")
        End If
        Dim 文件架构 = 程序架构.获取指定程序文件架构(主程序路径)
        If Not String.Equals(文件架构, 程序架构.获取自身程序架构(), StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException($"发行文件架构 {文件架构} 与当前程序架构不一致。")
        End If
        Dim apiTag = 云端版本号.LastIndexOf("+v", StringComparison.OrdinalIgnoreCase)
        If apiTag < 0 Then Throw New InvalidDataException("Release 标签缺少 Ext API 版本。")
        Dim apiVersion As Version = Nothing
        If Not Version.TryParse(云端版本号.Substring(apiTag + 2), apiVersion) Then
            Throw New InvalidDataException("Release 标签中的 Ext API 版本无效。")
        End If
        For Each 文件名 In 必需文件名.Skip(1)
            Dim 信息 = FileVersionInfo.GetVersionInfo(Path.Combine(暂存目录, 文件名))
            If 信息.FileMajorPart <> apiVersion.Major OrElse 信息.FileMinorPart <> apiVersion.Minor Then
                Throw New InvalidDataException($"{文件名} 与 Release 的 Ext API 版本不一致。")
            End If
        Next
        进度.Report(100)
        Return New 待安装更新(云端版本号, 暂存目录, 文件列表)
    End Function

    Private Shared Sub 安全删除暂存目录(目录 As String)
        Try
            Dim 根目录 = Path.GetFullPath(Path.Combine(Application.StartupPath, ".update-staging"))
            Dim 目标 = Path.GetFullPath(目录)
            If Not 目标.StartsWith(根目录 & Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) Then Return
            If Directory.Exists(目标) Then Directory.Delete(目标, recursive:=True)
        Catch ex As Exception
            Debug.WriteLine($"清理更新暂存目录失败：{ex.Message}")
        End Try
    End Sub

    Friend Shared Function 创建安装脚本(更新 As 待安装更新, 安装目录 As String, 原进程ID As Integer, 重启应用 As Boolean) As String
        Dim 行 As New List(Of String) From {
            "$ErrorActionPreference = 'Stop'",
            "$stage = " & PowerShell字符串(更新.暂存目录),
            "$target = " & PowerShell字符串(安装目录),
            "$tag = " & PowerShell字符串(更新.版本号),
            "$parentId = " & 原进程ID.ToString(Globalization.CultureInfo.InvariantCulture),
            "$restart = " & If(重启应用, "$true", "$false"),
            "$statusFile = Join-Path $target '.update-result.json'",
            "function Get-UpdateHash([string]$path) {",
            "    $sha = [Security.Cryptography.SHA256]::Create()",
            "    try {",
            "        $stream = [IO.File]::OpenRead($path)",
            "        try { return ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-', '') }",
            "        finally { $stream.Dispose() }",
            "    } finally { $sha.Dispose() }",
            "}",
            "$files = @("
        }
        For Each 文件 In 更新.文件列表
            行.Add("    @{ Name = " & PowerShell字符串(文件.文件名) &
                   "; Hash = " & PowerShell字符串(文件.SHA256) &
                   "; Size = " & 文件.长度.ToString(Globalization.CultureInfo.InvariantCulture) & " }")
        Next
        行.AddRange({
            ")",
            "$changed = New-Object System.Collections.ArrayList",
            "$failure = ''",
            "$succeeded = $false",
            "try {",
            "    $fullTarget = [IO.Path]::GetFullPath($target).TrimEnd('\')",
            "    $fullStage = [IO.Path]::GetFullPath($stage)",
            "    if (-not $fullStage.StartsWith($fullTarget + '\.update-staging\', [StringComparison]::OrdinalIgnoreCase)) { throw '更新暂存目录不在程序目录内。' }",
            "    $oldProcess = Get-Process -Id $parentId -ErrorAction SilentlyContinue",
            "    if ($null -ne $oldProcess) { $oldProcess.WaitForExit() }",
            "    $backup = Join-Path $stage 'backup'",
            "    [IO.Directory]::CreateDirectory($backup) | Out-Null",
            "    foreach ($item in $files) {",
            "        $source = Join-Path $stage $item.Name",
            "        if (-not [IO.File]::Exists($source)) { throw ('缺少更新文件：' + $item.Name) }",
            "        if (([IO.FileInfo]$source).Length -ne $item.Size) { throw ('文件长度不符：' + $item.Name) }",
            "        if ((Get-UpdateHash $source) -ne $item.Hash) { throw ('SHA-256 不符：' + $item.Name) }",
            "    }",
            "    foreach ($item in $files) {",
            "        $source = Join-Path $stage $item.Name",
            "        $destination = Join-Path $target $item.Name",
            "        $oldBackup = Join-Path $backup $item.Name",
            "        $hadOriginal = [IO.File]::Exists($destination)",
            "        [void]$changed.Add(@{ Name = $item.Name; HadOriginal = $hadOriginal })",
            "        if ($hadOriginal) { [IO.File]::Replace($source, $destination, $oldBackup, $true) }",
            "        else { [IO.File]::Move($source, $destination) }",
            "    }",
            "    $succeeded = $true",
            "} catch {",
            "    $failure = $_.Exception.Message",
            "    for ($i = $changed.Count - 1; $i -ge 0; $i--) {",
            "        $item = $changed[$i]",
            "        $destination = Join-Path $target $item.Name",
            "        $oldBackup = Join-Path $backup $item.Name",
            "        try {",
            "            if ($item.HadOriginal -and [IO.File]::Exists($oldBackup)) { [IO.File]::Copy($oldBackup, $destination, $true) }",
            "            elseif (-not $item.HadOriginal -and [IO.File]::Exists($destination)) { [IO.File]::Delete($destination) }",
            "        } catch { $failure += '；回滚 ' + $item.Name + ' 失败：' + $_.Exception.Message }",
            "    }",
            "}",
            "try {",
            "    $state = if ($succeeded) { 'success' } else { 'failed' }",
            "    $result = @{ status = $state; version = $tag; detail = $failure } | ConvertTo-Json -Compress",
            "    [IO.File]::WriteAllText($statusFile, $result, [Text.Encoding]::UTF8)",
            "} catch { }",
            "if ($succeeded) {",
            "    try { [IO.Directory]::Delete($stage, $true) } catch { }",
            "    if ($restart) { Start-Process -FilePath (Join-Path $target 'FFmpegFreeUI.exe') -WorkingDirectory $target }",
            "}"
        })
        Return String.Join(vbCrLf, 行)
    End Function

    Private Shared Function PowerShell字符串(值 As String) As String
        Return "'" & 值.Replace("'", "''") & "'"
    End Function

End Class
