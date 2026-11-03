using System.Text.Json.Serialization;
using ChatToDashboard.Api.Export;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ChatToDashboard.Api.Decks;

/// <summary>شريحة للتصدير: نص (عنوان + نقاط) أو صورة (لقطة PNG للرسم من المتصفح) أو جدول أو مؤشرات.</summary>
public class DeckPptxSlideInput
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("subtitle")] public string? Subtitle { get; set; }
    [JsonPropertyName("bullets")] public List<string>? Bullets { get; set; }
    [JsonPropertyName("columns")] public List<string>? Columns { get; set; }
    [JsonPropertyName("rows")] public List<List<string>>? Rows { get; set; }
    [JsonPropertyName("kpis")] public List<DeckKpi>? Kpis { get; set; }
    [JsonPropertyName("image")] public string? Image { get; set; }
    [JsonPropertyName("notes")] public string? Notes { get; set; }
    [JsonPropertyName("source")] public string? Source { get; set; }
    [JsonPropertyName("isTitle")] public bool IsTitle { get; set; }
}

public class DeckPptxRequest
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("slides")] public List<DeckPptxSlideInput> Slides { get; set; } = new();
}

/// <summary>يبني ملف PPTX شريحةً بشريحة من العرض (بخلاف تصدير اللوحات الذي يجمّع عناصر في شريحة) بإعادة استخدام
/// أدوات <see cref="PptxBuilder"/>: نص عربي RTL، جداول، صور الرسوم، ومؤشرات كصناديق.</summary>
public static class DeckPptxBuilder
{
    public static byte[] Build(DeckPptxRequest request)
    {
        using var stream = new MemoryStream();
        using (var doc = PresentationDocument.Create(stream, PresentationDocumentType.Presentation))
        {
            var presentationPart = doc.AddPresentationPart();
            presentationPart.Presentation = new P.Presentation();
            var master = PptxBuilder.CreateSlideMaster(presentationPart);
            var layout = master.SlideLayoutParts.First();
            var parts = request.Slides.Select(s => BuildSlide(presentationPart, layout, s)).ToList();
            presentationPart.Presentation.Append(new P.SlideMasterIdList(new P.SlideMasterId { Id = 2147483648U, RelationshipId = presentationPart.GetIdOfPart(master) }));
            var ids = new P.SlideIdList(); uint id = 256;
            foreach (var p in parts) ids.Append(new P.SlideId { Id = id++, RelationshipId = presentationPart.GetIdOfPart(p) });
            presentationPart.Presentation.Append(ids);
            presentationPart.Presentation.Append(new P.SlideSize { Cx = (int)PptxBuilder.SlideWidth, Cy = (int)PptxBuilder.SlideHeight, Type = P.SlideSizeValues.Screen16x9 });
            presentationPart.Presentation.Append(new P.NotesSize { Cx = 6858000, Cy = 9144000 });
            presentationPart.Presentation.Save();
        }
        return stream.ToArray();
    }

    private static SlidePart BuildSlide(PresentationPart presentationPart, SlideLayoutPart layout, DeckPptxSlideInput s)
    {
        var part = PptxBuilder.NewSlidePart(presentationPart, layout);
        var tree = part.Slide!.CommonSlideData!.ShapeTree!;
        uint next = 2;
        var margin = PptxBuilder.Margin; var w = PptxBuilder.ContentWidth;
        if (s.IsTitle)
        {
            tree.Append(PptxBuilder.TextBox(next++, s.Title, margin, PptxBuilder.SlideHeight / 3, w, 1100000L, 40, bold: true, colorHex: "2A78D6", align: A.TextAlignmentTypeValues.Center));
            if (!string.IsNullOrWhiteSpace(s.Subtitle)) tree.Append(PptxBuilder.TextBox(next++, s.Subtitle, margin, PptxBuilder.SlideHeight / 3 + 1200000L, w, 700000L, 20, colorHex: "52514E", align: A.TextAlignmentTypeValues.Center));
            return part;
        }
        tree.Append(PptxBuilder.TextBox(next++, s.Title, margin, PptxBuilder.TitleTop, w, PptxBuilder.TitleHeight, 28, bold: true, colorHex: "0B3D5C"));
        var top = PptxBuilder.ContentTop; var h = PptxBuilder.ContentHeight - (string.IsNullOrWhiteSpace(s.Source) ? 0 : 400000L);
        if (s.Kpis is { Count: > 0 })
        {
            var n = Math.Min(s.Kpis.Count, 4); var gap = 228600L; var cw = (w - gap * (n - 1)) / n;
            for (var i = 0; i < n; i++)
            {
                var x = margin + (long)i * (cw + gap);
                tree.Append(PptxBuilder.TextBox(next++, s.Kpis[i].Value, x, top + 400000L, cw, 900000L, 36, bold: true, colorHex: "2A78D6", align: A.TextAlignmentTypeValues.Center));
                tree.Append(PptxBuilder.TextBox(next++, s.Kpis[i].Label + (string.IsNullOrWhiteSpace(s.Kpis[i].Delta) ? "" : "  " + s.Kpis[i].Delta), x, top + 1350000L, cw, 600000L, 16, colorHex: "52514E", align: A.TextAlignmentTypeValues.Center));
            }
        }
        else if (s.Rows is { Count: > 0 } && s.Columns is { Count: > 0 })
            tree.Append(PptxBuilder.BuildTable(next++, new PptxWidgetInput { Type = "table", Columns = s.Columns, Rows = s.Rows }, margin, top, w, h));
        else if (!string.IsNullOrWhiteSpace(s.Image) && PptxBuilder.TryDecodeImage(s.Image) is { } png)
            tree.Append(PptxBuilder.BuildPicture(part, next++, png, margin, top, w, h));
        else if (s.Bullets is { Count: > 0 })
            tree.Append(PptxBuilder.TextBox(next++, string.Join("\n", s.Bullets.Select(b => "• " + b)), margin, top, w, h, 20));
        if (!string.IsNullOrWhiteSpace(s.Source))
            tree.Append(PptxBuilder.TextBox(next++, s.Source, margin, PptxBuilder.SlideHeight - margin - 350000L, w, 350000L, 10, colorHex: "898781"));
        return part;
    }
}
