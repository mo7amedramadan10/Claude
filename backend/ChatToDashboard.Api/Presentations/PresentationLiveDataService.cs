using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Presentations;

/// <summary>نتيجة البيانات الحيّة لعرض منشور: وقت القراءة، ونتائج الاستعلامات بمفاتيحها (تصميم حر)، أو مواصفة العرض
/// بعد إعادة صياغة شرائح البيانات (منشئ العروض)، وعدد ما فشل.</summary>
public sealed record PresentationLiveData(DateTime RefreshedAt, IReadOnlyList<FreeformQuery> Queries, DeckSpec? Spec, int Failed, string? Error)
{
    /// <summary>كائن الجسر: <c>{refreshedAt, queries:{qN:{rows,rowCount}}, spec?, failed}</c>.</summary>
    public JsonObject ToJson()
    {
        var qs = new JsonObject();
        foreach (var q in Queries)
        {
            var r = q.Result?.AsObject();
            qs[q.Key] = new JsonObject { ["rows"] = r?["rows"]?.DeepClone() ?? new JsonArray(), ["rowCount"] = r?["rowCount"]?.DeepClone() ?? 0 };
        }
        var o = new JsonObject { ["refreshedAt"] = RefreshedAt.ToString("O"), ["queries"] = qs, ["failed"] = Failed };
        if (Spec is not null) o["spec"] = JsonNode.Parse(JsonSerializer.Serialize(Spec, DeckSpec.Json));
        if (Error is not null) o["error"] = Error;
        return o;
    }
}

/// <summary>
/// البيانات الحيّة للعروض المنشورة من داخل المنصة (المرحلة 3 من تحديث البيانات): الصفحة المنشورة لا تُعاد نشرها حين تتغيّر
/// الأرقام، بل عند كل فتح تُعاد قراءة بيانات مصدرها (الاستعلامات المسجَّلة لتصميم حر، أو <c>query.sql</c> لشرائح العرض) وتُحقن
/// في الصفحة قبل تقديمها، وتستطيع الصفحة المفتوحة طلبها دوريًا عبر الجسر (<c>jeem.data</c>). الاستعلامات ثابتة سجّلها الموديل عند
/// البناء — لا يُنفَّذ أي SQL يرسله المتصفح — وتمر من بوابة القراءة فقط نفسها بصلاحيات صاحب التصميم. النتيجة مخزَّنة مؤقتًا
/// لكل عرض (افتراضيًا 60 ثانية) حتى لا يضغط المشاهدون المتعدّدون على قاعدة البيانات.
/// </summary>
public sealed class PresentationLiveDataService
{
    public const string KindFreeform = "freeform", KindDeck = "deck";
    private static readonly Regex DeckSpecRx = new(@"(<script\b[^>]*\bid\s*=\s*[""']deck-spec[""'][^>]*>)([\s\S]*?)(</script>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly FreeformStore _freeform;
    private readonly DeckStore _decks;
    private readonly UserStore _users;
    private readonly AnalyticsTools _tools;
    private readonly ILogger<PresentationLiveDataService> _log;
    private readonly TimeSpan _ttl;
    private readonly ConcurrentDictionary<string, (DateTime At, PresentationLiveData Data)> _cache = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new();

    public PresentationLiveDataService(FreeformStore freeform, DeckStore decks, UserStore users, AnalyticsTools tools, IConfiguration config, ILogger<PresentationLiveDataService> log)
    {
        _freeform = freeform; _decks = decks; _users = users; _tools = tools; _log = log;
        _ttl = TimeSpan.FromSeconds(Math.Max(5, config.GetValue<int?>("Presentations:LiveDataCacheSeconds") ?? 60));
    }

    /// <summary>هل للعرض مصدر حيّ مفعَّل؟</summary>
    public static bool IsLive(Presentation p) => p.LiveData && p.SourceKind is KindFreeform or KindDeck && !string.IsNullOrEmpty(p.SourceId);

    /// <summary>يبطل الذاكرة المؤقتة لعرض (بعد إعادة نشر أو تحديث يدوي للمصدر).</summary>
    public void Invalidate(string presentationId) => _cache.TryRemove(presentationId, out _);

    /// <summary>البيانات الحيّة للعرض (من الذاكرة المؤقتة إن كانت حديثة) أو null إن لم يكن له مصدر حيّ أو حُذف مصدره.</summary>
    public async Task<PresentationLiveData?> GetAsync(Presentation p, bool force, CancellationToken ct)
    {
        if (!IsLive(p)) return null;
        if (!force && _cache.TryGetValue(p.Id, out var hit) && DateTime.UtcNow - hit.At < _ttl) return hit.Data;
        var gate = _gates.GetOrAdd(p.Id, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            if (!force && _cache.TryGetValue(p.Id, out hit) && DateTime.UtcNow - hit.At < _ttl) return hit.Data;
            var data = p.SourceKind == KindFreeform ? await FreeformAsync(p.SourceId!, ct) : await DeckAsync(p.SourceId!, ct);
            if (data is not null) _cache[p.Id] = (DateTime.UtcNow, data);
            return data;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.LogWarning(ex, "تعذّرت قراءة البيانات الحيّة للعرض {Id}", p.Id);
            return _cache.TryGetValue(p.Id, out hit) ? hit.Data : null;
        }
        finally { gate.Release(); }
    }

    /// <summary>يحقن البيانات الحيّة في HTML الصفحة المنشورة قبل تقديمها: كتلة <c>jeem-data</c> لتصميم حر، أو مواصفة <c>deck-spec</c> للعرض.</summary>
    public static string ApplyToHtml(string html, Presentation p, PresentationLiveData data)
    {
        if (p.SourceKind == KindFreeform) return data.Queries.Count == 0 ? html : FreeformDataBlock.Inject(html, data.Queries, data.RefreshedAt);
        if (p.SourceKind == KindDeck && data.Spec is not null) return ReplaceDeckSpec(html, data.Spec);
        return html;
    }

    /// <summary>يستبدل مواصفة العرض المضمّنة في الصفحة المنشورة مع الإبقاء على الهوية والقالب المرفقين فيها (لا تُخزَّن في المصدر).</summary>
    internal static string ReplaceDeckSpec(string html, DeckSpec fresh)
    {
        var m = DeckSpecRx.Match(html ?? "");
        if (!m.Success) return html ?? "";
        JsonObject? current;
        try { current = JsonNode.Parse(m.Groups[2].Value.Replace("<\\/", "</"))?.AsObject(); } catch (JsonException) { current = null; }
        var next = JsonNode.Parse(JsonSerializer.Serialize(fresh, DeckSpec.Json))!.AsObject();
        if (current is not null)
            foreach (var keep in new[] { "brand", "template", "templateMode" })
                if (current[keep] is { } v && next[keep] is null) next[keep] = v.DeepClone();
        var json = next.ToJsonString(DeckSpec.Json).Replace("</", "<\\/");
        return html!.Substring(0, m.Index) + m.Groups[1].Value + json + m.Groups[3].Value + html.Substring(m.Index + m.Length);
    }

    private async Task<SourceSelection> SelectionForAsync(string? projectId, string createdByUserId, CancellationToken ct)
    {
        // الاستعلامات نُفِّذت أصلًا بصلاحيات صاحب التصميم عند البناء؛ تُعاد بصلاحياته نفسها (لا بصلاحيات من يفتح الرابط العام).
        var owner = await _users.FindByIdAsync(createdByUserId, ct);
        var sel = SourceSelection.AllEnabled();
        sel.ProjectId = projectId; sel.UserId = createdByUserId; sel.IsAdmin = owner?.Role == UserRoles.Admin;
        return sel;
    }

    private async Task<PresentationLiveData?> FreeformAsync(string designId, CancellationToken ct)
    {
        var d = await _freeform.GetByIdAsync(designId, ct);
        if (d is null) return null;
        var stored = await _freeform.QueriesAsync(designId, ct);
        if (stored.Count == 0) stored = FreeformDataBlock.Parse(d.Html);
        if (stored.Count == 0) return new PresentationLiveData(DateTime.UtcNow, Array.Empty<FreeformQuery>(), null, 0, null);
        var context = await _tools.DescribeSourcesAsync(await SelectionForAsync(d.ProjectId, d.CreatedByUserId, ct), ct);
        var fresh = new List<FreeformQuery>(); var failed = 0;
        foreach (var q in stored)
        {
            var (res, isError) = await _tools.RunReadOnlyQueryAsync(q.Sql, context, ct);
            if (isError) { failed++; fresh.Add(q); continue; }
            JsonNode? parsed; try { parsed = JsonNode.Parse(res); } catch (JsonException) { parsed = null; failed++; }
            fresh.Add(parsed is null ? q : q with { Result = parsed });
        }
        return new PresentationLiveData(DateTime.UtcNow, fresh, null, failed, failed == stored.Count ? "فشلت كل الاستعلامات" : null);
    }

    private async Task<PresentationLiveData?> DeckAsync(string deckId, CancellationToken ct)
    {
        var d = await _decks.GetByIdAsync(deckId, ct);
        if (d is null) return null;
        var spec = JsonSerializer.Deserialize<DeckSpec>(d.SpecJson, DeckSpec.Json)?.Normalize();
        if (spec is null) return null;
        var withSql = spec.Slides.Where(s => !string.IsNullOrWhiteSpace(s.Query?.Sql)).ToList();
        if (withSql.Count == 0) return new PresentationLiveData(DateTime.UtcNow, Array.Empty<FreeformQuery>(), null, 0, null);
        var context = await _tools.DescribeSourcesAsync(await SelectionForAsync(d.ProjectId, d.CreatedByUserId, ct), ct);
        var failed = 0;
        foreach (var slide in withSql)
        {
            var (res, isError) = await _tools.RunReadOnlyQueryAsync(slide.Query!.Sql!, context, ct);
            if (isError) { failed++; continue; }
            JsonArray? rows = null; try { rows = JsonNode.Parse(res)?["rows"] as JsonArray; } catch (JsonException) { }
            if (rows is null || !DeckDataRefresher.Apply(slide, rows)) failed++;
        }
        return new PresentationLiveData(DateTime.UtcNow, Array.Empty<FreeformQuery>(), spec.Normalize(), failed, failed == withSql.Count ? "فشلت كل الاستعلامات" : null);
    }
}
