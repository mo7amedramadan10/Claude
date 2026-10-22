using System.Text.Json.Nodes;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.Llm;

/// <summary>
/// نداء واحد محدود النطاق في البناء المرحلي للقوالب (انظر Build/StagedTemplateBuilder): تعليمات
/// نظام ثابتة، ورسالة مستخدم واحدة، وأدوات اختيارية، وسقف دورات صغير، والمخرج كائن JSON واحد.
/// الفرق عن مسار GenerateDashboardAsync الكامل: لا تعليمات اللوحة العامة الطويلة، ولا سقف
/// الـ15 دورة، ولا إجبار على list_files — كل نداء يعرف مهمته الصغيرة فقط.
/// </summary>
public sealed record StagedCall(
    /// <summary>وصف قصير يظهر في سجل الاستهلاك (مثل «[قالب] مسار المبيعات — تخطيط»).</summary>
    string Label,
    string SystemPrompt,
    string UserMessage,
    /// <summary>الأدوات المتاحة لهذا النداء (عادةً query_data فقط)، أو null لنداء بلا أدوات.</summary>
    IReadOnlyList<ToolSpec>? Tools,
    AnalyticsTools.SourceContext Context,
    /// <summary>أقصى عدد دورات (رد من الموديل) قبل إجباره على الإجابة النهائية.</summary>
    int MaxIterations,
    /// <summary>هل يُجبر الموديل على نداء أداة في الدورة الأولى؟ (رسمة لا تُبنى بلا استعلام).</summary>
    bool ForceFirstToolCall,
    AppUser? RequestingUser,
    /// <summary>موديل محدد لهذا النداء بدل الموديل المحفوظ في الإعدادات (مثل موديل تحليل معاني الأعمدة)، أو null.</summary>
    string? ModelOverride = null);

/// <summary>نتيجة نداء مرحلي: نص الرد الخام وكائن JSON المفسَّر منه.</summary>
public sealed record StagedResult(string Text, JsonObject Json);

/// <summary>ينفّذه كل مزوّد: OpenAI وAnthropic وOllama (الأخير بمحاكاة إجبار الأداة لغياب tool_choice).</summary>
public interface IStagedLlm
{
    string ProviderName { get; }

    Task<StagedResult> RunStagedAsync(StagedCall call, CancellationToken ct = default);
}
