using System.Reflection;
using System.Text;
using System.Text.Json;
using FFmpegFreeUI;

internal static partial class Program
{
    private static void TestUpstream6243(string directory)
    {
        string Execute(string name, object arguments, int permission = AgentLocalTools.PermissionSystem) =>
            PumpAgentTask(AgentLocalTools.ExecuteAsync(new AgentToolCallInfo { Name = name, Arguments = JsonSerializer.Serialize(arguments) },
                permission, AgentNetworkMode.Disabled, null!, "", ""), "New Agent tools must finish");
        var root = Path.Combine(directory, "upstream-6243");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        Directory.CreateDirectory(Path.Combine(root, "bin"));
        File.WriteAllText(Path.Combine(root, "first.txt"), "before\nNeedle one\nafter\nneedle two", new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(root, "nested", "second.txt"), "Needle nested", new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(root, "bin", "ignored.txt"), "Needle ignored", new UTF8Encoding(false));
        File.WriteAllBytes(Path.Combine(root, "binary.txt"), [0, 1, 2, 255]);
        using (var result = JsonDocument.Parse(Execute("search_local_text", new { path = root, query = "Needle", pattern = "**/*.txt", context_lines = 1 })))
        {
            var matches = result.RootElement.GetProperty("matches").EnumerateArray().ToArray();
            Check(matches.Length == 3, "Text search handles nested globs, ignores binary and excluded bin files");
            var first = matches.Single(x => x.GetProperty("line").GetInt32() == 2);
            Check(first.GetProperty("column").GetInt32() == 1 && first.GetProperty("before")[0].GetString() == "before" &&
                first.GetProperty("after")[0].GetString() == "after", "Search preserves original line numbers, columns and context");
        }
        Check(Execute("search_local_files", new { path = root }, AgentLocalTools.PermissionSafe).Contains("权限不足"),
            "Safe permission cannot use local search");
        using (var first = JsonDocument.Parse(Execute("search_local_files", new { path = root, pattern = "*.txt", limit = 1 })))
        using (var second = JsonDocument.Parse(Execute("search_local_files", new { path = root, pattern = "*.txt", limit = 1, offset = first.RootElement.GetProperty("next_offset").GetInt32() })))
            Check(first.RootElement.GetProperty("has_more").GetBoolean() &&
                first.RootElement.GetProperty("matches")[0].GetProperty("path").GetString() != second.RootElement.GetProperty("matches")[0].GetProperty("path").GetString(),
                "Search pagination advances past returned matches");
        using (var listing = JsonDocument.Parse(Execute("list_directory", new { path = root, pattern = "*.txt", limit = 1 })))
            Check(listing.RootElement.GetProperty("entries").GetArrayLength() == 1 && listing.RootElement.GetProperty("has_more").GetBoolean(),
                "Directory listing uses paged entries rather than an unbounded array");
        Check(Execute("read_local_text_file", new { path = Path.Combine(root, "first.txt"), start_line = 2, line_count = 1, include_line_numbers = true })
            .StartsWith("2: Needle one", StringComparison.Ordinal), "Text read can include original line numbers");
        foreach (var encoding in new Encoding[] { new UTF8Encoding(true), Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32, new UTF32Encoding(true, true) })
        {
            var path = Path.Combine(root, $"encoded-{encoding.CodePage}.txt");
            File.WriteAllText(path, "旧内容", encoding);
            Execute("apply_local_text_patch", new { path, old_text = "旧", new_text = "新" });
            var bytes = File.ReadAllBytes(path);
            Check(bytes.AsSpan().StartsWith(encoding.GetPreamble()) && File.ReadAllText(path, encoding) == "新内容", "Unicode patch preserves BOM, byte order and content");
        }
        var invalid = Path.Combine(root, "legacy.txt");
        File.WriteAllBytes(invalid, [0x81, 0xff]);
        Check(Execute("apply_local_text_patch", new { path = invalid, old_text = "x", new_text = "y" }).Contains("补丁未应用") &&
            File.ReadAllBytes(invalid).SequenceEqual(new byte[] { 0x81, 0xff }), "Unknown legacy encoding is rejected without overwriting bytes");

        var parseQuestion = typeof(AgentLocalTools).Assembly.GetType("FFmpegFreeUI.AgentUserInputCoordinator")!
            .GetMethod("CreateQuestion", BindingFlags.Public | BindingFlags.Static)!;
        AgentPendingQuestionData? Parse(object options, out string error)
        {
            object?[] values = [new AgentToolCallInfo { Id = "question", Name = "ask_user", Arguments = JsonSerializer.Serialize(new { question = "选择处理方式", options }) }, "turn", ""];
            var question = (AgentPendingQuestionData?)parseQuestion.Invoke(null, values);
            error = (string)values[2]!;
            return question;
        }
        var question = Parse(new[] { new { id = "keep", label = "保留", description = "保留原文件" }, new { id = "skip", label = "跳过", description = "跳过当前文件" } }, out var error);
        Check(question is not null && question.Options.Count == 2 && error == "", "Valid question preserves explicit options");
        Check(Parse(new[] { new { id = "a", label = "重复" }, new { id = "b", label = "重复" } }, out error) is null && error.Contains("invalid_arguments"),
            "Duplicate choice labels are rejected");
        Check(Parse(new[] { new { id = "a", label = "稍后选择" }, new { id = "b", label = "其他" } }, out _) is null,
            "Reserved question actions cannot be supplied as choices");
        Check(Parse(new[] { new { id = "a", label = "一\n二" }, new { id = "b", label = "其他" } }, out _) is null,
            "Choice buttons remain single line");
        using (var unavailable = JsonDocument.Parse(Execute("ask_user", new { }, AgentLocalTools.PermissionSafe)))
            Check(unavailable.RootElement.GetProperty("status").GetString() == "unavailable", "Standalone tool invocation cannot invent user consent");
        var storePath = Path.Combine(root, "conversations");
        var store = AgentConversationStore.Load(storePath);
        var conversation = new AgentConversationData { PendingQuestion = question };
        conversation.Messages.Add(new AgentMessageData { Role = "assistant", ToolCalls = [new() { Id = "question", Name = "ask_user", Arguments = "{}" }] });
        conversation.Turns.Add(new AgentTurnData { Id = "turn", State = "waiting", Activities = [new() { ToolCallId = "question", State = "waiting" }] });
        store.Conversations.Add(conversation);
        store.Save();
        store = AgentConversationStore.Load(storePath);
        conversation = store.Conversations.Single();
        Check(conversation.PendingQuestion?.ToolCallId == "question" && conversation.Turns.Single().State == "waiting" && !conversation.Messages.Any(x => x.Role == "tool"),
            "Restart preserves the pending question and does not synthesize an answer");
        Check(AgentConversationStore.ReadConversationIndex(storePath).Single().HasPendingQuestion, "Conversation index retains the waiting indicator");
        using (var form = new Form_v6_Agent())
        {
            AgentSet(form, "_store", store);
            AgentSet(form, "_current", conversation);
            Invoke(form, "ResolvePendingQuestion", conversation, "{\"status\":\"selected\",\"option_id\":\"keep\"}");
            Invoke(form, "ResolvePendingQuestion", conversation, "{\"status\":\"selected\",\"option_id\":\"skip\"}");
            Check(conversation.PendingQuestion is null && conversation.Messages.Count(x => x.ToolCallId == "question") == 1 &&
                conversation.Messages.Single(x => x.Role == "tool").Content.Contains("keep"), "Repeated UI callbacks cannot duplicate or overwrite a resolved answer");
        }
        var page = Agent技能资料库_v6.读取资料("ffmpegfreeui", "SKILL.md", 1, 3, 1);
        Check(page.Contains("[未完，请继续读取 start_line=4"), "Skill references expose a continuation after requested lines");
        var paginate = typeof(Agent技能资料库_v6).GetMethod("分页技能文本", BindingFlags.Static | BindingFlags.NonPublic, null,
            [typeof(string), typeof(int), typeof(int), typeof(int)], null)!;
        var longLine = (string)paginate.Invoke(null, [new string('x', 12500) + "\nend", 1, 100, 1])!;
        Check(longLine.Contains("start_line=1, start_column=12001"), "Long skill lines can continue without losing the remainder");
        Check((string)paginate.Invoke(null, [new string('x', 12500) + "\nend", 1, 100, 12001])! == new string('x', 500) + Environment.NewLine + "end" + Environment.NewLine,
            "Long-line continuation returns the remainder and following lines");
        Console.WriteLine("PASS: upstream 6.2.43 Agent questions, persistence, paged search, Unicode patches and skill continuation");
    }
}
