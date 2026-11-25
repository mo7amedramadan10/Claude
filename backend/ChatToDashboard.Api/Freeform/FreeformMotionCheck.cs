using System.Net;
using System.Text.RegularExpressions;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// فحص حتمي لصفحة «حركة» بعد التوليد: هل التزمت بعقد محرّك المنصة (jeem-motion.js)؟ المشاكل تُعاد للموديل في محاولة ثانية،
/// فإن بقيت أُلحقت بالرد. لا يُشغَّل شيء من الصفحة — فحص نصي فقط.
/// </summary>
public static class FreeformMotionCheck
{
    /// <summary>أقصى عدد كلمات معقول في مشهد واحد (عنوان قصير + سطران أو ثلاثة بنود).</summary>
    public const int MaxWordsPerScene = 45;

    private static readonly Regex SceneStart = new(@"<([a-zA-Z][\w-]*)\b(?=[^>]*\bclass\s*=\s*[""'][^""']*(?<![\w-])jm-scene(?![\w-]))[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StageAttr = new(@"<[a-zA-Z][\w-]*\b[^>]*\sdata-jeem-motion\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Script = new(@"<script\b([^>]*)>([\s\S]*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StyleBlock = new(@"<style\b[^>]*>([\s\S]*?)</style>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Timers = new(@"\b(setInterval|setTimeout|requestAnimationFrame)\s*\(", RegexOptions.Compiled);
    private static readonly Regex Tags = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex Words = new(@"[\p{L}\p{N}][\p{L}\p{N}\p{M}٫٬.,%٪]*", RegexOptions.Compiled);

    public static List<string> Validate(string? html)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(html)) return problems;
        var masked = FreeformScope.Mask(html);
        var starts = SceneStart.Matches(masked).Cast<Match>().ToList();
        if (!StageAttr.IsMatch(masked) || starts.Count == 0)
        {
            problems.Add("الصفحة لا تتبع عقد محرّك الحركة: يلزم مسرح <div data-jeem-motion> وأبناؤه المباشرون مشاهد <section class=\"jm-scene\" data-slide data-duration data-title>.");
            return problems;
        }
        if (starts.Count < 2) problems.Add($"في الصفحة {starts.Count} مشهد فقط؛ المطلوب 3–6 مشاهد يحكي كل منها جزءًا من القصة.");
        if (starts.Count > 8) problems.Add($"في الصفحة {starts.Count} مشهدًا؛ اختصرها إلى 6 على الأكثر.");

        // المشاهد: نص قليل في كل مشهد (المساحة ثابتة ولا تمرير فيها)
        var i = 0; var prevEnd = -1;
        foreach (var m in starts)
        {
            if (m.Index < prevEnd) continue; // مشهد داخل مشهد: يُتجاهل
            var end = FreeformScope.ElementEnd(masked, m.Index, m.Groups[1].Value); if (end < 0) continue;
            prevEnd = end; i++;
            var text = WebUtility.HtmlDecode(Tags.Replace(html[m.Index..end], " "));
            var n = Words.Matches(text).Count;
            if (n > MaxWordsPerScene) problems.Add($"المشهد {i} فيه {n} كلمة — كثير على مشهد واحد (الحد نحو {MaxWordsPerScene}): اختصره إلى عنوان وسطر أو قسّمه مشهدين.");
        }

        // محرّك أو مؤقّتات أو keyframes من صنع الموديل
        foreach (Match s in Script.Matches(html))
        {
            var attrs = s.Groups[1].Value;
            if (Regex.IsMatch(attrs, @"\bsrc\s*=|application/json|\bid\s*=\s*[""']jeem-", RegexOptions.IgnoreCase)) continue;
            if (Timers.IsMatch(s.Groups[2].Value)) { problems.Add("في الصفحة جافاسكربت بمؤقّتات (setInterval/setTimeout/requestAnimationFrame) — المحرّك يتولى التوقيت؛ احذفها واستخدم data-in/data-delay."); break; }
        }
        if (StyleBlock.Matches(html).Cast<Match>().Any(st => Regex.IsMatch(st.Groups[1].Value, @"@keyframes", RegexOptions.IgnoreCase)))
            problems.Add("في الأنماط @keyframes — لا تحرّك العناصر بنفسك؛ استخدم data-in (fade|up|down|start|end|zoom|pop|draw|count).");
        if (Regex.IsMatch(masked, @"<button\b[^>]*>\s*(تشغيل|إيقاف|play|pause)", RegexOptions.IgnoreCase))
            problems.Add("في الصفحة أزرار تشغيل/إيقاف من صنعك — المحرّك يضيف شريط التحكم؛ احذفها.");
        return problems;
    }

    /// <summary>رسالة المحاولة الثانية للموديل: الصفحة المرفوضة وأسباب الرفض.</summary>
    public static string RetryMessage(string html, IEnumerable<string> problems) =>
        "\n# محاولتك السابقة رُفضت آليًا — صحّحها وأعد الصفحة كاملة بعقد محرّك الحركة\n"
        + string.Join("\n", problems.Select(p => "- " + p)) + "\n\n# الصفحة المرفوضة\n"
        + (html.Length > 120_000 ? html[..120_000] : html) + "\n";
}
