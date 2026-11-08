using System.Text.Json;
using System.Text.Json.Nodes;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Presentations;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>البيانات الحيّة للعروض المنشورة (المرحلة 3): حقن النتائج في الصفحة المقدَّمة، واستبدال مواصفة العرض، وكائن الجسر.</summary>
public class PresentationLiveDataTests
{
    private const string Scratch = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/decks";

    [Fact]
    public void IsLive_RequiresFlagAndSource()
    {
        Assert.False(PresentationLiveDataService.IsLive(new Presentation { LiveData = true }));
        Assert.False(PresentationLiveDataService.IsLive(new Presentation { LiveData = false, SourceKind = "freeform", SourceId = "a" }));
        Assert.False(PresentationLiveDataService.IsLive(new Presentation { LiveData = true, SourceKind = "upload", SourceId = "a" }));
        Assert.True(PresentationLiveDataService.IsLive(new Presentation { LiveData = true, SourceKind = "deck", SourceId = "a" }));
    }

    [Fact]
    public void ApplyToHtml_Freeform_ReplacesDataBlockWithFreshRows()
    {
        var html = FreeformDataBlock.Inject("<!doctype html><html><head></head><body><script>var n = JEEM_DATA.q1.rows[0].n</script></body></html>",
            new[] { new FreeformQuery("q1", "SELECT 5 AS n", JsonNode.Parse("{\"rowCount\":1,\"rows\":[{\"n\":5}]}")) }, DateTime.UtcNow);
        var p = new Presentation { LiveData = true, SourceKind = "freeform", SourceId = "x" };
        var live = new PresentationLiveData(DateTime.UtcNow, new[] { new FreeformQuery("q1", "SELECT 5 AS n", JsonNode.Parse("{\"rowCount\":1,\"rows\":[{\"n\":99}]}")) }, null, 0, null);
        var served = PresentationLiveDataService.ApplyToHtml(html, p, live);
        var parsed = FreeformDataBlock.Parse(served);
        Assert.Single(parsed); Assert.Equal(99, parsed[0].Result!["rows"]![0]!["n"]!.GetValue<int>());
        Assert.Contains("JEEM_DATA.q1.rows[0].n", served);
        // بلا استعلامات: الصفحة كما هي
        Assert.Equal(html, PresentationLiveDataService.ApplyToHtml(html, p, new PresentationLiveData(DateTime.UtcNow, Array.Empty<FreeformQuery>(), null, 0, null)));
    }

    [Fact]
    public void ReplaceDeckSpec_KeepsBrandAndTemplate_FromPublishedPage()
    {
        var published = new DeckSpec { Title = "ع", Brand = new DeckBrand { Name = "شركة", Primary = "#123456" }, Slides = { new DeckSlide { Layout = "kpis", Title = "م", Kpis = new() { new DeckKpi { Label = "أ", Value = "1" } } } } }.Normalize();
        var html = DeckHtmlExporter.Build(published, "/*rt*/", includeBridge: true);
        var fresh = new DeckSpec { Title = "ع", Slides = { new DeckSlide { Layout = "kpis", Title = "م", Kpis = new() { new DeckKpi { Label = "أ", Value = "2" } } } } }.Normalize();
        var served = PresentationLiveDataService.ReplaceDeckSpec(html, fresh);
        var m = System.Text.RegularExpressions.Regex.Match(served, "<script type=\"application/json\" id=\"deck-spec\">([\\s\\S]*?)</script>");
        Assert.True(m.Success);
        var spec = JsonNode.Parse(m.Groups[1].Value)!.AsObject();
        Assert.Equal("2", spec["slides"]![0]!["kpis"]![0]!["value"]!.GetValue<string>());
        Assert.Equal("شركة", spec["brand"]!["name"]!.GetValue<string>());
        Assert.Contains("jeem:data", served);
        Assert.Contains("/js/jeem-page.js", served);
        Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(served, "id=\"deck-spec\"").Count);
        // صفحة بلا مواصفة: تبقى كما هي
        Assert.Equal("<p>x</p>", PresentationLiveDataService.ReplaceDeckSpec("<p>x</p>", fresh));
    }

    [Fact]
    public void ToJson_ShapesQueriesAndSpec()
    {
        var live = new PresentationLiveData(new DateTime(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc),
            new[] { new FreeformQuery("q1", "SELECT 1", JsonNode.Parse("{\"rowCount\":2,\"rows\":[{\"a\":1},{\"a\":2}]}")), new FreeformQuery("q2", "SELECT 2", null) },
            null, 1, null);
        var o = live.ToJson();
        Assert.Equal("2026-11-05T10:00:00.0000000Z", o["refreshedAt"]!.GetValue<string>());
        Assert.Equal(2, o["queries"]!["q1"]!["rowCount"]!.GetValue<int>());
        Assert.Equal(0, o["queries"]!["q2"]!["rowCount"]!.GetValue<int>());
        Assert.Equal(1, o["failed"]!.GetValue<int>());
        Assert.Null(o["spec"]);
        Assert.DoesNotContain("sql", o.ToJsonString()); // لا تُرسل جمل SQL للمتصفح
    }

    [Fact]
    public void EnsurePageBridge_AddsOnce()
    {
        var once = FreeformDataBlock.EnsurePageBridge("<html><body><p>x</p></body></html>");
        Assert.Contains("<script src=\"/js/jeem-page.js\"></script></body>", once);
        Assert.Equal(once, FreeformDataBlock.EnsurePageBridge(once));
        Assert.EndsWith("<script src=\"/js/jeem-page.js\"></script>", FreeformDataBlock.EnsurePageBridge("<p>x</p>"));
    }

    /// <summary>يكتب صفحة عرض منشورة حقيقية (بالمحرك الفعلي) في scratchpad لاختبار البيانات الحيّة في المتصفح عبر الخادم الوهمي.</summary>
    [Fact]
    public void DumpPublishedDeckForBrowserCheck()
    {
        if (!Directory.Exists(Scratch)) return;
        var runtime = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ChatToDashboard.Api", "wwwroot", "js", "jeem-deck.js");
        if (!File.Exists(runtime)) return;
        var spec = new DeckSpec
        {
            Title = "مراجعة الأعمال", Slides =
            {
                new DeckSlide { Layout = "title", Title = "مراجعة الأعمال", Subtitle = "الربع الثالث" },
                new DeckSlide { Layout = "kpis", Title = "المؤشرات", Query = new DeckQuery { Sql = "SELECT 1" }, Kpis = new() { new DeckKpi { Label = "الإيراد", Value = "100" }, new DeckKpi { Label = "الصفقات", Value = "10" } } },
                new DeckSlide { Layout = "bullets", Title = "ملاحظات", Bullets = new() { "أ", "ب" } },
            }
        }.Normalize();
        File.WriteAllText(Path.Combine(Scratch, "live-deck.html"), DeckHtmlExporter.Build(spec, File.ReadAllText(runtime), includeBridge: true));
        var fresh = JsonSerializer.Deserialize<DeckSpec>(JsonSerializer.Serialize(spec, DeckSpec.Json), DeckSpec.Json)!;
        fresh.Slides[1].Kpis![0].Value = "250"; fresh.Slides[1].Kpis![1].Value = "31";
        File.WriteAllText(Path.Combine(Scratch, "live-deck-fresh.json"), JsonSerializer.Serialize(fresh, DeckSpec.Json));
    }
}

/// <summary>رفع PowerPoint كما هو: صفحة شرائح واحدة بكل شريحة SVG مضمّنة وتنقّل وgoToSlide.</summary>
public class PptxPageBuilderTests
{
    private const string Scratch = "/tmp/claude-0/-home-user-Claude/da72bb5f-6107-5fbc-b2dd-f8387c2e28b5/scratchpad/decks";

    [Fact]
    public void Build_GeneratedDeck_OneSectionPerSlide_SelfContained()
    {
        var pptx = DeckPptxBuilder.Build(new DeckPptxRequest { Title = "x", Slides = { new DeckPptxSlideInput { Title = "عنوان", IsTitle = true }, new DeckPptxSlideInput { Title = "نقاط", Bullets = new() { "أ", "ب" } } } });
        var r = ChatToDashboard.Api.Freeform.PptxPageBuilder.Build(pptx, "تجربة");
        Assert.Equal(2, r.SlideCount);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(r.Html, "<section class=\"slide").Count);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(r.Html, "<svg xmlns=").Count);
        Assert.Contains("window.goToSlide=go", r.Html);
        Assert.Contains("<title>تجربة</title>", r.Html);
        Assert.Contains("عنوان", r.Html);
        Assert.DoesNotContain("/api/decks/brand/template", r.Html); // الصور مضمّنة لا روابط خادم
        if (Directory.Exists(Scratch)) File.WriteAllText(Path.Combine(Scratch, "pptx-page.html"), r.Html);
        var userFile = Path.Combine(Scratch, "..", "imp", "user.pptx");
        if (File.Exists(userFile)) { var u = ChatToDashboard.Api.Freeform.PptxPageBuilder.Build(File.ReadAllBytes(userFile), "ملف المستخدم"); Assert.True(u.SlideCount > 0); File.WriteAllText(Path.Combine(Scratch, "pptx-page-user.html"), u.Html); }
    }
}

/// <summary>ربط ملف برابط عام: تحويل روابط المشاركة إلى تنزيل مباشر، وكشف صيغة الملف المنزَّل.</summary>
public class LinkedFileFetcherTests
{
    [Fact]
    public void ToDownloadUrl_GoogleSheets_WholeFileXlsx_OrSheetCsvWithGid()
    {
        Assert.Equal("https://docs.google.com/spreadsheets/d/1AbC_x-9/export?format=xlsx", ChatToDashboard.Api.Repository.LinkedFileFetcher.ToDownloadUrl("https://docs.google.com/spreadsheets/d/1AbC_x-9/edit?usp=sharing"));
        // الملف كاملًا دائمًا (كل التبويبات للتنزيل)؛ الورقة تُختار بالاسم لا بـ gid
        Assert.Equal("https://docs.google.com/spreadsheets/d/1AbC_x-9/export?format=xlsx", ChatToDashboard.Api.Repository.LinkedFileFetcher.ToDownloadUrl("https://docs.google.com/spreadsheets/d/1AbC_x-9/edit#gid=123"));
        Assert.Equal("123", ChatToDashboard.Api.Repository.LinkedFileFetcher.GoogleGid("https://docs.google.com/spreadsheets/d/1AbC_x-9/edit#gid=123"));
        Assert.Equal("google", ChatToDashboard.Api.Repository.LinkedFileFetcher.ProviderOf("https://docs.google.com/spreadsheets/d/1AbC/edit"));
    }

    [Fact]
    public void ToDownloadUrl_OneDrive_SharePoint_Dropbox_Direct()
    {
        var od = ChatToDashboard.Api.Repository.LinkedFileFetcher.ToDownloadUrl("https://1drv.ms/x/s!AbCd-ef");
        Assert.StartsWith("https://api.onedrive.com/v1.0/shares/u!", od); Assert.EndsWith("/root/content", od); Assert.DoesNotContain("=", od[od.IndexOf("u!")..]);
        Assert.Equal("https://contoso.sharepoint.com/:x:/s/team/Ebook.xlsx?e=abc&download=1", ChatToDashboard.Api.Repository.LinkedFileFetcher.ToDownloadUrl("https://contoso.sharepoint.com/:x:/s/team/Ebook.xlsx?e=abc"));
        Assert.Equal("https://www.dropbox.com/s/abc/data.csv?dl=1", ChatToDashboard.Api.Repository.LinkedFileFetcher.ToDownloadUrl("https://www.dropbox.com/s/abc/data.csv?dl=0"));
        Assert.Equal("https://example.com/data.csv", ChatToDashboard.Api.Repository.LinkedFileFetcher.ToDownloadUrl("https://example.com/data.csv"));
        Assert.Equal("sharepoint", ChatToDashboard.Api.Repository.LinkedFileFetcher.ProviderOf("https://contoso.sharepoint.com/x"));
        Assert.Equal("onedrive", ChatToDashboard.Api.Repository.LinkedFileFetcher.ProviderOf("https://1drv.ms/x/s!a"));
        Assert.Equal("url", ChatToDashboard.Api.Repository.LinkedFileFetcher.ProviderOf("https://example.com/a.csv"));
        Assert.False(ChatToDashboard.Api.Repository.LinkedFileFetcher.IsValidUrl("ftp://x")); Assert.True(ChatToDashboard.Api.Repository.LinkedFileFetcher.IsValidUrl(" https://a.b/c "));
    }

    [Fact]
    public void DetectExtension_ByMagic_Header_ContentType_OrSniff()
    {
        var zip = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0, 0 };
        Assert.Equal(".xlsx", ChatToDashboard.Api.Repository.LinkedFileFetcher.DetectExtension(zip, "application/octet-stream", null, "https://x/y"));
        var csv = System.Text.Encoding.UTF8.GetBytes("a,b,c\n1,2,3\n");
        Assert.Equal(".csv", ChatToDashboard.Api.Repository.LinkedFileFetcher.DetectExtension(csv, "text/plain", null, "https://x/y"));
        Assert.Equal(".csv", ChatToDashboard.Api.Repository.LinkedFileFetcher.DetectExtension(csv, "application/octet-stream", "report.csv", "https://x/y"));
        var html = System.Text.Encoding.UTF8.GetBytes("<!doctype html><html><head><title>Sign in</title></head></html>");
        Assert.Null(ChatToDashboard.Api.Repository.LinkedFileFetcher.DetectExtension(html, "text/html", null, "https://docs.google.com/spreadsheets/d/x/export?format=csv"));
        Assert.Null(ChatToDashboard.Api.Repository.LinkedFileFetcher.DetectExtension(html, "text/html", null, "https://x/y.xlsx"));
    }
}

public class LinkedFileFetcherHttpTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _f; public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> f) { _f = f; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(_f(request));
    }
    private sealed class Factory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _h; public Factory(HttpMessageHandler h) { _h = h; }
        public HttpClient CreateClient(string name) => new(_h, disposeHandler: false);
    }

    [Fact]
    public async Task FetchAsync_Csv_FromGoogleExport_And_HtmlLoginPage_Error()
    {
        string? seen = null;
        var ok = new Factory(new StubHandler(r => { seen = r.RequestUri!.ToString(); return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("الاسم,القيمة\nأ,1\n")) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv") } } }; }));
        var f = await new ChatToDashboard.Api.Repository.LinkedFileFetcher(ok).FetchAsync("https://example.com/export/data", "مبيعات 2026", CancellationToken.None);
        Assert.Equal("https://example.com/export/data", seen);
        Assert.EndsWith(".csv", f.FileName); Assert.True(f.Bytes.Length > 0);

        var html = new Factory(new StubHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("<!doctype html><html><head><title>Sign in</title></head></html>", System.Text.Encoding.UTF8, "text/html") }));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => new ChatToDashboard.Api.Repository.LinkedFileFetcher(html).FetchAsync("https://example.com/private", "x", CancellationToken.None));
        Assert.Contains("أي شخص معه الرابط", ex.Message);

        var forbidden = new Factory(new StubHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)));
        var ex2 = await Assert.ThrowsAsync<InvalidOperationException>(() => new ChatToDashboard.Api.Repository.LinkedFileFetcher(forbidden).FetchAsync("https://example.com/p.csv", "x", CancellationToken.None));
        Assert.Contains("تسجيل دخول", ex2.Message);
    }
}


/// <summary>قراءة ورقة بعينها من ملف Excel متعدد الأوراق، وأسماء الأوراق، ورسالة واضحة لورقة غير موجودة.</summary>
public class MultiSheetXlsxTests
{
    [Fact]
    public void ReadXlsx_BySheetName_AndSheetNames()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xlsx");
        try
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var a = wb.Worksheets.Add("الملخص"); a.Cell(1, 1).Value = "البند"; a.Cell(1, 2).Value = "القيمة"; a.Cell(2, 1).Value = "أ"; a.Cell(2, 2).Value = 1;
                var b = wb.Worksheets.Add("التفاصيل"); b.Cell(1, 1).Value = "الاسم"; b.Cell(1, 2).Value = "المدينة"; b.Cell(1, 3).Value = "المبلغ"; b.Cell(2, 1).Value = "محمد"; b.Cell(2, 2).Value = "الرياض"; b.Cell(2, 3).Value = 250;
                wb.SaveAs(temp);
            }
            Assert.Equal(new[] { "الملخص", "التفاصيل" }, ChatToDashboard.Api.Data.TabularFileParser.SheetNames(temp));
            var first = ChatToDashboard.Api.Data.TabularFileParser.ReadXlsx(temp);
            Assert.Equal(2, first.Headers.Count);
            var second = ChatToDashboard.Api.Data.TabularFileParser.ReadXlsx(temp, " التفاصيل ");
            Assert.Equal(3, second.Headers.Count); Assert.Equal("محمد", second.Rows[0][0]);
            var ex = Assert.Throws<InvalidDataException>(() => ChatToDashboard.Api.Data.TabularFileParser.ReadXlsx(temp, "غير موجودة"));
            Assert.Contains("الملخص", ex.Message); Assert.Contains("التفاصيل", ex.Message);
        }
        finally { File.Delete(temp); }
    }
}
