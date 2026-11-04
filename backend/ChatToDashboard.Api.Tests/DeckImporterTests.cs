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

/// <summary>صفحات التطبيقات التفاعلية (مثل صفحة المستخدم «دور الإدارة في مشروع الخوارزمي»): عناصر الواجهة تُتجاهل، الروابط تُجمع،
/// والمحتوى الحقيقي في سكربتات الصفحة يُلتقط كبيانات لوضع الفهم بالموديل.</summary>
public class DeckImporterAppPageTests
{
    private const string Page = """
        <!doctype html><html><head><title>دور الإدارة في المشروع</title></head><body>
        <div class="wrap" dir="rtl">
          <header><span class="eyebrow">ثقة · مشروع مركز الاتصال</span><h1>دور الإدارة في المشروع</h1></header>
          <nav class="tabs" role="tablist"><button role="tab">دور الإدارة</button><button role="tab">المهام</button></nav>
          <div class="view" hidden="">
            <h2>خريطة العلاقات</h2><span class="hint">اضغط على أي طرف لإبراز علاقاته</span>
            <button class="node"><span class="nm">محمد الشربيني</span></button>
            <p class="empty">اختر طرفًا من الخريطة لعرض دوره.</p>
          </div>
          <div class="view" hidden=""><h2>الروابط</h2><ul>
            <li><a href="https://docs.google.com/document/d/1">تسجيل الاجتماعات <span class="lk-s">مستند Google</span><span aria-hidden="true">↗</span></a></li>
            <li><a href="https://docs.google.com/spreadsheets/d/2">خطة الرفع</a></li></ul></div>
          <section><article><h2>الدور</h2><p>تتولى الإدارة المسؤولية التقنية الكاملة عن المشروع.</p></article></section>
          <section><h2>المسؤوليات</h2><ol id="stepper"></ol>
            <div class="nav"><button>→ السابق</button><button>التالي ←</button></div></section>
        </div>
        <script type="application/json" id="tk-data">{"sections":[{"title":"المتابعة مع أحمد","items":[{"t":"Public IP","done":false}]}]}</script>
        <script>
        const R=[{id:"requirements",t:"متابعة المتطلبات",m:["يوفّر المورد مصفوفة تتبع متطلبات موحّدة","يشارك المورد محضر كل جلسة"],o:["مصفوفة تتبع المتطلبات"]}];
        const $=id=>document.getElementById(id); const cls="node dim"; const x='#panel';
        </script></body></html>
        """;

    [Fact]
    public void UiChrome_IsSkipped_AndLinksCollected()
    {
        var doc = DeckImporter.Extract("role.html", Encoding.UTF8.GetBytes(Page));
        var all = doc.Pages.SelectMany(p => p.Paragraphs).ToList();
        Assert.DoesNotContain(all, x => x.Contains("السابق") || x.Contains("التالي") || x.Contains("دور الإدارة‏") && false);
        Assert.DoesNotContain(all, x => x.Contains("اضغط على"));
        Assert.DoesNotContain(all, x => x.Contains("محمد الشربيني"));
        Assert.DoesNotContain(all, x => x == "دور الإدارة" || x == "المهام");
        Assert.Contains(all, x => x.Contains("المسؤولية التقنية الكاملة"));
        var links = doc.Pages.Single(p => p.Title == "الروابط").Links;
        Assert.Equal(2, links.Count);
        Assert.Equal("تسجيل الاجتماعات مستند Google", links[0].Text);
        Assert.Equal("https://docs.google.com/document/d/1", links[0].Href);

        var spec = DeckImporter.ToSpec(doc);
        var linksSlide = Assert.Single(spec.Slides, s => s.Title == "الروابط");
        Assert.Contains(linksSlide.Bullets!, b => b.StartsWith("خطة الرفع — https://"));
    }

    [Fact]
    public void DataBlocks_JsonAndScriptStrings_Collected_NotCode()
    {
        var doc = DeckImporter.Extract("role.html", Encoding.UTF8.GetBytes(Page));
        Assert.Equal(2, doc.DataBlocks.Count);
        Assert.StartsWith("JSON:", doc.DataBlocks[0]);
        Assert.Contains("Public IP", doc.DataBlocks[0]);
        Assert.Contains("يوفّر المورد مصفوفة تتبع متطلبات موحّدة", doc.DataBlocks[1]);
        Assert.Contains("مصفوفة تتبع المتطلبات", doc.DataBlocks[1]);
        Assert.DoesNotContain("node dim", doc.DataBlocks[1]);
        Assert.DoesNotContain("#panel", doc.DataBlocks[1]);
        Assert.DoesNotContain("requirements", doc.DataBlocks[1]);

        var input = DeckImporter.BuildUnderstandInput(doc);
        Assert.Contains("## 1.", input);
        Assert.Contains("رابط: خطة الرفع — https://docs.google.com/spreadsheets/d/2", input);
        Assert.Contains("بيانات مضمّنة في الصفحة", input);
    }

    [Fact]
    public void ApplyUnderstood_ParsesTasksAndImages()
    {
        var doc = DeckImporter.Extract("x.html", Encoding.UTF8.GetBytes("<html><body><h1>ع</h1><p>نص</p><img src=\"data:image/png;base64,iVBORw0KGgo=\"></body></html>"));
        var json = System.Text.Json.Nodes.JsonNode.Parse("""
            {"spec":{"title":"العرض","slides":[
              {"layout":"title","title":"العرض"},
              {"layout":"tasks","title":"المهام","tasks":[{"t":"Public IP","done":false},{"t":"","done":true}]},
              {"layout":"image","title":"صورة","image":{"src":"image:0-0"}},
              {"layout":"image","title":"مجهولة","image":{"src":"image:9-9"}},
              {"layout":"two-column","title":"آلية/مخرجات","columns":[{"title":"الآلية","bullets":["أ"]},{"title":"المخرجات","bullets":["ب"]}]}
            ]}}
            """)!.AsObject();
        var spec = DeckImporter.ApplyUnderstood(json, doc)!;
        var tasks = spec.Slides.Single(s => s.Layout == "tasks");
        Assert.Single(tasks.Tasks!); Assert.Equal("tasks", tasks.Editable!.Kind);
        Assert.StartsWith("data:image/png", spec.Slides[2].Image!.Src);
        Assert.Null(spec.Slides[3].Image);
        Assert.Equal("two-column", spec.Slides[4].Layout);
        Assert.Null(DeckImporter.ApplyUnderstood(System.Text.Json.Nodes.JsonNode.Parse("{\"reply\":\"x\"}")!.AsObject(), doc));
    }

    [Fact]
    public void UserPage_IfPresent_YieldsCleanOutline()
    {
        var path = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/imp/user.html";
        if (!File.Exists(path)) return;
        var doc = DeckImporter.Extract("user.html", File.ReadAllBytes(path));
        var all = doc.Pages.SelectMany(p => p.Paragraphs).ToList();
        Assert.DoesNotContain(all, x => x.Contains("حفظ التعديلات") || x.Contains("السابق") || x.Contains("اضغط على"));
        Assert.Contains(doc.DataBlocks, b => b.Contains("مصفوفة تتبع المتطلبات"));
        Assert.Contains(doc.DataBlocks, b => b.StartsWith("JSON:") && b.Contains("Public IP"));
        var spec = DeckImporter.ToSpec(doc);
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "user-outline.txt"), DeckImporter.BuildUnderstandInput(doc));
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "user-spec.json"), System.Text.Json.JsonSerializer.Serialize(spec, DeckSpec.Json));
    }
}
