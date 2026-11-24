using System.Text;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Freeform;

/// <summary>نطاق تعديل الموديل في «تصميم حر»: الصفحة كلها (all)، أو شريحة واحدة (slide)، أو العنصر المحدد (element).</summary>
public class FreeformScopeRequest
{
    /// <summary>all | slide | element.</summary>
    public string? Kind { get; set; }
    /// <summary>ترتيب الشريحة (من 0) للنطاق slide — الترتيب نفسه الذي يعدّه شريط التنقّل في المحرر.</summary>
    public int? Index { get; set; }
}

/// <summary>أدوات نقية (مختبَرة) لتعديل جزء من الصفحة فقط: تحديد مدى الشريحة/العنصر داخل HTML، والتحقق من الجزء العائد من الموديل
/// (لا أنماط ولا سكربتات ولا أصناف جديدة، والوسم الجذر كما هو)، ثم تركيبه مكانه فتبقى بقية الصفحة حرفيًا كما كانت.</summary>
public static class FreeformScope
{
    public readonly record struct Range(int Start, int End);

    private static readonly HashSet<string> Void = new(StringComparer.OrdinalIgnoreCase) { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr" };
    private static readonly Regex StartTag = new(@"<([a-zA-Z][\w-]*)\b[^>]*>", RegexOptions.Compiled);
    private static readonly Regex SlideStart = new(@"<([a-zA-Z][\w-]*)\b(?=[^>]*(?:\bclass\s*=\s*(?:""[^""]*(?<![\w-])slide(?![\w-])[^""]*""|'[^']*(?<![\w-])slide(?![\w-])[^']*')|\sdata-slide\b))[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TargetStart = new(@"<([a-zA-Z][\w-]*)\b[^>]*\sdata-jeem-target\b[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ClassAttr = new(@"\bclass\s*=\s*(""([^""]*)""|'([^']*)')", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>نسخة بالطول نفسه تُفرَّغ فيها محتويات script/style والتعليقات، حتى لا يُعدّ وسم داخل نص جافاسكربت أو CSS.</summary>
    internal static string Mask(string html)
    {
        var sb = new StringBuilder(html);
        foreach (Match m in Regex.Matches(html, @"<!--[\s\S]*?-->|(<(script|style)\b[^>]*>)([\s\S]*?)(</\2\s*>)", RegexOptions.IgnoreCase))
        {
            int from, len;
            if (m.Value.StartsWith("<!--")) { from = m.Index; len = m.Length; }
            else { from = m.Groups[3].Index; len = m.Groups[3].Length; }
            for (var i = from; i < from + len; i++) if (sb[i] != '\n') sb[i] = ' ';
        }
        return sb.ToString();
    }

    /// <summary>نهاية العنصر الذي يبدأ وسمه عند <paramref name="start"/> (بعدّ الوسوم المتداخلة من الاسم نفسه)؛ -1 إن لم يُغلق.</summary>
    internal static int ElementEnd(string masked, int start, string tag)
    {
        var open = masked.IndexOf('>', start); if (open < 0) return -1;
        if (Void.Contains(tag) || masked[open - 1] == '/') return open + 1;
        var rx = new Regex($@"<(/?){Regex.Escape(tag)}\b[^>]*>", RegexOptions.IgnoreCase);
        var depth = 1;
        for (var m = rx.Match(masked, open + 1); m.Success; m = m.NextMatch())
        {
            if (m.Groups[1].Value == "/") { if (--depth == 0) return m.Index + m.Length; }
            else if (!m.Value.EndsWith("/>")) depth++;
        }
        return -1;
    }

    /// <summary>الشرائح بالترتيب: عناصر بصنف slide أو سمة data-slide غير متداخلة داخل شريحة أخرى (القاعدة نفسها في محرر المنصة). أقل من شريحتين = لا شرائح.</summary>
    public static List<Range> FindSlides(string html)
    {
        var masked = Mask(html); var list = new List<Range>();
        foreach (Match m in SlideStart.Matches(masked))
        {
            if (list.Count > 0 && m.Index < list[^1].End) continue; // داخل شريحة سابقة
            var end = ElementEnd(masked, m.Index, m.Groups[1].Value);
            if (end > 0) list.Add(new Range(m.Index, end));
        }
        return list.Count >= 2 ? list : new List<Range>();
    }

    /// <summary>مدى العنصر المعلَّم بـ <c>data-jeem-target</c> (العنصر المحدد في المحرر)، أو null.</summary>
    public static Range? FindMarked(string html)
    {
        var masked = Mask(html); var m = TargetStart.Match(masked); if (!m.Success) return null;
        var end = ElementEnd(masked, m.Index, m.Groups[1].Value);
        return end > 0 ? new Range(m.Index, end) : null;
    }

    /// <summary>أنماط الصفحة (محتوى كل &lt;style&gt;) مرجعًا للقراءة فقط عند تعديل جزء، بحد أقصى.</summary>
    public static string PageCss(string html, int max = 40_000)
    {
        var sb = new StringBuilder();
        foreach (Match m in Regex.Matches(html, @"<style\b[^>]*>([\s\S]*?)</style\s*>", RegexOptions.IgnoreCase)) sb.AppendLine(m.Groups[1].Value.Trim());
        var s = sb.ToString(); return s.Length > max ? s[..max] + "\n/* …اقتُطع */" : s;
    }

    /// <summary>كل الأصناف المستخدمة في الصفحة (في الوسوم وفي محدِّدات CSS) — المسموح للموديل استعماله.</summary>
    public static HashSet<string> KnownClasses(string html)
    {
        var set = ClassesIn(Mask(html));
        foreach (Match m in Regex.Matches(PageCss(html, int.MaxValue), @"\.(-?[_a-zA-Z][\w-]*)")) set.Add(m.Groups[1].Value);
        return set;
    }

    internal static HashSet<string> ClassesIn(string html)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match m in ClassAttr.Matches(html))
            foreach (var c in (m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value).Split(' ', '\t', '\n', '\r'))
                if (c.Length > 0) set.Add(c);
        return set;
    }

    /// <summary>يتحقق من الجزء العائد من الموديل وينظّفه: يزيل script/style/link، ويشترط عنصرًا جذرًا واحدًا بالوسم نفسه ويعيد إليه وسم البداية
    /// الأصلي كما هو (فلا يتغيّر إطار الشريحة ولا أصنافها)، ويرفض الأصناف غير الموجودة في الصفحة. يعيد (null, null) لرد فارغ (سؤال بلا تعديل).</summary>
    public static (string? Fragment, string? Error) CheckFragment(string original, string? returned, HashSet<string> knownClasses)
    {
        if (string.IsNullOrWhiteSpace(returned)) return (null, null);
        var s = Regex.Replace(returned.Trim(), @"<(script|style)\b[^>]*>[\s\S]*?</\1\s*>|<link\b[^>]*>", "", RegexOptions.IgnoreCase).Trim();
        s = Regex.Replace(s, @"^```(?:html)?\s*|\s*```$", "").Trim();
        var origStart = StartTag.Match(original); if (!origStart.Success) return (null, "تعذّر قراءة الجزء الأصلي.");
        var tag = origStart.Groups[1].Value;
        var m = StartTag.Match(s);
        if (!m.Success || m.Index != 0 || !string.Equals(m.Groups[1].Value, tag, StringComparison.OrdinalIgnoreCase))
            return (null, $"يجب أن يكون الجزء المُعاد عنصرًا واحدًا يبدأ بالوسم <{tag}> نفسه.");
        var end = ElementEnd(Mask(s), 0, tag);
        if (end < 0) return (null, $"الوسم <{tag}> غير مغلق في الجزء المُعاد.");
        if (s[end..].Trim().Length > 0) return (null, "أعد عنصرًا واحدًا فقط (الجزء المطلوب) بلا أي شيء قبله أو بعده.");
        s = s[..end];
        s = origStart.Value + s[m.Length..]; // وسم البداية الأصلي: الأصناف والسمات والإطار كما هي
        var unknown = ClassesIn(s).Where(c => !knownClasses.Contains(c)).ToList();
        if (unknown.Count > 0) return (null, $"استخدمت أصنافًا غير موجودة في تصميم الصفحة ({string.Join("، ", unknown.Take(8))}) — استخدم أصناف القالب الموجودة فقط.");
        return (s, null);
    }

    private static readonly Regex ChartTag = new(@"<(svg|canvas|div)\b([^>]*)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ChartCls = new(@"(^|[\s_-])(chart|graph|plot|donut|pie|gauge)([\s_-]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>وصف رسوم القالب في جزء (svg/canvas بأصناف رسم، أو canvas): «svg#c12.chart» — ليعرف الموديل ما المقصود بـ«الرسومات الموجودة».
    /// يُستبعد ما سبق استبداله (data-jeem-replaced) وبلوكات البيانات والزخارف (deco).</summary>
    public static List<string> ChartElements(string html)
    {
        var list = new List<string>();
        foreach (Match m in ChartTag.Matches(Mask(html)))
        {
            var attrs = m.Groups[2].Value; var tag = m.Groups[1].Value.ToLowerInvariant();
            if (attrs.Contains("data-jeem-replaced") || attrs.Contains("data-jeem-chart")) continue;
            var cls = Regex.Match(attrs, @"\bclass\s*=\s*[""']([^""']*)[""']").Groups[1].Value;
            if (Regex.IsMatch(cls, @"(^|\s)deco(\s|$)")) continue;
            if (tag != "canvas" && !ChartCls.IsMatch(cls)) continue;
            var id = Regex.Match(attrs, @"\bid\s*=\s*[""']([^""']*)[""']").Groups[1].Value;
            list.Add(tag + (id.Length > 0 ? "#" + id : "") + string.Concat(cls.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(c => "." + c)));
            if (list.Count >= 12) break;
        }
        return list;
    }

    public static string Splice(string html, Range r, string fragment) => html[..r.Start] + fragment + html[r.End..];

    /// <summary>لا يُخزَّن الشعار داخل الصفحة: `data-jeem-logo` يملؤه الخادم عند العرض والنشر.</summary>
    public static string StripLogoSrc(string html) =>
        Regex.Replace(html, @"<img\b[^>]*\bdata-jeem-logo\b[^>]*>", m => Regex.Replace(m.Value, @"\s+src\s*=\s*(""[^""]*""|'[^']*')", ""), RegexOptions.IgnoreCase);
}
