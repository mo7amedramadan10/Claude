using System.Text;
using ChatToDashboard.Api.Decks;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>استيراد ملفات موجودة إلى عرض: PowerPoint (مولَّد ببنّاء PPTX نفسه)، HTML، وPDF بسيط.</summary>
public class DeckImporterTests
{
    [Fact]
    public void Pptx_RoundTrip_TitleBulletsTableKpis()
    {
        var pptx = DeckPptxBuilder.Build(new DeckPptxRequest
        {
            Title = "نتائج الربع",
            Slides =
            {
                new DeckPptxSlideInput { Title = "نتائج الربع الثالث", Subtitle = "إدارة المبيعات", IsTitle = true },
                new DeckPptxSlideInput { Title = "أبرز الملاحظات", Bullets = new() { "نمو 12٪", "الرياض 48٪ من الإيراد", "تراجع متوسط الصفقة" } },
                new DeckPptxSlideInput { Title = "أهم العملاء", Columns = new() { "العميل", "الإيراد" }, Rows = new() { new() { "الأفق", "820,000" }, new() { "النخيل", "610,000" } } },
            },
        });
        var doc = DeckImporter.Extract("q3.pptx", pptx);
        Assert.Equal("pptx", doc.Kind);
        Assert.Equal(3, doc.Pages.Count);
        Assert.Contains(doc.Pages[1].Paragraphs, p => p.Contains("نمو 12٪"));
        Assert.Single(doc.Pages[2].Tables);
        Assert.Equal(new[] { "العميل", "الإيراد" }, doc.Pages[2].Tables[0].Columns);

        var spec = DeckImporter.ToSpec(doc);
        Assert.Equal("title", spec.Slides[0].Layout);
        Assert.Contains(spec.Slides, s => s.Layout == "bullets" && s.Bullets!.Count == 3);
        Assert.Contains(spec.Slides, s => s.Layout == "table" && s.Table!.Rows.Count == 2);
    }

    [Fact]
    public void Html_HeadingsListsTablesImages()
    {
        var html = """
            <html><head><title>خطة 2027</title><style>h1{color:red}</style></head><body>
            <h1>الأهداف</h1><p>نرفع الإيراد.</p><ul><li>هدف أول</li><li>هدف ثانٍ</li></ul>
            <h2>الأرقام</h2><ul><li>الإيراد: 4.2M</li><li>الصفقات: 318</li><li>النمو: 12٪</li></ul>
            <h2>الجدول</h2><table><tr><th>البند</th><th>القيمة</th></tr><tr><td>أ</td><td>1</td></tr></table>
            <h2>صورة</h2><img src="data:image/png;base64,iVBORw0KGgo="><img src="https://x/y.png">
            <script>alert(1)</script></body></html>
            """;
        var doc = DeckImporter.Extract("plan.html", Encoding.UTF8.GetBytes(html));
        Assert.Equal("خطة 2027", doc.Title);
        Assert.Equal(4, doc.Pages.Count);
        Assert.Equal("الأهداف", doc.Pages[0].Title);
        Assert.Equal(3, doc.Pages[0].Paragraphs.Count);
        Assert.Single(doc.Pages[2].Tables);
        Assert.Single(doc.Pages[3].Images);
        Assert.Contains(doc.Warnings, w => w.Contains("صورة خارجية"));
        Assert.DoesNotContain(doc.Pages.SelectMany(p => p.Paragraphs), p => p.Contains("alert"));

        var spec = DeckImporter.ToSpec(doc);
        var kpi = Assert.Single(spec.Slides, s => s.Layout == "kpis");
        Assert.Equal(3, kpi.Kpis!.Count); Assert.Equal("4.2M", kpi.Kpis[0].Value);
        Assert.Contains(spec.Slides, s => s.Layout == "image");
    }

    [Fact]
    public void Pdf_MinimalText_PagesAndTitle()
    {
        var pdf = MinimalPdf(new[] { ("Quarterly Review", "Revenue grew 12 percent"), ("Next steps", "Hire four engineers") });
        var doc = DeckImporter.Extract("review.pdf", pdf);
        Assert.Equal("pdf", doc.Kind);
        Assert.Equal(2, doc.Pages.Count);
        Assert.Equal("Quarterly Review", doc.Pages[0].Title);
        Assert.Contains(doc.Pages[0].Paragraphs, p => p.Contains("12 percent"));
        var spec = DeckImporter.ToSpec(doc);
        Assert.Equal("title", spec.Slides[0].Layout);
        Assert.Equal("Quarterly Review", spec.Title);
    }

    [Fact]
    public void Unsupported_Extension_Throws()
    {
        Assert.Null(DeckImporter.KindOf("x.docx"));
        Assert.Throws<InvalidOperationException>(() => DeckImporter.Extract("x.docx", new byte[] { 1 }));
    }

    /// <summary>PDF يدوي بسيط (Helvetica، بلا ضغط): سطر عنوان كبير وسطر نص لكل صفحة.</summary>
    private static byte[] MinimalPdf((string Title, string Body)[] pages)
    {
        var objs = new List<string>();
        var kids = new List<int>();
        var pageObjStart = 4;
        for (var i = 0; i < pages.Length; i++) { kids.Add(pageObjStart + i * 2); }
        objs.Add("<< /Type /Catalog /Pages 2 0 R >>");
        objs.Add($"<< /Type /Pages /Kids [{string.Join(" ", kids.Select(k => k + " 0 R"))}] /Count {pages.Length} >>");
        objs.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        foreach (var (t, b) in pages)
        {
            var content = $"BT /F1 24 Tf 72 720 Td ({t}) Tj ET\nBT /F1 12 Tf 72 680 Td ({b}) Tj ET";
            objs.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {objs.Count + 2} 0 R >>");
            objs.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream");
        }
        var sb = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
        for (var i = 0; i < objs.Count; i++) { offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString())); sb.Append($"{i + 1} 0 obj\n{objs[i]}\nendobj\n"); }
        var xref = Encoding.ASCII.GetByteCount(sb.ToString());
        sb.Append($"xref\n0 {objs.Count + 1}\n0000000000 65535 f \n");
        foreach (var o in offsets) sb.Append($"{o:D10} 00000 n \n");
        sb.Append($"trailer\n<< /Size {objs.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}
