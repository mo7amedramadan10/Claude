using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatToDashboard.Api.ClaudeChat;

/// <summary>
/// تشغيل Claude Code CLI (الأمر <c>claude</c>) على الخادم نفسه في وضع الطباعة (<c>-p</c>) ببث JSON:
/// محادثة فقط بلا أدوات (<c>--tools ""</c> و<c>--permission-prompts none</c>) — لا يقرأ ملفات ولا ينفّذ
/// أوامر. كل محادثة جلسة CLI بمعرّف نختاره نحن (<c>--session-id</c>) ثم نستأنفها (<c>--resume</c>)
/// فتبقى ذاكرة المحادثة لدى الـ CLI نفسه. الإعدادات في قسم <c>ClaudeCli</c>:
/// <c>Command</c> (افتراضي claude)، <c>WorkingDirectory</c> (مجلد فارغ بجوار التطبيق)، <c>Model</c>،
/// <c>AllowedRoles</c> (افتراضي Admin فقط)، <c>TimeoutSeconds</c>.
/// </summary>
public class ClaudeCliService
{
    public sealed record StreamEvent(string Kind, string? Text = null, double? CostUsd = null, long? DurationMs = null, string? Model = null,
        string? Tool = null, string? Detail = null);

    /// <summary>ملخص قصير لنداء أداة يُعرض في شريط المحادثة («🔎 يستعلم عن…»): اسم الأداة بلا بادئة الخادم وأهم مدخلاتها.</summary>
    public static (string Tool, string Detail) SummarizeToolUse(string? name, JsonObject? input)
    {
        var tool = (name ?? "").Replace("mcp__jeem__", "");
        var detail = tool switch
        {
            "run_sql" => Shorten(input?["sql"]?.GetValue<string>(), 160),
            "describe_table" => input?["table"]?.GetValue<string>() ?? "",
            _ => "",
        };
        return (tool, detail);
        static string Shorten(string? v, int max) { var t = (v ?? "").Replace("\n", " ").Trim(); return t.Length > max ? t[..max] + "…" : t; }
    }

    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeCliService> _logger;
    private readonly Mcp.McpSessionTokens _tokens;
    private readonly Microsoft.AspNetCore.Hosting.Server.IServer _server;

    public ClaudeCliService(IConfiguration configuration, ILogger<ClaudeCliService> logger, Mcp.McpSessionTokens tokens,
        Microsoft.AspNetCore.Hosting.Server.IServer server)
    {
        _configuration = configuration;
        _logger = logger;
        _tokens = tokens;
        _server = server;
    }

    /// <summary>أدوات القراءة عبر MCP مفعَّلة؟ (ClaudeCli:ToolsEnabled، افتراضيًا true). false = محادثة صِرف بلا أدوات.</summary>
    public bool ToolsEnabled => _configuration.GetValue("ClaudeCli:ToolsEnabled", true);

    /// <summary>عنوان خادم MCP الداخلي الذي يناديه الـ CLI من الجهاز نفسه: ClaudeCli:McpBaseUrl إن ضُبط، وإلا أول عنوان
    /// HTTP يستمع إليه هذا الخادم مع استبدال 0.0.0.0/* بـ 127.0.0.1.</summary>
    public string McpUrl
    {
        get
        {
            var configured = _configuration["ClaudeCli:McpBaseUrl"]?.Trim();
            string baseUrl;
            if (!string.IsNullOrEmpty(configured)) baseUrl = configured;
            else
            {
                var addresses = _server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()?.Addresses ?? Array.Empty<string>();
                var http = addresses.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) ?? addresses.FirstOrDefault() ?? "http://127.0.0.1:5000";
                baseUrl = http.Replace("://0.0.0.0", "://127.0.0.1").Replace("://[::]", "://127.0.0.1").Replace("://*", "://127.0.0.1").Replace("://+", "://127.0.0.1");
            }
            return baseUrl.TrimEnd('/') + Mcp.McpTokenMiddleware.Path;
        }
    }

    /// <summary>تعليمات قصيرة بالعربية تُلحق بتعليمات الـ CLI: تعريف الأدوات الثلاث وطلب الاستكشاف قبل الإجابة.</summary>
    public const string ToolsSystemPrompt =
        "أنت مساعد تحليل بيانات داخل منصة «جيم». لديك ثلاث أدوات قراءة فقط على بيانات مشروع المستخدم الحالي: " +
        "list_tables (الجداول المتاحة)، describe_table (الأعمدة ومعانيها وعينة)، run_sql (استعلام SELECT واحد بـ T-SQL على SQL Server). " +
        "قبل الإجابة عن أي سؤال يخص البيانات: نادِ list_tables ثم describe_table للجداول المعنية، ثم run_sql، ولا تخمّن أسماء أعمدة أو أرقامًا. " +
        "اكتب أسماء الجداول كما تعيدها list_tables (بالمخطط). الأدوات للقراءة فقط ولا ترى جداول مشاريع أخرى. " +
        "أجب بالعربية باختصار مع الأرقام التي حصلت عليها فعلًا، واذكر الاستعلام الذي اعتمدت عليه عند الحاجة.";

    public string Command => _configuration["ClaudeCli:Command"]?.Trim() is { Length: > 0 } c ? c : "claude";

    private string? _resolved;

    /// <summary>المسار الفعلي للأمر. على Windows يُثبَّت claude إمّا كـ <c>claude.exe</c> (المثبّت الأصلي في
    /// <c>%USERPROFILE%\.local\bin</c>) أو كغلاف npm <c>claude.cmd</c> في <c>%APPDATA%\npm</c> — و<c>Process.Start</c>
    /// بلا shell لا يكمل الامتداد تلقائيًا فيفشل باسم «claude» وحده. نبحث في PATH بكل امتدادات PATHEXT ثم في
    /// المواقع المعروفة، ونُبقي الاسم كما هو إن كان مسارًا صريحًا.</summary>
    public string ResolvedCommand
    {
        get
        {
            if (_resolved is not null) return _resolved;
            var cmd = Command;
            if (cmd.Contains(Path.DirectorySeparatorChar) || cmd.Contains(Path.AltDirectorySeparatorChar) || File.Exists(cmd))
                return _resolved = cmd;
            var isWindows = OperatingSystem.IsWindows();
            var extensions = isWindows
                ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries).Prepend("").ToArray()
                : new[] { "" };
            var dirs = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            dirs.Add(Path.Combine(home, ".local", "bin"));
            if (isWindows)
            {
                dirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm"));
                dirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "claude"));
            }
            else
            {
                dirs.Add("/usr/local/bin"); dirs.Add("/opt/homebrew/bin"); dirs.Add(Path.Combine(home, ".npm-global", "bin"));
            }
            foreach (var dir in dirs.Where(d => d.Length > 0).Distinct())
                foreach (var ext in extensions)
                {
                    var candidate = Path.Combine(dir, cmd + ext.ToLowerInvariant());
                    if (File.Exists(candidate)) return _resolved = candidate;
                    if (isWindows && File.Exists(Path.Combine(dir, cmd + ext))) return _resolved = Path.Combine(dir, cmd + ext);
                }
            return _resolved = cmd; // غير موجود: يُترك للنظام ليبلّغ، والرسالة تشرح أين بحثنا
        }
    }

    /// <summary>وصف للمستخدم عند التعذّر: أين بحثنا وكيف يضبط المسار.</summary>
    public string NotFoundHint =>
        OperatingSystem.IsWindows()
            ? "لم يُعثر على «claude.exe/claude.cmd» في PATH الخاص بالخدمة ولا في %USERPROFILE%\\.local\\bin ولا %APPDATA%\\npm. " +
              "تأكد أن Claude Code مثبّت لنفس حساب Windows الذي يشغّل الخادم، أو اضبط المسار الكامل في ClaudeCli:Command " +
              "(مثل C:\\Users\\<user>\\.local\\bin\\claude.exe أو C:\\Users\\<user>\\AppData\\Roaming\\npm\\claude.cmd) ثم أعد التشغيل."
            : "لم يُعثر على «claude» في PATH ولا في ~/.local/bin أو /usr/local/bin. ثبّت Claude Code لنفس المستخدم الذي يشغّل الخادم أو اضبط المسار الكامل في ClaudeCli:Command.";
    public string? Model => _configuration["ClaudeCli:Model"]?.Trim() is { Length: > 0 } m ? m : null;
    public bool Enabled => _configuration.GetValue("ClaudeCli:Enabled", true);
    public IReadOnlyList<string> AllowedRoles =>
        _configuration.GetSection("ClaudeCli:AllowedRoles").Get<string[]>() is { Length: > 0 } roles ? roles : new[] { Users.UserRoles.Admin };

    public string WorkingDirectory
    {
        get
        {
            var dir = _configuration["ClaudeCli:WorkingDirectory"]?.Trim() is { Length: > 0 } d ? d : Path.Combine(AppContext.BaseDirectory, "claude-chat");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    /// <summary>هل الأمر متاح على هذا الخادم؟ يعيد الإصدار أو سبب التعذّر.</summary>
    public async Task<(bool Available, string Detail)> ProbeAsync(CancellationToken ct)
    {
        try
        {
            using var p = Start(new[] { "--version" });
            var output = await p.StandardOutput.ReadToEndAsync(ct);
            var error = await p.StandardError.ReadToEndAsync(ct);
            await p.WaitForExitAsync(ct);
            return p.ExitCode == 0 ? (true, output.Trim()) : (false, (error + output).Trim());
        }
        catch (Exception ex)
        {
            var resolved = ResolvedCommand;
            var hint = File.Exists(resolved) ? "" : " " + NotFoundHint;
            return (false, $"تعذّر تشغيل «{resolved}»: {ex.Message}.{hint}");
        }
    }

    /// <summary>يرسل رسالة إلى جلسة CLI (جديدة أو مستأنفة) ويبث الأحداث: delta (نص جزئي)، done (النص الكامل
    /// والتكلفة)، error. يُقتل العملية عند إلغاء الطلب.</summary>
    public async IAsyncEnumerable<StreamEvent> SendAsync(string sessionId, bool resume, string message, string userId, string? projectId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        // الأدوات المدمجة معطَّلة دائمًا (--tools "")؛ أدوات القراءة تأتي من خادم MCP الداخلي فقط (الخطوة 4 من الخطة).
        var args = new List<string>
        {
            "-p", "--output-format", "stream-json", "--verbose", "--include-partial-messages",
            "--tools", "", "--permission-prompts", "none", "--no-chrome",
            resume ? "--resume" : "--session-id", sessionId,
        };
        if (Model is { } model) { args.Add("--model"); args.Add(model); }
        var timeout = TimeSpan.FromSeconds(_configuration.GetValue("ClaudeCli:TimeoutSeconds", 600));

        // مفتاح جلسة مؤقت لهذه الرسالة + ملف mcp-config مؤقت يحمل عنوان الخادم والمفتاح؛ يُلغى الاثنان في النهاية.
        Mcp.McpSession? session = null; string? configPath = null;
        if (ToolsEnabled)
        {
            session = _tokens.Issue(userId, projectId, TimeSpan.FromMinutes(Math.Max(10, timeout.TotalMinutes + 1)));
            configPath = Path.Combine(Path.GetTempPath(), $"jeem-mcp-{Guid.NewGuid():N}.json");
            var config = new JsonObject
            {
                ["mcpServers"] = new JsonObject
                {
                    ["jeem"] = new JsonObject
                    {
                        ["type"] = "http", ["url"] = McpUrl,
                        ["headers"] = new JsonObject { ["Authorization"] = "Bearer " + session.Token },
                    },
                },
            };
            await File.WriteAllTextAsync(configPath, config.ToJsonString(), ct);
            args.AddRange(new[] { "--mcp-config", configPath, "--strict-mcp-config", "--allowedTools", "mcp__jeem__*", "--append-system-prompt", ToolsSystemPrompt });
        }
        try
        {
            await foreach (var ev in RunAsync(args, message, timeout, ct)) yield return ev;
        }
        finally
        {
            if (session is not null) _tokens.Revoke(session.Token);
            if (configPath is not null) { try { File.Delete(configPath); } catch { } }
        }
    }

    private async IAsyncEnumerable<StreamEvent> RunAsync(List<string> args, string message, TimeSpan timeout,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        Process process; string? startError = null;
        try { process = Start(args); }
        catch (Exception ex) { process = null!; startError = ex.Message; }
        if (startError is not null)
        {
            yield return new StreamEvent("error", $"تعذّر تشغيل «{ResolvedCommand}» على الخادم: {startError}.{(File.Exists(ResolvedCommand) ? "" : " " + NotFoundHint)}");
            yield break;
        }

        using (process)
        using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            timeoutCts.CancelAfter(timeout);
            var token = timeoutCts.Token;
            using var kill = token.Register(() => { try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { } });

            // الرسالة عبر stdin (لا كوسيط سطر أوامر) فلا حد للطول ولا مشكلة اقتباس.
            await process.StandardInput.WriteAsync(message);
            process.StandardInput.Close();
            var stderr = new StringBuilder();
            var stderrTask = Task.Run(async () => { string? l; while ((l = await process.StandardError.ReadLineAsync(token)) is not null) stderr.AppendLine(l); }, token);

            var full = new StringBuilder();
            string? resultText = null; double? cost = null; long? duration = null; string? modelName = null; string? errorText = null;
            string? line;
            while (true)
            {
                try { line = await process.StandardOutput.ReadLineAsync(token); }
                catch (OperationCanceledException) { line = null; }
                if (line is null) break;
                if (line.Length == 0 || line[0] != '{') continue;
                JsonObject? node;
                try { node = JsonNode.Parse(line)?.AsObject(); } catch (JsonException) { continue; }
                if (node is null) continue;
                var type = node["type"]?.GetValue<string>();
                switch (type)
                {
                    case "system" when node["subtype"]?.GetValue<string>() == "init":
                        modelName = node["model"]?.GetValue<string>();
                        yield return new StreamEvent("init", Model: modelName);
                        break;
                    case "stream_event":
                    {
                        var ev = node["event"]?.AsObject();
                        if (ev?["type"]?.GetValue<string>() == "content_block_delta" && ev["delta"]?["type"]?.GetValue<string>() == "text_delta")
                        {
                            var t = ev["delta"]?["text"]?.GetValue<string>() ?? "";
                            full.Append(t);
                            yield return new StreamEvent("delta", t);
                        }
                        break;
                    }
                    case "assistant":
                    {
                        // كتل tool_use في رسالة المساعد (مع الأدوات): تُبث لتظهر في الشريط أثناء العمل.
                        if (node["message"]?["content"] is JsonArray blocks)
                            foreach (var block in blocks.OfType<JsonObject>())
                                if (block["type"]?.GetValue<string>() == "tool_use")
                                {
                                    var (toolName, detail) = SummarizeToolUse(block["name"]?.GetValue<string>(), block["input"] as JsonObject);
                                    yield return new StreamEvent("tool", Tool: toolName, Detail: detail);
                                }
                        break;
                    }
                    case "result":
                        resultText = node["result"]?.GetValue<string>();
                        cost = node["total_cost_usd"]?.GetValue<double>();
                        duration = node["duration_ms"]?.GetValue<long>();
                        if (node["is_error"]?.GetValue<bool>() == true)
                            errorText = resultText ?? node["error"]?.ToString() ?? "خطأ من الـ CLI";
                        break;
                }
            }
            try { await process.WaitForExitAsync(token); } catch (OperationCanceledException) { }
            try { await stderrTask; } catch (OperationCanceledException) { }

            if (ct.IsCancellationRequested) yield break;
            if (timeoutCts.IsCancellationRequested) { yield return new StreamEvent("error", "انتهت مهلة الرد."); yield break; }
            if (errorText is not null) { yield return new StreamEvent("error", errorText); yield break; }
            if (process.ExitCode != 0 && full.Length == 0)
            {
                var err = stderr.ToString().Trim();
                _logger.LogWarning("claude CLI خرج بالرمز {Code}: {Err}", process.ExitCode, err);
                yield return new StreamEvent("error", err.Length > 0 ? Trim(err) : $"الأمر خرج بالرمز {process.ExitCode}.");
                yield break;
            }
            yield return new StreamEvent("done", resultText is { Length: > 0 } ? resultText : full.ToString(), cost, duration, modelName);
        }
    }

    private Process Start(IEnumerable<string> args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ResolvedCommand, WorkingDirectory = WorkingDirectory,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8, StandardInputEncoding = new UTF8Encoding(false),
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        // لا نرث متغيرات جلسة Claude Code إن كان التطبيق نفسه يعمل من داخلها (تتعارض مع --session-id).
        psi.Environment.Remove("CLAUDECODE");
        psi.Environment.Remove("CLAUDE_CODE_ENTRYPOINT");
        return Process.Start(psi) ?? throw new InvalidOperationException("Process.Start أعاد null");
    }

    private static string Trim(string s) => s.Length > 600 ? s[..600] + "…" : s;
}
