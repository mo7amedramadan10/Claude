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
