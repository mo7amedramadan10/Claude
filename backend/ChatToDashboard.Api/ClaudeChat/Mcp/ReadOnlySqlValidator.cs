using System.Text;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.ClaudeChat.Mcp;

/// <summary>نتيجة التدقيق: إمّا استعلام مقبول (بعد فرض TOP) أو سبب رفض واضح للموديل.</summary>
public sealed record SqlValidation(bool Ok, string? Error, string Sql, IReadOnlyList<string> Tables)
{
    public static SqlValidation Reject(string error) => new(false, error, "", Array.Empty<string>());
}

/// <summary>
/// مدقّق SELECT مستقل لأداة <c>run_sql</c> في محادثة Claude — لا يعتمد على أي مدقّق آخر في التطبيق.
/// القواعد: جملة واحدة تبدأ بـ SELECT أو WITH، كلمات ممنوعة بحدود كلمات (خارج النصوص المقتبسة والتعليقات)،
/// كل جدول مذكور بعد FROM/JOIN/APPLY أو بفاصلة في قائمة FROM يجب أن يكون ضمن جداول المستخدم المسموح بها
/// (أسماء CTE المعرَّفة في WITH مسموحة)، وفرض <c>TOP (n)</c> على الاستعلام الرئيسي إن غاب ولم يُستخدم OFFSET.
/// </summary>
public static class ReadOnlySqlValidator
{
    private static readonly string[] Forbidden =
    {
        "INSERT", "UPDATE", "DELETE", "MERGE", "DROP", "ALTER", "CREATE", "TRUNCATE", "EXEC", "EXECUTE", "GRANT", "REVOKE", "DENY",
        "OPENROWSET", "OPENQUERY", "OPENDATASOURCE", "BULK", "INTO", "WAITFOR", "SHUTDOWN", "RECONFIGURE", "BACKUP", "RESTORE",
        "DBCC", "KILL", "USE", "GO",
    };
    private static readonly Regex ForbiddenWord = new(@"\b(" + string.Join("|", Forbidden) + @")\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ForbiddenPrefix = new(@"\b(xp_|sp_|fn_)\w*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    // معرّف جدول: [schema].[name] أو schema.name أو name (بأقواس اختيارية لكل جزء).
    private const string Ident = @"(?:\[[^\]]+\]|[\p{L}\w#@$]+)(?:\s*\.\s*(?:\[[^\]]+\]|[\p{L}\w#@$]+)){0,2}";
    private static readonly Regex TableRef = new(@"\b(FROM|JOIN|APPLY)\s+(" + Ident + ")", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AfterFrom = new(@"\bFROM\s+(" + Ident + @")(?:\s+(?:AS\s+)?(?:\[[^\]]+\]|\w+))?((?:\s*,\s*" + Ident + @"(?:\s+(?:AS\s+)?(?:\[[^\]]+\]|\w+))?)*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CteName = new(@"(?:\bWITH\b|,)\s*(\[[^\]]+\]|\w+)\s*(?:\([^)]*\))?\s*AS\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Keywords = new(@"^(SELECT|WHERE|GROUP|ORDER|HAVING|ON|AND|OR|NOT|IN|EXISTS|VALUES|TOP|DISTINCT|ALL|UNION|EXCEPT|INTERSECT|CROSS|OUTER|INNER|LEFT|RIGHT|FULL|JOIN|AS|CASE|WHEN|THEN|ELSE|END|NULL|OFFSET|FETCH|OPTION|PIVOT|UNPIVOT|TABLESAMPLE|WITH)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>يدقّق الاستعلام ويعيده بعد فرض TOP. <paramref name="allowed"/>: الجداول المسموحة بشكل
    /// <c>schema.name</c> (مثل <c>proj_ab12.Sales</c> أو <c>dbo.sys_procurement</c>).</summary>
    public static SqlValidation Validate(string? sql, IReadOnlyCollection<string> allowed, int maxRows = 500)
    {
        if (string.IsNullOrWhiteSpace(sql)) return SqlValidation.Reject("الاستعلام فارغ.");
        var stripped = StripComments(sql).Trim().TrimEnd(';').Trim();
        if (stripped.Length == 0) return SqlValidation.Reject("الاستعلام فارغ.");
        var blanked = BlankStrings(stripped);
        if (blanked.Contains(';')) return SqlValidation.Reject("مسموح بجملة واحدة فقط (لا فاصلة منقوطة داخل الاستعلام).");
        if (!Regex.IsMatch(blanked, @"^\s*(SELECT|WITH)\b", RegexOptions.IgnoreCase))
            return SqlValidation.Reject("الاستعلام يجب أن يبدأ بـ SELECT أو WITH — القراءة فقط.");
        if (ForbiddenWord.Match(blanked) is { Success: true } fw)
            return SqlValidation.Reject($"الكلمة «{fw.Groups[1].Value.ToUpperInvariant()}» غير مسموحة — هذه الأداة للقراءة فقط (SELECT).");
        if (ForbiddenPrefix.Match(blanked) is { Success: true } fp)
            return SqlValidation.Reject($"استدعاء «{fp.Value}» غير مسموح.");

        var allowedMap = BuildAllowed(allowed);
        var ctes = new HashSet<string>(CteName.Matches(blanked).Select(m => Unbracket(m.Groups[1].Value)), StringComparer.OrdinalIgnoreCase);
        var referenced = new List<string>();
        var resolved = new List<string>();
        foreach (var m in TableRef.Matches(blanked).Cast<Match>()) referenced.Add(m.Groups[2].Value);
        foreach (var m in AfterFrom.Matches(blanked).Cast<Match>())
            foreach (Match extra in Regex.Matches(m.Groups[2].Value, Ident)) referenced.Add(extra.Value);
        foreach (var raw in referenced)
        {
            var parts = raw.Split('.').Select(p => Unbracket(p.Trim())).Where(p => p.Length > 0).ToList();
            if (parts.Count == 1 && (ctes.Contains(parts[0]) || Keywords.IsMatch(parts[0]))) continue;
            var key = parts.Count >= 2 ? $"{parts[^2]}.{parts[^1]}" : parts[0];
            if (allowedMap.TryGetValue(key, out var full)) { if (!resolved.Contains(full)) resolved.Add(full); continue; }
            if (parts.Count == 1)
            {
                var candidates = allowedMap.Values.Distinct().Where(v => v.Split('.')[1].Equals(parts[0], StringComparison.OrdinalIgnoreCase)).ToList();
                if (candidates.Count == 1) { if (!resolved.Contains(candidates[0])) resolved.Add(candidates[0]); continue; }
                if (candidates.Count > 1)
                    return SqlValidation.Reject($"الاسم «{parts[0]}» موجود في أكثر من مخطط — اكتبه بالمخطط: {string.Join(" أو ", candidates)}.");
            }
            return SqlValidation.Reject($"الجدول «{raw}» ليس من جداول مشروعك — استخدم list_tables لمعرفة الجداول المتاحة واكتب الاسم كما يظهر فيها.");
        }
        if (resolved.Count == 0 && !Regex.IsMatch(blanked, @"\bFROM\b", RegexOptions.IgnoreCase))
            return new SqlValidation(true, null, stripped, resolved); // SELECT 1+1 ونحوه

        return new SqlValidation(true, null, EnsureTop(stripped, maxRows), resolved);
    }

    private static Dictionary<string, string> BuildAllowed(IEnumerable<string> allowed)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var a in allowed)
        {
            var parts = a.Split('.');
            if (parts.Length != 2) continue;
            map[a] = a;
        }
        return map;
    }

    private static string Unbracket(string s) => s.StartsWith('[') && s.EndsWith(']') ? s[1..^1] : s;

    /// <summary>يزيل تعليقات <c>--</c> و<c>/* */</c> (خارج النصوص المقتبسة).</summary>
    public static string StripComments(string sql)
    {
        var sb = new StringBuilder(sql.Length);
        for (var i = 0; i < sql.Length; i++)
        {
            var c = sql[i];
            if (c == '\'')
            {
                var j = i + 1;
                while (j < sql.Length) { if (sql[j] == '\'') { if (j + 1 < sql.Length && sql[j + 1] == '\'') { j += 2; continue; } break; } j++; }
                sb.Append(sql, i, Math.Min(j + 1, sql.Length) - i); i = j; continue;
            }
            if (c == '-' && i + 1 < sql.Length && sql[i + 1] == '-') { while (i < sql.Length && sql[i] != '\n') i++; sb.Append(' '); continue; }
            if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*') { var end = sql.IndexOf("*/", i + 2, StringComparison.Ordinal); i = end < 0 ? sql.Length : end + 1; sb.Append(' '); continue; }
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>يستبدل محتوى النصوص المقتبسة بفراغات حتى لا تُحسب الكلمات داخلها.</summary>
    public static string BlankStrings(string sql)
    {
        var sb = new StringBuilder(sql.Length);
        var inString = false;
        for (var i = 0; i < sql.Length; i++)
        {
            var c = sql[i];
            if (inString)
            {
                if (c == '\'') { if (i + 1 < sql.Length && sql[i + 1] == '\'') { sb.Append("  "); i++; continue; } inString = false; sb.Append('\''); }
                else sb.Append(' ');
                continue;
            }
            if (c == '\'') { inString = true; sb.Append('\''); continue; }
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>يُدرج <c>TOP (n)</c> بعد SELECT الرئيسي (بعد DISTINCT/ALL إن وُجدا) ما لم يكن موجودًا أو يُستخدم OFFSET.</summary>
    public static string EnsureTop(string sql, int maxRows)
    {
        var blanked = BlankStrings(sql);
        var depth = 0; var mainSelect = -1;
        for (var i = 0; i < blanked.Length; i++)
        {
            var c = blanked[i];
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (depth == 0 && (i == 0 || !IsWordChar(blanked[i - 1])) && blanked.Length >= i + 6
                     && string.Compare(blanked, i, "SELECT", 0, 6, StringComparison.OrdinalIgnoreCase) == 0
                     && (i + 6 == blanked.Length || !IsWordChar(blanked[i + 6])))
            { mainSelect = i; break; }
        }
        if (mainSelect < 0) return sql;
        var rest = blanked[(mainSelect + 6)..];
        var m = Regex.Match(rest, @"^\s*((?:DISTINCT|ALL)\s+)?(TOP\b)?", RegexOptions.IgnoreCase);
        if (m.Groups[2].Success) return sql;
        if (Regex.IsMatch(rest, @"\bOFFSET\b", RegexOptions.IgnoreCase)) return sql;
        var insertAt = mainSelect + 6 + m.Groups[1].Length + (m.Groups[1].Success ? m.Groups[1].Index : 0);
        // m.Groups[1].Index نسبي إلى rest ويبدأ بعد الفراغ البادئ
        insertAt = mainSelect + 6 + (m.Groups[1].Success ? m.Groups[1].Index + m.Groups[1].Length : 0);
        var prefix = sql[..insertAt];
        var suffix = sql[insertAt..];
        var needsSpaceBefore = prefix.Length > 0 && !char.IsWhiteSpace(prefix[^1]);
        return prefix + (needsSpaceBefore ? " " : "") + $"TOP ({maxRows}) " + suffix.TrimStart();
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
}
