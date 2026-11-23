using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>نماذج تصميم حر المسلَّمة مجلداتٍ: القراءة من wwwroot الحقيقي، ورفض المجلد المخالف بسببه.</summary>
public class FreeformFolderTemplateTests
{
    private static string WebRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "ChatToDashboard.Api", "wwwroot"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "ChatToDashboard.Api", "wwwroot");
    }

    [Fact]
    public void RealFolder_AllTemplatesLoad_Bilingual_WithPages()
    {
        var root = WebRoot();
        var r = FreeformFolderTemplates.Load(root);
        Assert.Empty(r.Rejected);
        Assert.True(r.Templates.Count >= 56, $"عدد النماذج {r.Templates.Count}");
        Assert.Equal(r.Templates.Count, r.Templates.Select(t => t.Key).Distinct().Count());
        foreach (var t in r.Templates)
        {
            Assert.Equal("slides", t.Kind);
            Assert.True(t.HasEn, $"{t.Key} بلا ترجمة كاملة");
            Assert.NotNull(t.PageEn);
            Assert.True(File.Exists(Path.Combine(root, t.Page!)), t.Page);
            Assert.True(File.Exists(Path.Combine(root, t.PageEn!)), t.PageEn);
            Assert.False(string.IsNullOrWhiteSpace(t.Brief));
        }
        // التسجيل: الثابتة أولًا ثم المجلدات، والمفتاح المكرر مع ثابت يُتجاهل
        FreeformTemplates.RegisterFolderTemplates(r.Templates.Append(new FreeformTemplate { Key = "sl-corporate-blue", Name = "مكرر", Kind = "slides" }));
        try
        {
            Assert.Equal(FreeformTemplates.Builtin.Count + r.Templates.Count, FreeformTemplates.All.Count);
            Assert.Equal("الأزرق المؤسسي — عرض جاهز", FreeformTemplates.Find("sl-corporate-blue")!.Name);
            Assert.NotNull(FreeformTemplates.Find(r.Templates[0].Key)?.Page);
        }
        finally { FreeformTemplates.RegisterFolderTemplates(Array.Empty<FreeformTemplate>()); }
    }

    [Fact]
    public void BadFolders_AreRejectedWithReason_WithoutStoppingOthers()
    {
        var web = Path.Combine(Path.GetTempPath(), "fft-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(web, "templates", "freeform");
        void Make(string folder, string json, string? ar)
        {
            var d = Path.Combine(root, folder); Directory.CreateDirectory(d);
            File.WriteAllText(Path.Combine(d, "template.json"), json);
            if (ar is not null) File.WriteAllText(Path.Combine(d, "ar.html"), ar);
        }
        const string two = "<html lang=\"ar\" dir=\"rtl\"><section class=\"slide active\"></section><section class=\"slide\"></section></html>";
        const string secs = "[{\"title\":\"أ\",\"titleEn\":\"A\"},{\"title\":\"ب\",\"titleEn\":\"B\",\"needsData\":true}]";
        try
        {
            Make("sl-good", $"{{\"key\":\"sl-good\",\"kind\":\"slides\",\"category\":\"hr\",\"icon\":\"🧩\",\"name\":\"جيد\",\"nameEn\":\"Good\",\"sections\":{secs}}}", two);
            Make("sl-wrong-key", $"{{\"key\":\"sl-other\",\"kind\":\"slides\",\"category\":\"hr\",\"name\":\"س\",\"sections\":{secs}}}", two);
            Make("sl-bad-cat", $"{{\"key\":\"sl-bad-cat\",\"kind\":\"slides\",\"category\":\"xx\",\"name\":\"س\",\"sections\":{secs}}}", two);
            Make("sl-count", $"{{\"key\":\"sl-count\",\"kind\":\"slides\",\"category\":\"hr\",\"name\":\"س\",\"sections\":[{{\"title\":\"أ\"}}]}}", two);
            Make("sl-no-ar", $"{{\"key\":\"sl-no-ar\",\"kind\":\"slides\",\"category\":\"hr\",\"name\":\"س\",\"sections\":{secs}}}", null);
            Make("sl-bad-json", "{ not json", two);
            Directory.CreateDirectory(Path.Combine(root, "helper")); // بلا template.json: يُتجاهل بصمت

            var r = FreeformFolderTemplates.Load(web);
            var t = Assert.Single(r.Templates);
            Assert.Equal("sl-good", t.Key);
            Assert.Equal("Good", t.NameEn);
            Assert.Equal("templates/freeform/sl-good/ar.html", t.Page);
            Assert.Null(t.PageEn);
            Assert.Equal(FreeformFolderTemplates.DefaultBrief, t.Brief);
            Assert.Equal(1, t.DataSections);
            Assert.Equal(new[] { "sl-bad-cat", "sl-bad-json", "sl-count", "sl-no-ar", "sl-wrong-key" }, r.Rejected.Select(x => x.Folder).OrderBy(x => x));
            Assert.Contains(r.Rejected, x => x.Folder == "sl-count" && x.Reason.Contains("2 شرائح"));
            Assert.Empty(FreeformFolderTemplates.Load(Path.Combine(web, "missing")).Templates);
        }
        finally { Directory.Delete(web, true); }
    }
}
