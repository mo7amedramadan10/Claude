using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Llm;

namespace ChatToDashboard.Api.Analyst;

/// <summary>The model's structured final answer — see spec section 5 step 5. Never carries row
/// values; AnswerTemplate's placeholders are filled separately (see AnalystPlaceholders) from
/// whichever captured query ResultQueryId points at (see AnalystOllamaLoop).</summary>
public record AnalystModelResult(
    string Title, string ResultType, string? ResultQueryId, string? KeyColumn,
    string? PrimaryMeasure, string AnswerTemplate, string? AuditSql);

public static class AnalystPrompts
{
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Step 1 of the pipeline (spec section 5) — a cheap, tool-free, single-word
    /// classification so the controller knows whether to run the tool loop at all. Kept
    /// separate from the main analyst system prompt so this stays a fast, focused call.</summary>
    public const string ClassifySystemPrompt =
        "أنت تصنّف سؤال مستخدم في أداة تحليل بيانات مؤسسية. ردّ بكلمة واحدة فقط من الثلاث دول، " +
        "من غير أي شرح أو علامات ترقيم:\n" +
        "- org_data: السؤال عن بيانات المشروع (مبيعات، مناديب، عملاء، مخزون، ميزانية، أداء، مقارنات أرقام...).\n" +
        "- general: سؤال عام مش له علاقة ببيانات المشروع، لكن له إجابة معروفة (تعريف مصطلح، نصيحة عامة، سؤال عام).\n" +
        "- out_of_scope: سؤال مش منطقي، أو مطلوب فيه حاجة خارج نطاق التحليل (مثلاً طلب إنشاء لوحة أو تعديل بيانات).\n" +
        "رد بالكلمة بس: org_data أو general أو out_of_scope.";

    public static string ClassifyUserMessage(string question) => question.Trim();

    /// <summary>For a "general" question — no tools offered, so the model answers from its own
    /// knowledge; still ends in the same JSON contract (result_query_id/audit_sql null) so the
    /// caller has one parsing path regardless of scope. See spec section 2 point 2: a general-
    /// knowledge answer carries no verification badge (nothing to verify), only the "من خارج
    /// البيانات" label the frontend shows (Phase 2).</summary>
    public const string GeneralKnowledgeSystemPrompt = """
        أنت «المحلل الذكي» في منصة جيم. السؤال ده مش عن بيانات المشروع — جاوب من معرفتك العامة،
        بإيجاز ووضوح، بالعربية.

        رُد بكائن JSON واحد بس (من غير أي نص أو markdown حواليه) بالشكل ده بالظبط:
        {
          "title": "عنوان قصير للسؤال",
          "result_type": "summary",
          "result_query_id": null,
          "key_column": null,
          "primary_measure": null,
          "answer_template": "الإجابة الكاملة بالعربي",
          "audit_sql": null
        }
        """;

    public static AnalystKnowledgeScope ParseClassification(string text)
    {
        var normalized = text.Trim().ToLowerInvariant();
        if (normalized.Contains(AnalystKnowledgeScopes.OutOfScope)) return AnalystKnowledgeScope.OutOfScope;
        if (normalized.Contains(AnalystKnowledgeScopes.General)) return AnalystKnowledgeScope.General;
        if (normalized.Contains(AnalystKnowledgeScopes.OrgData)) return AnalystKnowledgeScope.OrgData;
        // An ambiguous/malformed classification defaults to attempting real work over silently
        // answering from general knowledge — see the Phase 1 report for why.
        return AnalystKnowledgeScope.OrgData;
    }

    /// <summary>The main tool-loop system prompt — list_files/query_data/query_client_data
    /// (reusing AnalyticsTools.BuildTools/ExecuteToolAsync verbatim, see AnalystOllamaLoop),
    /// ending in the fixed JSON contract from spec section 5. The placeholder key vocabulary is
    /// explained so the model knows what it CAN reference in answer_template — it is never shown
    /// an actual row value.</summary>
    // A plain (non-interpolated) raw string — the prompt text itself uses literal {{...}}
    // braces to describe the placeholder syntax to the model, which would collide with C#'s
    // $$"""...""" interpolation delimiter if this were interpolated. {{SOURCES}} below is
    // substituted with a plain .Replace() instead, not left to the compiler.
    private const string AnalystSystemPromptTemplate = """
        أنت «المحلل الذكي» في منصة جيم — تجاوب على أسئلة تحليلية عن بيانات مشروع حقيقي، بدقة
        تامة، من غير اختراع أي رقم أو واقعة.

        {{SOURCES}}

        القواعد الأساسية
            - كل نتيجة query_data/query_client_data ناجحة بترجعلك نص يبدأ بـ "[query id: qN]" —
              الـ qN ده هو المعرّف اللي تستخدمه في result_query_id تحت، نسخة حرفية منه بالظبط.
            - نادِ list_files أولًا لمعرفة الجداول/الأعمدة المتاحة فعليًا، ثم query_data لجلب
              البيانات الحقيقية. ممنوع تمامًا تفترض اسم جدول أو عمود من غير ما تتأكد منه.
            - كل استعلام SELECT للقراءة فقط.
            - لو مفيش مصدر بيانات يجاوب على السؤال فعليًا، قول كده صراحة — ممنوع التخمين.

            الرد النهائي — إلزامي
            لما توصل لإجابة نهائية مبنية على بيانات حقيقية جلبتها، رُد بكائن JSON واحد بس (من غير
            أي نص أو markdown حواليه) بالشكل ده بالظبط:
            {
              "title": "عنوان قصير للنتيجة",
              "result_type": "table | ranking | comparison | summary",
              "result_query_id": "معرّف نداء query_data اللي فيه الجدول النهائي (مثلاً q2) — لازم يكون نداء فعلي ناديته",
              "key_column": "اسم العمود اللي بيمثل التسمية/الاسم في كل صف (أو null)",
              "primary_measure": "اسم العمود الرقمي الأساسي اللي بيتبني عليه الترتيب (أو null)",
              "answer_template": "نص الإجابة بالعربي، بديل أي رقم فيه بـ {{مفتاح}} من القائمة تحت — ممنوع تكتب رقم حرفي",
              "audit_sql": "استعلام SELECT مستقل بيحسب نفس الإجمالي الأساسي من غير GROUP BY، للتحقق لاحقًا (أو null لو مفيش قياس مجمّع)"
            }

            مفاتيح answer_template المتاحة (تقدر تستخدمها جوه {{...}} بس، وتقدر تضيف |n أو |p أو |c
            أو |d بعد أي مفتاح رقمي للتنسيق — رقم بفواصل، نسبة مئوية، مختصر ألف/مليون، أو تاريخ):
            - rows — عدد الصفوف.
            - sum.<عمود>, avg.<عمود>, min.<عمود>, max.<عمود> — لأي عمود رقمي فعلي في النتيجة.
            - top1.<عمود> إلى top5.<عمود> — قيمة العمود في الصف الأعلى (١ إلى ٥) حسب primary_measure.
            - bottom1.<عمود> إلى bottom3.<عمود> — نفس الفكرة من الأسفل.
            - share_top3.<عمود> — نسبة مجموع أعلى ٣ صفوف من إجمالي العمود.
            ممنوع تمامًا تكتب أي رقم حرفي في answer_template — كل رقم لازم يكون من المفاتيح دي بس.
            لو السؤال عن سؤال عام (مش بيانات المشروع) أو خارج النطاق، رُد بنفس شكل JSON لكن
        result_query_id يبقى null وanswer_template يحتوي الإجابة النصية من غير أي رقم محسوب.
        """;

    public static string BuildAnalystSystemPrompt(AnalyticsTools.SourceContext context)
    {
        var sourcesBlock =
            "المصادر المفعّلة حاليًا\n" +
            $"- أنظمة: {(context.EnabledSystems.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledSystems))}\n" +
            $"- ملفات: {(context.EnabledFiles.Count == 0 ? "(لا يوجد)" : string.Join("، ", context.EnabledFiles))}";

        return AnalystSystemPromptTemplate.Replace("{{SOURCES}}", sourcesBlock);
    }

    public static (AnalystModelResult? Result, string? Error) TryParseAnalystResult(string text)
    {
        var (candidate, extractError) = AnalyticsTools.ExtractJsonCandidate(text);
        if (candidate is null) return (null, extractError);
        try
        {
            var node = JsonNode.Parse(candidate)?.AsObject();
            if (node is null) return (null, "رد الموديل مش JSON صالح.");

            var title = node["title"]?.GetValue<string>();
            var resultType = node["result_type"]?.GetValue<string>();
            var answerTemplate = node["answer_template"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(answerTemplate))
                return (null, "title وanswer_template مطلوبين.");
            if (!AnalystResultTypes.IsValid(resultType))
                return (null, $"result_type غير معروف: {resultType}");

            return (new AnalystModelResult(
                title.Trim(),
                resultType!,
                node["result_query_id"]?.GetValue<string>(),
                node["key_column"]?.GetValue<string>(),
                node["primary_measure"]?.GetValue<string>(),
                answerTemplate.Trim(),
                node["audit_sql"]?.GetValue<string>()), null);
        }
        catch (JsonException ex)
        {
            return (null, ex.Message);
        }
    }
}

public enum AnalystKnowledgeScope { OrgData, General, OutOfScope }
