using System.Text.Json;
using System.Text.Json.Serialization;
using ChatToDashboard.Api.Analyst;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Ollama;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

public class AnalystAskRequest
{
    [JsonPropertyName("conversationId")] public string? ConversationId { get; set; }
    // Accepted for API-shape compatibility with the spec's contract, but never trusted for
    // authorization — see AnalystController.Ask's own remarks: this app has no "pass a
    // projectId, validate access to it" mechanism anywhere yet (every other controller resolves
    // "the caller's current project" server-side via ProjectStore.ResolveCurrentProjectIdAsync),
    // and Phase 1's own instructions are to reuse the current context mechanism, not invent one.
    [JsonPropertyName("projectId")] public string? ProjectId { get; set; }
    [JsonPropertyName("question")] public string Question { get; set; } = "";
    [JsonPropertyName("sourceIds")] public List<string>? SourceIds { get; set; }
}

/// <summary>
/// "المحلل الذكي" — see the Project Analyst spec (session transcript) for the full design this
/// is Phase 1 of: contracts + storage only. POST ask streams Server-Sent Events; every other
/// action here is a plain JSON endpoint. Always Ollama/Qwen (see OllamaClient.GenerateAnalystAsync) —
/// never routed through LlmRouter's provider selection, per the spec's own fixed stack section.
/// </summary>
[ApiController]
[Route("api/analyst")]
public class AnalystController : ControllerBase
{
    /// <summary>Spec section 3 — "الصفوف المخزّنة محدودة بسقف إعداد Analyst.MaxStoredRows".</summary>
    private const int DefaultMaxStoredRows = 5000;

    private readonly AnalystStore _store;
    private readonly OllamaClient _ollama;
    private readonly AnalyticsTools _analytics;
    private readonly PermissionsService _permissions;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalystController> _logger;

    public AnalystController(
        AnalystStore store, OllamaClient ollama, AnalyticsTools analytics, PermissionsService permissions,
        IConfiguration configuration, ILogger<AnalystController> logger)
    {
        _store = store;
        _ollama = ollama;
        _analytics = analytics;
        _permissions = permissions;
        _configuration = configuration;
        _logger = logger;
    }

    private int MaxStoredRows => _configuration.GetValue("Analyst:MaxStoredRows", DefaultMaxStoredRows);

    private static readonly JsonSerializerOptions SseJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [HttpPost("ask")]
    public async Task Ask([FromBody] AnalystAskRequest request, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) { Response.StatusCode = 401; return; }
        if (string.IsNullOrWhiteSpace(request.Question)) { Response.StatusCode = 400; return; }

        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        // Common reverse-proxy (nginx) buffering hint — harmless where unsupported, and
        // without it a proxy in front of this app could hold every event until the stream
        // closes, defeating the whole point of streaming stage events as they happen.
        Response.Headers["X-Accel-Buffering"] = "no";

        async Task EmitAsync(string eventName, object payload)
        {
            var json = JsonSerializer.Serialize(payload, SseJsonOptions);
            await Response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }

        Task EmitStageAsync(string stage, string state) => EmitAsync("stage", new { id = stage, state });

        try
        {
            await RunAskPipelineAsync(request, user, EmitAsync, EmitStageAsync, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // The caller navigated away / cancelled — nothing left to stream to.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analyst ask pipeline failed");
            try
            {
                await EmitAsync("error", new { code = "internal_error", message = "حدث خطأ غير متوقع أثناء معالجة السؤال." });
                await EmitAsync("done", new { });
            }
            catch
            {
                // The connection itself is gone — nothing more to do.
            }
        }
    }

    private async Task RunAskPipelineAsync(
        AnalystAskRequest request, AppUser user,
        Func<string, object, Task> emit, Func<string, string, Task> emitStage, CancellationToken ct)
    {
        // "sources" — pure server-side permission intersection (spec section 5 step 2), no
        // dependency on the question's own classification, so resolved first even though the
        // spec's stage list names "understand" before it — see the Phase 1 report for why this
        // ordering was chosen over threading one extra round trip between two separate model
        // calls just to preserve a literal event order with no behavioral difference either way.
        await emitStage(AnalystStages.Sources, AnalystStageStates.Running);
        var requestedSourceIds = request.SourceIds ?? new List<string>();
        var requestedSelection = requestedSourceIds.Count == 0
            ? SourceSelection.AllEnabled()
            : new SourceSelection { Systems = requestedSourceIds, SystemsUnset = false, Files = requestedSourceIds, FilesUnset = false };
        var effectiveSources = await _permissions.GetEffectiveSelectionAsync(user, requestedSelection, ct);
        if (effectiveSources.ProjectId is null)
        {
            await emitStage(AnalystStages.Sources, AnalystStageStates.Failed);
            await emit("error", new { code = "no_project", message = "لا يوجد مشروع متاح لهذا الحساب." });
            await emit("done", new { });
            return;
        }
        await emitStage(AnalystStages.Sources, AnalystStageStates.Done);

        AnalystConversation? conversation;
        if (!string.IsNullOrWhiteSpace(request.ConversationId))
        {
            conversation = await _store.GetConversationAsync(user.Id, request.ConversationId, ct);
            if (conversation is null)
            {
                await emit("error", new { code = "not_found", message = "المحادثة غير موجودة." });
                await emit("done", new { });
                return;
            }
        }
        else
        {
            conversation = await _store.CreateConversationAsync(effectiveSources.ProjectId, user.Id, MakeTitle(request.Question), ct);
        }
        // So the frontend can pass this back as conversationId on the next question in the same
        // session — the spec's own event list (section 4) doesn't name this event explicitly,
        // but conversationId has to reach the client somehow, and a dedicated early event is
        // simpler than repeating it on every later event.
        await emit("conversation", new { conversationId = conversation.Id, title = conversation.Title });

        await _store.AddMessageAsync(new AnalystMessage
        {
            ConversationId = conversation.Id, Role = AnalystRoles.User, Text = request.Question.Trim(),
        }, ct);

        var loopResult = await _ollama.GenerateAnalystAsync(request.Question.Trim(), effectiveSources, user, (s, st) => emitStage(s, st), ct);

        if (loopResult.Outcome == AnalystLoopOutcome.Failed)
        {
            await emit("error", new { code = "model_error", message = "تعذّر الحصول على إجابة — حاول صياغة السؤال بشكل مختلف." });
            await _store.TouchConversationAsync(conversation.Id, ct);
            await emit("done", new { });
            return;
        }

        if (loopResult.Outcome == AnalystLoopOutcome.OutOfScope)
        {
            await emitStage(AnalystStages.Verify, AnalystStageStates.Done);
            await emitStage(AnalystStages.Compose, AnalystStageStates.Running);
            const string text = "لا أجد إجابة في مصادر المشروع.";
            var message = await _store.AddMessageAsync(new AnalystMessage
            {
                ConversationId = conversation.Id, Role = AnalystRoles.Assistant, Text = text,
            }, ct);
            await emitStage(AnalystStages.Compose, AnalystStageStates.Done);
            await _store.TouchConversationAsync(conversation.Id, ct);
            await emit("message", new { messageId = message.Id, html = EscapeForDisplay(text), knowledgeScope = (string?)null });
            await emit("done", new { });
            return;
        }

        if (loopResult.Outcome == AnalystLoopOutcome.General)
        {
            await emitStage(AnalystStages.Verify, AnalystStageStates.Done);
            await emitStage(AnalystStages.Compose, AnalystStageStates.Running);
            var text = loopResult.Model!.AnswerTemplate;
            var message = await _store.AddMessageAsync(new AnalystMessage
            {
                ConversationId = conversation.Id, Role = AnalystRoles.Assistant, Text = text,
                TemplateText = text, KnowledgeScope = AnalystKnowledgeScopes.General,
            }, ct);
            await emitStage(AnalystStages.Compose, AnalystStageStates.Done);
            await _store.TouchConversationAsync(conversation.Id, ct);
            await emit("message", new { messageId = message.Id, html = EscapeForDisplay(text), knowledgeScope = AnalystKnowledgeScopes.General });
            await emit("done", new { });
            return;
        }

        // OrgData.
        await emitStage(AnalystStages.Verify, AnalystStageStates.Running);
        var model = loopResult.Model!;
        AnalystCapturedQuery? capturedQuery = null;
        if (!string.IsNullOrWhiteSpace(model.ResultQueryId))
            loopResult.CapturedQueries.TryGetValue(model.ResultQueryId, out capturedQuery);

        // Phase 4 owns the real C1-C4 checks (reconciliation against audit_sql, file totals,
        // freshness) — this phase only records that they haven't run yet, rather than claiming
        // a "verified" status nothing has actually earned. See the Phase 1 report.
        var verificationJson = JsonSerializer.Serialize(new
        {
            status = "unverified",
            checks = Array.Empty<object>(),
            reason = "لم يتم التحقق من مطابقة هذه الأرقام لإجماليات المصدر بعد.",
        });
        await emitStage(AnalystStages.Verify, AnalystStageStates.Done);

        await emitStage(AnalystStages.Compose, AnalystStageStates.Running);
        string finalText;
        string? resultId = null;
        if (capturedQuery is null)
        {
            // The model answered org_data but either found nothing to point at, or named a
            // query id we never actually captured (a contract violation) — either way, no
            // AnalystResult to store; the template is shown as-is (it should already read as an
            // honest "couldn't find this" sentence per the system prompt's own instructions).
            finalText = model.AnswerTemplate;
        }
        else
        {
            var columns = capturedQuery.Columns;
            var allRows = capturedQuery.Rows;
            var isTruncated = allRows.Count > MaxStoredRows;
            var storedRows = isTruncated ? allRows.Take(MaxStoredRows).ToList() : allRows.ToList();

            // Render against the FULL captured rows, not storedRows — MaxStoredRows caps what
            // gets persisted for the table/export view, but a sum/avg/top computed only over a
            // truncated slice would be a wrong number, not an honestly-smaller one. The guard
            // upstream (OllamaClient.ValidateAnswerTemplate) already validated this exact call
            // against the same full rows, so MissingKeys should be empty here in practice; still
            // logged, not silently dropped, for the one path the guard doesn't cover (no
            // result_query_id at all) and as a defensive trace if it ever isn't.
            var render = AnalystPlaceholders.Render(model.AnswerTemplate, columns, allRows, model.PrimaryMeasure);
            finalText = render.Text;
            if (render.MissingKeys.Count > 0)
                _logger.LogWarning("Analyst answer_template referenced unknown keys: {Keys}", string.Join(", ", render.MissingKeys));

            var result = await _store.AddResultAsync(new AnalystResult
            {
                ConversationId = conversation.Id,
                ProjectId = effectiveSources.ProjectId,
                Title = model.Title,
                ResultType = model.ResultType,
                ColumnsJson = JsonSerializer.Serialize(columns),
                RowsJson = JsonSerializer.Serialize(storedRows),
                KeyColumn = model.KeyColumn,
                PrimaryMeasure = model.PrimaryMeasure,
                ExecutedSql = capturedQuery.Sql,
                AuditSql = model.AuditSql,
                VerificationJson = verificationJson,
                DataVersion = 1,
                IsTruncated = isTruncated,
            }, ct);
            resultId = result.Id;

            var sourceContext = await _analytics.DescribeSourcesAsync(effectiveSources, ct);
            var sources = DetectTouchedSources(capturedQuery.Sql, sourceContext);
            if (sources.Count > 0)
                await _store.AddResultSourcesAsync(sources.Select(s => new AnalystResultSource
                {
                    ResultId = result.Id, SourceId = s.Id, SourceDisplayName = s.Name, SourceKind = s.Kind, TablesJson = JsonSerializer.Serialize(new[] { s.Table }),
                }).ToList(), ct);
        }

        var finalMessage = await _store.AddMessageAsync(new AnalystMessage
        {
            ConversationId = conversation.Id, Role = AnalystRoles.Assistant, Text = finalText,
            TemplateText = model.AnswerTemplate, ResultId = resultId, KnowledgeScope = AnalystKnowledgeScopes.OrgData,
        }, ct);
        await emitStage(AnalystStages.Compose, AnalystStageStates.Done);
        await _store.TouchConversationAsync(conversation.Id, ct);

        if (resultId is not null) await emit("result", new { resultId });
        await emit("message", new
        {
            messageId = finalMessage.Id, html = EscapeForDisplay(finalText),
            knowledgeScope = AnalystKnowledgeScopes.OrgData, resultId,
        });
        await emit("done", new { });
    }

    private record TouchedSource(string Id, string Name, string Kind, string Table);

    /// <summary>Heuristic, not a SQL parser: a captured query's table/column list is already
    /// known from its own result rows' column names, but which SOURCE (file/system) it came
    /// from isn't directly recorded — so this looks for which of the context's own known table
    /// names appear as a substring of the executed SQL. Good enough for Phase 1's "مصادر
    /// البيانات" panel data to exist at all; a false-negative here (a table name not matched)
    /// just means that source doesn't show up, never a wrong one being shown.</summary>
    private static List<TouchedSource> DetectTouchedSources(string sql, AnalyticsTools.SourceContext context)
    {
        var found = new List<TouchedSource>();
        foreach (var (table, systemName) in context.TableSystems)
            if (sql.Contains(table, StringComparison.OrdinalIgnoreCase))
                found.Add(new TouchedSource(systemName, systemName, AnalystSourceKinds.System, table));
        foreach (var (table, fileName) in context.TableFiles)
            if (sql.Contains(table, StringComparison.OrdinalIgnoreCase))
                found.Add(new TouchedSource(fileName, fileName, AnalystSourceKinds.File, table));
        return found;
    }

    private static string EscapeForDisplay(string text) =>
        // Phase 1 stores/returns plain text; Phase 2's frontend renders it into safe DOM nodes
        // itself (spec section 6 — "العميل لا يستقبل HTML من الموديل أبدًا"). No HTML escaping
        // needed server-side since nothing here is ever interpreted as markup before Phase 2's
        // own renderer exists to define that contract.
        text;

    private static string MakeTitle(string question)
    {
        const int maxLen = 60;
        var trimmed = question.Trim();
        if (trimmed.Length <= maxLen) return trimmed;
        var cut = trimmed[..maxLen];
        var lastSpace = cut.LastIndexOf(' ');
        if (lastSpace > 20) cut = cut[..lastSpace];
        return cut + "…";
    }

    /// <summary>GET /api/analyst/results/{id} — spec section 4: the full result (columns, rows,
    /// sources, verification, data version). Scoped to the caller's own current project, same
    /// cross-tenant defense every other project-scoped GET-by-id already applies.</summary>
    [HttpGet("results/{id}")]
    public async Task<ActionResult<object>> GetResult(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effectiveSources = await _permissions.GetEffectiveSelectionAsync(user, SourceSelection.AllEnabled(), ct);
        if (effectiveSources.ProjectId is null) return NotFound();

        var result = await _store.GetResultAsync(effectiveSources.ProjectId, id, ct);
        if (result is null) return NotFound();

        var sources = await _store.ListResultSourcesAsync(id, ct);
        return Ok(new
        {
            id = result.Id,
            title = result.Title,
            resultType = result.ResultType,
            columns = AnalystStore.DeserializeColumns(result.ColumnsJson),
            rows = AnalystStore.DeserializeRows(result.RowsJson),
            keyColumn = result.KeyColumn,
            primaryMeasure = result.PrimaryMeasure,
            executedSql = result.ExecutedSql,
            auditSql = result.AuditSql,
            verification = JsonDocument.Parse(result.VerificationJson).RootElement,
            dataVersion = result.DataVersion,
            isTruncated = result.IsTruncated,
            sources = sources.Select(s => new
            {
                sourceId = s.SourceId, sourceDisplayName = s.SourceDisplayName, sourceKind = s.SourceKind,
                tables = JsonSerializer.Deserialize<string[]>(s.TablesJson), sourceLastUpdatedAt = s.SourceLastUpdatedAt, isStale = s.IsStale,
            }),
            createdAt = result.CreatedAt,
        });
    }

    /// <summary>GET /api/analyst/results/{id}/export.csv — spec section 4/8 ("تنزيل CSV"),
    /// Phase 2 scope. UTF-8 with a BOM so Excel opens Arabic text correctly (same requirement
    /// the spec calls out explicitly) — plain string concatenation is fine here since every
    /// value is quoted and internal quotes doubled, the one escaping rule CSV actually needs.</summary>
    [HttpGet("results/{id}/export.csv")]
    public async Task<IActionResult> ExportCsv(string id, CancellationToken ct)
    {
        var user = await _permissions.GetCurrentUserAsync(User, ct);
        if (user is null) return Unauthorized();
        var effectiveSources = await _permissions.GetEffectiveSelectionAsync(user, SourceSelection.AllEnabled(), ct);
        if (effectiveSources.ProjectId is null) return NotFound();

        var result = await _store.GetResultAsync(effectiveSources.ProjectId, id, ct);
        if (result is null) return NotFound();

        var columns = AnalystStore.DeserializeColumns(result.ColumnsJson);
        var rows = AnalystStore.DeserializeRows(result.RowsJson);

        string CsvField(object? value) => "\"" + (value?.ToString() ?? "").Replace("\"", "\"\"") + "\"";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", columns.Select(CsvField)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", columns.Select(c => CsvField(row.GetValueOrDefault(c)))));

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var fileName = (string.IsNullOrWhiteSpace(result.Title) ? "نتيجة" : result.Title) + ".csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }
}
