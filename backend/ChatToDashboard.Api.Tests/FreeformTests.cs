using System.Text.Json.Nodes;
using ChatToDashboard.Api.Controllers;
using ChatToDashboard.Api.Decks;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>تصميم حر: استخراج HTML من رد الموديل، العنوان من الطلب، وحقن شعار الهوية.</summary>
public class FreeformTests
{
    [Fact]
    public void ExtractHtml_FromJsonField_OrText_OrFragment()
    {
        var j = JsonNode.Parse("{\"reply\":\"x\",\"html\":\"<!doctype html><html><body>ok</body></html>\"}")!.AsObject();
        Assert.StartsWith("<!doctype html>", FreeformBuilder.ExtractHtml(j, ""));
        var frag = JsonNode.Parse("{\"html\":\"<section><h1>عنوان</h1></section>\"}")!.AsObject();
        var h = FreeformBuilder.ExtractHtml(frag, "")!; Assert.StartsWith("<!doctype html>", h); Assert.Contains("dir=\"rtl\"", h); Assert.Contains("<section>", h);
        var empty = JsonNode.Parse("{\"reply\":\"فقط نص\"}")!.AsObject();
        Assert.Null(FreeformBuilder.ExtractHtml(empty, "لا يوجد"));
        Assert.StartsWith("<html", FreeformBuilder.ExtractHtml(empty, "نص قبل <html><body>a</body></html> بعد"));
    }

    [Theory]
    [InlineData("اعمل صفحة تعريفية بمشروع مركز الاتصال", "تعريفية بمشروع مركز الاتصال")]
    [InlineData("صمّم لي لوحة متابعة المهام", "متابعة المهام")]
    [InlineData("   ", "تصميم جديد")]
    public void TitleFrom_Message(string msg, string expected) => Assert.Equal(expected, FreeformController.TitleFrom(msg));

    [Fact]
    public void ApplyBrandAssets_InjectsOrRemovesLogo()
    {
        var html = "<header><img data-jeem-logo alt=\"الشعار\"><img data-jeem-logo src=\"old.png\"></header>";
        var with = FreeformController.ApplyBrandAssets(html, new DeckBrand { Logo = "data:image/png;base64,AAAA" });
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(with, "data:image/png;base64,AAAA").Count);
        Assert.DoesNotContain("old.png", with);
        var without = FreeformController.ApplyBrandAssets(html, null);
        Assert.DoesNotContain("<img", without);
    }

    [Fact]
    public void SystemPrompt_IncludesBrandAndData()
    {
        var p = FreeformBuilder.BuildSystemPrompt(new DeckBrand { Name = "ثقة", Primary = "#0B3D5C", Font = "Tanseek", FontFaces = new() { new DeckBrandFont { Id = "ab", Family = "Tanseek", Format = "truetype" } } }, true, "جدول Sales (240 صفًا)");
        Assert.Contains("ثقة", p); Assert.Contains("#0B3D5C", p); Assert.Contains("@font-face", p); Assert.Contains("query_data", p); Assert.Contains("jeem-page.js", p); Assert.Contains("240 صفًا", p);
        Assert.DoesNotContain("# بيانات المشروع", FreeformBuilder.BuildSystemPrompt(null, false, ""));
    }
}

public class FreeformKindTests
{
    [Theory]
    [InlineData("doc", "A4")]
    [InlineData("slides", "16:9")]
    [InlineData("dashboard", "query_data")]
    [InlineData("motion", "prefers-reduced-motion")]
    [InlineData("design", "نموذج أولي")]
    public void KindInstructions_Present(string kind, string marker) => Assert.Contains(marker, FreeformBuilder.BuildSystemPrompt(null, true, "x", kind));

    [Fact]
    public void NormalizeKind_FallsBackToDesign()
    {
        Assert.Equal("design", FreeformStore.NormalizeKind(null)); Assert.Equal("design", FreeformStore.NormalizeKind("video")); Assert.Equal("motion", FreeformStore.NormalizeKind("motion"));
        Assert.Equal("مستند", FreeformBuilder.KindLabel("doc"));
    }
}
