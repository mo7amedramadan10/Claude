using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Users;
using UglyToad.PdfPig;

namespace ChatToDashboard.Api.Integrations;

/// <summary>One integration's visual-identity token set, exactly as it will be stored/applied —
/// the same three tokens every path (manual, AI-suggested, deterministic extraction, vision
/// extraction) produces. <see cref="Adjusted"/>/<see cref="AdjustmentNote"/> are set only when
/// ContrastValidator had to darken a model-produced color (paths 2 and 3c) — never for manual
/// entry or deterministic palette extraction, which the analyst chose on purpose.</summary>
public record VisualIdentityResult(
    string AccentColor, string SecondaryColor, string FontFamily,
    bool Adjusted, string? AdjustmentNote);

/// <summary>
/// Implements every path of Part B ("Visual identity per integration"). Each of the three
/// analyst-facing paths maps to exactly one public method here:
///   1. Manual  -> ValidateManual (no model call at all)
///   2. AI-assisted suggestion -> SuggestAsync (text-only model call + contrast guard)
///   3. Extraction:
///      - image palette -> ExtractPaletteFromImage (deterministic, ColorClusterer, no AI)
///      - PDF font name -> ExtractFontFromPdfAsync (PdfPig text + text-only model call)
///      - genuine image/logo understanding -> ExtractFromImageAsync (vision model, gated on
///        LlmSettingsStore.VisualIdentitySupportsImage via VisualIdentityImageRouter)
/// Only the token *values* ever change across these paths — nothing here touches how the
/// viewer.html deliverable renders a dashboard; see IntegrationDeliverables.
/// </summary>
public class VisualIdentityService
{
    // Somewhat arbitrary but reasonable: a brand's own body-text color chosen this way is
    // its own business, but the FIRST accent/secondary pair is picked from the extracted
    // palette by frequency, not filtered for contrast the way SuggestAsync's model output is —
    // Part B explicitly scopes path 3a as deterministic-only, no validation step described.
    private const int DefaultPaletteSize = 5;

    private readonly IIntegrationSetupAssistant _assistant;
    private readonly VisualIdentityImageRouter _imageRouter;
    private readonly ILogger<VisualIdentityService> _logger;

    public VisualIdentityService(
        IIntegrationSetupAssistant assistant, VisualIdentityImageRouter imageRouter, ILogger<VisualIdentityService> logger)
    {
        _assistant = assistant;
        _imageRouter = imageRouter;
        _logger = logger;
    }

    /// <summary>Path 1 — manual entry. Purely a format check; the analyst's exact values are
    /// used verbatim (never contrast-adjusted — they typed them on purpose).</summary>
    public static (bool Valid, string? Error) ValidateManual(string? accentColor, string? secondaryColor, string? fontFamily)
    {
        if (!ContrastValidator.IsValidHex(accentColor) || !ContrastValidator.IsValidHex(secondaryColor))
            return (false, "لون غير صالح — استخدم صيغة hex بالشكل #RRGGBB.");
        if (string.IsNullOrWhiteSpace(fontFamily))
            return (false, "اسم الخط مطلوب.");
        return (true, null);
    }

    /// <summary>Path 2 — AI-assisted suggestion. At least one of <paramref name="description"/>/
    /// <paramref name="seedColor"/> must be given. Null means the model call itself failed or
    /// returned something unusable (IIntegrationSetupAssistant never throws) — the caller's
    /// fallback is manual entry, same as any other suggestion failure in this app.</summary>
    public async Task<VisualIdentityResult?> SuggestAsync(
        string? description, string? seedColor, AppUser? requestingUser, CancellationToken ct)
    {
        var suggestion = await _assistant.SuggestVisualIdentityAsync(description, seedColor, requestingUser, ct);
        return suggestion is null ? null : ApplyContrastGuard(suggestion);
    }

    /// <summary>Path 3a — deterministic color-palette extraction from an uploaded image. No AI
    /// model involved; returns the two most dominant non-near-duplicate colors as accent/
    /// secondary, with a neutral default font (the analyst can still change it manually — an
    /// image carries no font information at all). Empty list means the image had no readable
    /// pixels (e.g. fully transparent).</summary>
    public VisualIdentityResult? ExtractPaletteFromImage(byte[] imageBytes)
    {
        var colors = ColorClusterer.ExtractDominantColors(imageBytes, DefaultPaletteSize);
        if (colors.Count == 0) return null;
        var accent = colors[0];
        var secondary = colors.Count > 1 ? colors[1] : accent;
        return new VisualIdentityResult(accent, secondary, "Inter", false, null);
    }

    /// <summary>Path 3, text-only half — reads a PDF's real text layer (PdfPig, same library
    /// DocumentSearchService already uses) and asks the text-only model to locate an EXPLICIT
    /// font-name mention already written in it. Null means either the PDF had no extractable
    /// text or the text never names a font — both fall back to manual entry.</summary>
    public async Task<string?> ExtractFontFromPdfAsync(byte[] pdfBytes, AppUser? requestingUser, CancellationToken ct)
    {
        string text;
        try
        {
            using var stream = new MemoryStream(pdfBytes);
            using var pdf = PdfDocument.Open(stream);
            var sb = new System.Text.StringBuilder();
            foreach (var page in pdf.GetPages()) sb.AppendLine(page.Text);
            text = sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read PDF text for visual-identity font extraction");
            return null;
        }
        if (string.IsNullOrWhiteSpace(text)) return null;
        return await _assistant.SuggestFontFromPdfTextAsync(text, requestingUser, ct);
    }

    /// <summary>Path 3, vision half — "genuine visual/image understanding (e.g. a logo image
    /// with no accompanying text)". Refuses up front with a clear Arabic reason (never sends
    /// the image) when nothing is configured, or when a provider IS configured but not
    /// explicitly flagged as supporting image input — see LlmSettingsStore.
    /// VisualIdentitySupportsImage and this app's /الإعدادات screen.</summary>
    public async Task<(VisualIdentityResult? Result, string? RefusalReason)> ExtractFromImageAsync(
        string imageDataUrl, AppUser? requestingUser, CancellationToken ct)
    {
        var (configured, supportsImage) = await _imageRouter.DescribeAsync(ct);
        if (!configured)
            return (null, "مفيش نموذج مُعد لاستخراج الهوية البصرية من الصور — اضبطه من الإعدادات أولاً.");
        if (!supportsImage)
            return (null, "النموذج المُعد لاستخراج الهوية البصرية غير مؤشَّر عليه كداعم لإدخال الصور — " +
                          "اختر موديل يدعم الصور من الإعدادات وفعّل العلامة الخاصة بذلك، أو ارفع بدلًا من " +
                          "الصورة وصفًا نصيًا أو ملف PDF فيه نص.");

        var suggestion = await _imageRouter.ExtractVisualIdentityFromImageAsync(imageDataUrl, requestingUser, ct);
        if (suggestion is null)
            return (null, "تعذّر استخراج هوية بصرية من هذه الصورة.");
        return (ApplyContrastGuard(suggestion), null);
    }

    private static VisualIdentityResult ApplyContrastGuard(VisualIdentitySuggestion suggestion)
    {
        var (accent, accentAdjusted) = ContrastValidator.EnsureContrastAgainstWhite(suggestion.AccentColor);
        var (secondary, secondaryAdjusted) = ContrastValidator.EnsureContrastAgainstWhite(suggestion.SecondaryColor);
        var adjusted = accentAdjusted || secondaryAdjusted;
        string? note = null;
        if (adjusted)
        {
            var parts = new List<string>();
            if (accentAdjusted) parts.Add($"اللون الأساسي عُدِّل من {suggestion.AccentColor} إلى {accent} لتحسين التباين.");
            if (secondaryAdjusted) parts.Add($"اللون الثانوي عُدِّل من {suggestion.SecondaryColor} إلى {secondary} لتحسين التباين.");
            note = string.Join(" ", parts);
        }
        return new VisualIdentityResult(accent, secondary, suggestion.FontFamily, adjusted, note);
    }
}
