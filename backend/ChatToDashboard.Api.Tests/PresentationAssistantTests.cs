using System.Text.Json.Nodes;
using ChatToDashboard.Api.Freeform;
using ChatToDashboard.Api.Presentations;
using ChatToDashboard.Api.Users;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>مساعد «جيم» في صفحة العرض: استخراج سياق الملف كله حتميًا (مواصفة عرض / شرائح / صفحة + بيانات بلا SQL)، وسياسة من يسأل، والسقف.</summary>
public class PresentationAssistantTests
{
    [Fact]
    public void ExtractContext_DeckSpec_ListsEverySlideWithKpisChartsTablesAndNotes()
    {
        var spec = """{"title":"نتائج الربع","slides":[{"layout":"title","title":"نتائج الربع الثالث"},{"layout":"kpis","title":"المؤشرات","kpis":[{"label":"الإيراد","value":"1,200","delta":"+5%"}]},{"layout":"chart","title":"الاتجاه","chart":{"type":"line","labels":["يناير","فبراير"],"values":[10,12],"unit":"ألف"}},{"layout":"table","title":"أكبر العملاء","table":{"columns":["العميل","القيمة"],"rows":[["أ","5"],["ب","3"]]},"notes":"ملاحظة سرية للمتحدث"}]}""";
        var html = $"<html><head></head><body><script type=\"application/json\" id=\"deck-spec\">{spec}</script><div id=\"deck\"></div></body></html>";
        var ctx = PresentationAssistantService.ExtractContext(html, "نتائج الربع");
        Assert.Contains("الشريحة 1: نتائج الربع الثالث", ctx);
        Assert.Contains("الشريحة 4: أكبر العملاء", ctx);
        Assert.Contains("مؤشر: الإيراد = 1,200 (التغيّر +5%)", ctx);
        Assert.Contains("رسم line (ألف): يناير=10، فبراير=12", ctx);
        Assert.Contains("جدول: العميل | القيمة", ctx);
        Assert.Contains("ملاحظات المتحدث: ملاحظة سرية للمتحدث", ctx);
    }

    [Fact]
    public void ExtractContext_SlidesPage_KeepsSvgForeignObjectText_PerSlide()
    {
        var html = """
            <html><head><style>.x{}</style></head><body>
            <section class="slide active"><svg viewBox="0 0 10 10"><foreignObject><div xmlns="http://www.w3.org/1999/xhtml">العنوان الأول</div><div>سطر داخل الشريحة</div></foreignObject></svg></section>
            <section class="slide"><svg><text>الشريحة الثانية</text><text>رقم 42</text></svg></section>
            <script>var slides=1;</script></body></html>
            """;
        var ctx = PresentationAssistantService.ExtractContext(html, "ع");
        Assert.Contains("صفحة شرائح (2 شريحة)", ctx);
        Assert.Contains("الشريحة 1: العنوان الأول", ctx);
        Assert.Contains("- سطر داخل الشريحة", ctx);
        Assert.Contains("الشريحة 2: الشريحة الثانية", ctx);
        Assert.Contains("- رقم 42", ctx);
        Assert.DoesNotContain("var slides", ctx);
    }

    [Fact]
    public void ExtractContext_JeemData_IncludesRowsButNeverSql()
    {
        var page = "<!doctype html><html><head></head><body><h1>لوحة المبيعات</h1><p>إجمالي المبيعات هذا العام</p><script>render()</script></body></html>";
        var html = FreeformDataBlock.Inject(page, new[] { new FreeformQuery("q1", "SELECT Region, SUM(Amount) AS Total FROM proj_x.sales GROUP BY Region", JsonNode.Parse("{\"rowCount\":2,\"rows\":[{\"Region\":\"الرياض\",\"Total\":500},{\"Region\":\"جدة\",\"Total\":300}]}")) }, DateTime.UtcNow);
        var ctx = PresentationAssistantService.ExtractContext(html, "لوحة");
        Assert.Contains("## q1 (2 صفًا)", ctx);
        Assert.Contains("Region: الرياض | Total: 500", ctx);
        Assert.Contains("لوحة المبيعات", ctx);
        Assert.DoesNotContain("SELECT", ctx);
        Assert.DoesNotContain("proj_x", ctx);
        Assert.DoesNotContain("JEEM_DATA", ctx);
    }

    [Fact]
    public void ExtractContext_TruncatesLongContent()
    {
        var html = "<html><body>" + string.Concat(Enumerable.Range(0, 4000).Select(i => $"<p>فقرة رقم {i} بنص طويل بعض الشيء ليملأ السياق</p>")) + "</body></html>";
        var ctx = PresentationAssistantService.ExtractContext(html, "ط");
        Assert.True(ctx.Length <= PresentationAssistantService.MaxContextChars + 50);
        Assert.EndsWith("…(اقتُطع باقي المحتوى)", ctx);
    }

    [Fact]
    public void TextLines_DedupesAndDropsScripts()
    {
        var lines = PresentationAssistantService.TextLines("<div>أول</div><div>أول</div><script>x()</script><p>ثانٍ <b>مع غامق</b></p><br>ثالث");
        Assert.Equal(new[] { "أول", "ثانٍ مع غامق", "ثالث" }, lines);
    }

    [Fact]
    public void CanAsk_FollowsPolicyAndSwitch()
    {
        var p = new Presentation { OrganizationId = "org1", Assistant = true, AssistantPolicy = "anyone" };
        var member = new AppUser { Id = "u1", OrganizationId = "org1" };
        var outsider = new AppUser { Id = "u2", OrganizationId = "org2" };
        Assert.True(PresentationAssistantService.CanAsk(null, p));
        p.AssistantPolicy = "members";
        Assert.False(PresentationAssistantService.CanAsk(null, p));
        Assert.False(PresentationAssistantService.CanAsk(outsider, p));
        Assert.True(PresentationAssistantService.CanAsk(member, p));
        p.Assistant = false;
        Assert.False(PresentationAssistantService.CanAsk(member, p));
    }

    [Fact]
    public void RateLimiter_SlidingWindow()
    {
        var rl = new PresentationAssistantService.RateLimiter(3, TimeSpan.FromMinutes(10));
        var t0 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(rl.TryConsume("a", t0)); Assert.True(rl.TryConsume("a", t0)); Assert.True(rl.TryConsume("a", t0));
        Assert.False(rl.TryConsume("a", t0.AddMinutes(5)));
        Assert.True(rl.TryConsume("b", t0.AddMinutes(5)));
        Assert.True(rl.TryConsume("a", t0.AddMinutes(11)));
    }
}
