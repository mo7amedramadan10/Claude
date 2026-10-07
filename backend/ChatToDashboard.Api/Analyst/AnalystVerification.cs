using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Ollama;
using Microsoft.Extensions.Configuration;

namespace ChatToDashboard.Api.Analyst;

public static class AnalystVerificationStatus
{
    public const string Verified = "verified";
    public const string Unverified = "unverified";
}

/// <summary>One of C1/C3/C4's outcomes — the exact {id, pass, detail} shape spec section 7
/// defines for VerificationJson.checks. C2 (file upload totals) is never emitted — see
/// RunAsync's own remarks for why.</summary>
public record AnalystVerificationCheck(string Id, bool Pass, string Detail);

public record AnalystVerificationOutcome(string Status, IReadOnlyList<AnalystVerificationCheck> Checks, string? Reason);

/// <summary>
/// The real C1-C4 checks from spec section 7 — code-driven, never model judgment, replacing the
/// Phase 1-3 stub that always marked an org_data result "unverified" with an honest "not checked
/// yet" reason. Called once when a result is first created (AnalystController.RunAskPipelineAsync)
/// and again on "تحقق الآن" (AnalystController.Reverify) — same method either way, since
/// verification only ever depends on the result's own current columns/rows/template, never on
/// when it runs.
///
/// C2 ("إجماليات الملف", spec section 7) needs upload-time per-column totals recorded in file
/// metadata — nothing in this codebase (RepositoryFile/RepositoryStore) records that anywhere,
/// and the spec itself marks C2 optional. Rather than inventing new upload-time bookkeeping this
/// phase was never asked to build, C2 is simply never emitted (not emitted-and-failing — spec's
/// own "verified يحتاج نجاح كل الفحوصات القابلة للتطبيق" (all APPLICABLE checks) already covers
/// an inapplicable check being absent rather than forced to pass or fail).
/// </summary>
public static class AnalystVerification
{
    public static async Task<AnalystVerificationOutcome> RunAsync(
        AnalyticsTools analytics,
        AnalyticsTools.SourceContext context,
        string? primaryMeasure,
        string? auditSql,
        IReadOnlyList<string> columns,
        IReadOnlyList<Dictionary<string, object?>> rows,
        string answerTemplate,
        IReadOnlyList<AnalystResultSource> sources,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var checks = new List<AnalystVerificationCheck>();

        // C1 — reconciliation against an independent audit query. Only applicable when the
        // result is actually built around an aggregated measure; a plain listing with no
        // primary_measure has nothing for audit_sql to reconcile against.
        if (!string.IsNullOrWhiteSpace(primaryMeasure))
            checks.Add(await RunReconciliationAsync(analytics, context, auditSql, primaryMeasure, rows, configuration, ct));

        // C3 — source freshness. Undefined Analyst:FreshnessMaxAgeHours = skip entirely (spec's
        // own rule). Defined = check every source that actually carries a timestamp — only file
        // sources do (see SourceContext.TableFileLastUpdated's own remarks); a system source with
        // no SourceLastUpdatedAt has nothing to check, not a failure.
        var freshnessMaxAgeHours = configuration.GetValue<double?>("Analyst:FreshnessMaxAgeHours");
        if (freshnessMaxAgeHours is { } maxAgeHours)
        {
            foreach (var source in sources)
            {
                if (source.SourceLastUpdatedAt is not { } lastUpdated) continue;
                var fresh = IsFresh(lastUpdated, maxAgeHours, DateTime.UtcNow);
                source.IsStale = !fresh;
                checks.Add(new AnalystVerificationCheck(
                    $"C3:{source.SourceDisplayName}", fresh,
                    fresh
                        ? $"{source.SourceDisplayName} محدّث."
                        : $"{source.SourceDisplayName} لم يُحدّث منذ أكثر من {maxAgeHours:0.#} ساعة."));
            }
        }

        // C4 — template integrity (spec section 6: the number guard + every placeholder key
        // resolving against this result's own real columns/rows). Independently recomputed here
        // from the final answer_template rather than trusting that the upstream guard
        // (OllamaClient.ValidateAnswerTemplate) always ran and always caught it — the one path it
        // doesn't cover (a guard violation that persisted past every retry) is exactly the case
        // this check exists to still catch, honestly, at the point the result is actually shown.
        var hasStrayDigits = AnalystPlaceholders.HasStrayDigits(answerTemplate);
        var render = AnalystPlaceholders.Render(answerTemplate, columns, rows, primaryMeasure);
        var c4Pass = !hasStrayDigits && render.MissingKeys.Count == 0;
        checks.Add(new AnalystVerificationCheck("C4", c4Pass,
            hasStrayDigits ? "أرقام غير مرتبطة بالبيانات."
            : render.MissingKeys.Count > 0 ? $"مفاتيح غير معروفة في القالب: {string.Join("، ", render.MissingKeys)}."
            : "القالب سليم."));

        var failed = checks.FirstOrDefault(c => !c.Pass);
        var status = failed is null ? AnalystVerificationStatus.Verified : AnalystVerificationStatus.Unverified;
        return new AnalystVerificationOutcome(status, checks, failed?.Detail);
    }

    private static async Task<AnalystVerificationCheck> RunReconciliationAsync(
        AnalyticsTools analytics, AnalyticsTools.SourceContext context, string? auditSql, string primaryMeasure,
        IReadOnlyList<Dictionary<string, object?>> rows, IConfiguration configuration, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(auditSql))
            return new AnalystVerificationCheck("C1", false, "لا يوجد استعلام تدقيق.");

        var (resultJson, isError) = await analytics.RunReadOnlyQueryAsync(auditSql, context, ct);
        if (isError)
            return new AnalystVerificationCheck("C1", false, "تعذّر تنفيذ استعلام التدقيق.");

        if (!OllamaClient.TryExtractRows(resultJson, out _, out var auditRows) || !TryGetFirstNumber(auditRows, out var auditValue))
            return new AnalystVerificationCheck("C1", false, "استعلام التدقيق لم يُرجع قيمة رقمية.");

        var actualSum = rows.Sum(r => ToDouble(r.GetValueOrDefault(primaryMeasure)));
        var tolerance = configuration.GetValue("Analyst:ReconcileTolerance", 0.0001);
        var pass = ReconciliationPasses(actualSum, auditValue, tolerance);
        return new AnalystVerificationCheck("C1", pass,
            pass
                ? "مطابق لاستعلام التدقيق."
                : $"فرق عن استعلام التدقيق ({actualSum:0.##} مقابل {auditValue:0.##}) — يرجّح تكرار صفوف بسبب Join.");
    }

    /// <summary>Relative difference within <paramref name="tolerance"/> of the larger magnitude;
    /// both ~0 counts as matching (division by near-zero would otherwise be undefined). Exposed
    /// (internal) so the tolerance math itself — the one part of C1 that's pure and worth a
    /// direct unit test — doesn't need a live database to verify.</summary>
    internal static bool ReconciliationPasses(double actual, double audit, double tolerance)
    {
        var denom = Math.Max(Math.Abs(actual), Math.Abs(audit));
        var diff = denom < 1e-9 ? 0d : Math.Abs(actual - audit) / denom;
        return diff <= tolerance;
    }

    /// <summary>Exposed (internal) for the same reason as <see cref="ReconciliationPasses"/> —
    /// pure freshness math, testable without a live source.</summary>
    internal static bool IsFresh(DateTime lastUpdatedAtUtc, double maxAgeHours, DateTime nowUtc) =>
        (nowUtc - lastUpdatedAtUtc).TotalHours <= maxAgeHours;

    private static bool TryGetFirstNumber(IReadOnlyList<Dictionary<string, object?>> rows, out double value)
    {
        value = 0;
        if (rows.Count == 0) return false;
        foreach (var cell in rows[0].Values)
        {
            var candidate = ToDoubleOrNull(cell);
            if (candidate is { } d) { value = d; return true; }
        }
        return false;
    }

    private static double ToDouble(object? value) => ToDoubleOrNull(value) ?? 0d;

    private static double? ToDoubleOrNull(object? value) => value switch
    {
        null => null,
        double d => d,
        float f => f,
        decimal dec => (double)dec,
        int i => i,
        long l => l,
        string s when double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed) => parsed,
        _ => null,
    };
}
