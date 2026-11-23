using ChatToDashboard.Api.Users;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>صلاحيات الشاشات: الحفظ والقراءة، وخريطة الواجهات البرمجية للاختيار الجاهز «تصميم حر فقط».</summary>
public class ScreenAccessTests
{
    [Fact]
    public void SerializeAndParse_NullMeansAll()
    {
        Assert.Null(ScreenAccess.Serialize(null));
        Assert.Null(ScreenAccess.Serialize(ScreenAccess.Screens)); // كل الشاشات = بلا تقييد
        Assert.Equal("[\"freeform\",\"repo\"]", ScreenAccess.Serialize(new[] { "repo", "FREEFORM", "unknown", "freeform" }));
        Assert.Null(ScreenAccess.Parse(null)); Assert.Null(ScreenAccess.Parse("not json"));
        Assert.Equal(new[] { "freeform", "repo" }, ScreenAccess.Parse("[\"freeform\",\"repo\",\"x\"]"));
        Assert.Empty(ScreenAccess.Parse("[]")!);
    }

    [Fact]
    public void FreeformOnly_KeepsWhatFreeformNeeds_BlocksOtherScreens()
    {
        var a = ScreenAccess.FreeformOnly;
        foreach (var ok in new[] { "/api/freeform", "/api/freeform/d1/sharing", "/api/repository/files", "/api/semantics", "/api/sources", "/api/presentations/p1/data/k",
                                   "/api/chat", "/api/history/h1", "/api/share/by/o/s", "/api/templates", "/api/decks/brand", "/api/users/lookup", "/api/auth/me", "/api/projects" })
            Assert.True(ScreenAccess.IsApiAllowed(ok, a), ok);
        foreach (var no in new[] { "/api/analyst", "/api/analyst/x", "/api/decks", "/api/decks/d1/publish", "/api/published-dashboards", "/api/tokens" })
            Assert.False(ScreenAccess.IsApiAllowed(no, a), no);
        Assert.True(ScreenAccess.IsApiAllowed("/api/analyst", null));
        Assert.False(ScreenAccess.IsGated("/api/freeformx") && !ScreenAccess.IsApiAllowed("/api/freeformx", a)); // حدود المقطع
        Assert.False(ScreenAccess.IsGated("/api/auth/login"));
        Assert.True(ScreenAccess.IsGated("/api/decksx") == false);
    }
}
