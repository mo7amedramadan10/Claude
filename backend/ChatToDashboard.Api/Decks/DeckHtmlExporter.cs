using System.Text;
using System.Text.Json;

namespace ChatToDashboard.Api.Decks;

/// <summary>
/// يحوّل مواصفة العرض إلى صفحة HTML مستقلة تُنشر في «العروض التقديمية»: تضمّن محرك العرض نفسه (`jeem-deck.js`)
/// والمواصفة JSON وجسر «بيانات العرض» للكتل القابلة للتعديل، فتبدو كما في المنصة تمامًا وتعمل بلا اتصال بالمحرك.
/// </summary>
public static class DeckHtmlExporter
{
    public static string Build(DeckSpec spec, string deckRuntimeJs, bool includeBridge)
    {
        var json = JsonSerializer.Serialize(spec, DeckSpec.Json).Replace("</", "<\\/");
        var title = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(spec.Title) ? "عرض تقديمي" : spec.Title);
        var sb = new StringBuilder();
        sb.Append("<!doctype html>\n<html lang=\"ar\" dir=\"rtl\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
        sb.Append("<title>").Append(title).Append("</title>\n");
        var font = spec.Brand?.Font is { } f && DeckBrand.Fonts.Contains(f) ? f : "IBM Plex Sans Arabic";
        sb.Append("<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\"><link rel=\"stylesheet\" href=\"https://fonts.googleapis.com/css2?family=").Append(Uri.EscapeDataString(font).Replace("%20", "+")).Append(":wght@400;500;600;700&display=swap\">\n");
        sb.Append("<style>").Append(DeckRuntimeCss);
        if (spec.Brand?.FontFaces is { Count: > 0 } faces) foreach (var ff in faces) sb.Append(ff.FontFaceCss());
        sb.Append("</style>\n</head>\n<body>\n");
        if (includeBridge) sb.Append("<script src=\"/js/jeem-page.js\"></script>\n");
        sb.Append("<div id=\"deck\" class=\"jeem-deck-root\"></div>\n");
        sb.Append("<script type=\"application/json\" id=\"deck-spec\">").Append(json).Append("</script>\n");
        sb.Append("<script>\n").Append(deckRuntimeJs).Append("\n</script>\n");
        sb.Append("<script>JEEM_DECK.present(document.getElementById('deck'), JSON.parse(document.getElementById('deck-spec').textContent), { editableStore: window.jeem ? jeem.store : null, viewer: window.jeem ? jeem.viewer : null });</script>\n");
        sb.Append("</body>\n</html>\n");
        return sb.ToString();
    }

    /// <summary>الحد الأدنى من أنماط صفحة العرض المستقلة (المحرك يحقن أنماط الشرائح بنفسه).</summary>
    private const string DeckRuntimeCss = "html,body{margin:0;height:100%;background:#0f1720;font-family:'IBM Plex Sans Arabic',Tahoma,Arial,sans-serif}.jeem-deck-root{height:100%}";
}
