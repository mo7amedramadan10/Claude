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
    public sealed record StreamEvent(string Kind, string? Text = null, double? CostUsd = null, long? DurationMs = null, string? Model = null);

    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeCliService> _logger;

    public ClaudeCliService(IConfiguration configuration, ILogger<ClaudeCliService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

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
    public async IAsyncEnumerable<StreamEvent> SendAsync(string sessionId, bool resume, string message,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var args = new List<string>
        {
            "-p", "--output-format", "stream-json", "--verbose", "--include-partial-messages",
            "--tools", "", "--permission-prompts", "none", "--no-chrome",
            resume ? "--resume" : "--session-id", sessionId,
        };
        if (Model is { } model) { args.Add("--model"); args.Add(model); }
        var timeout = TimeSpan.FromSeconds(_configuration.GetValue("ClaudeCli:TimeoutSeconds", 600));

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
