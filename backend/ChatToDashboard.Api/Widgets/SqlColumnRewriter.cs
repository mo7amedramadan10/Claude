using System.Text;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Widgets;

/// <summary>
/// «تصحيح الأعمدة»: يستبدل معرّفات أعمدة في استعلام عنصر محفوظ بأعمدة أخرى من الجدول نفسه،
/// بلا موديل. يعمل على مستوى المعرّف (لا داخل النصوص المقتبسة بعلامة مفردة، ولا اسم دالة،
/// ولا اسم مستعار بعد AS حتى تبقى مفاتيح البيانات التي يعتمد عليها الرسم كما هي). يقبل الاسم
/// مجردًا أو مقتبسًا بـ "…" أو […] أو `…`.
/// </summary>
public static class SqlColumnRewriter
{
    private static readonly Regex Literal = new(@"'(?:[^']|'')*'", RegexOptions.Compiled);

    /// <summary>الأعمدة (من القائمة المعطاة) التي يشير إليها الاستعلام فعلًا.</summary>
    public static IReadOnlyList<string> UsedColumns(string sql, IEnumerable<string> columns)
    {
        var used = new List<string>();
        var code = string.Join(" ", SplitOutsideLiterals(sql));
        foreach (var c in columns)
            if (IdentifierPattern(c).IsMatch(code)) used.Add(c);
        return used;
    }

    /// <summary>يعيد الاستعلام بعد استبدال كل عمود في <paramref name="mapping"/> (من ← إلى).</summary>
    public static string Rewrite(string sql, IReadOnlyDictionary<string, string> mapping, Func<string, string> quote)
    {
        var sb = new StringBuilder();
        var last = 0;
        foreach (Match m in Literal.Matches(sql))
        {
            sb.Append(RewriteCode(sql[last..m.Index], mapping, quote));
            sb.Append(m.Value);
            last = m.Index + m.Length;
        }
        sb.Append(RewriteCode(sql[last..], mapping, quote));
        return sb.ToString();
    }

    private static string RewriteCode(string code, IReadOnlyDictionary<string, string> mapping, Func<string, string> quote)
    {
        foreach (var (from, to) in mapping)
        {
            if (string.Equals(from, to, StringComparison.Ordinal)) continue;
            code = IdentifierPattern(from).Replace(code, m =>
                m.Groups["bare"].Success && IsSimple(to) ? to : quote(to));
        }
        return code;
    }

    private static IEnumerable<string> SplitOutsideLiterals(string sql)
    {
        var last = 0;
        foreach (Match m in Literal.Matches(sql))
        {
            yield return sql[last..m.Index];
            last = m.Index + m.Length;
        }
        yield return sql[last..];
    }

    // مجرد: لا يسبقه حرف/رقم/نقطة-اقتباس ولا AS، ولا يتبعه حرف أو قوس فتح (دالة).
    private static Regex IdentifierPattern(string name)
    {
        var n = Regex.Escape(name);
        return new Regex(
            $@"(?<!\bAS\s+)(?:""{n}""|\[{n}\]|`{n}`|(?<bare>(?<![\w؀-ۿ""\[`]){n}(?![\w؀-ۿ""\]`])(?!\s*\()))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool IsSimple(string name) => Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$");
}
