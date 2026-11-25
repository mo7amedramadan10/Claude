using System.Text.Json;
using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>سجلات الإدخال (jeem.records): سياسة الإدخال (أعضاء المنظمة افتراضيًا أو عام)، رؤية السجلات، الملكية، التحقق، الدمج، وCSV.</summary>
public class FreeformRecordsTests
{
    private static FreeformDesign D(string policy = "members") => new() { Id = "d1", OrganizationId = "o1", CreatedByUserId = "owner", RecordsPolicy = policy };
    private static FreeformRecordAccess.Viewer Anon(string? anon = "0123456789abcdef0123456789abcdef") => new(null, null, anon, false, false);
    private static FreeformRecordAccess.Viewer Member(string id = "u1") => new(id, "o1", null, false, false);
    private static FreeformRecordAccess.Viewer Outsider() => new("x1", "o2", null, false, false);
    private static JsonElement J(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    [Fact]
    public void MembersPolicy_IsDefault_AndRequiresOrgMember()
    {
        Assert.Equal("members", new FreeformDesign().RecordsPolicy);
        Assert.Equal("members", FreeformRecordPolicy.Normalize(null));
        Assert.Equal(401, FreeformRecordAccess.CanAdd(Anon(), D())!.Value.Status);
        Assert.Equal(403, FreeformRecordAccess.CanAdd(Outsider(), D())!.Value.Status);
        Assert.Null(FreeformRecordAccess.CanAdd(Member(), D()));
        // داخل المنصة يُدخل من يرى التصميم مهما كانت السياسة
        Assert.Null(FreeformRecordAccess.CanAdd(new("x1", "o2", null, false, true), D()));
    }

    [Fact]
    public void PublicPolicy_AnyoneAdds_ButOnlyMembersSeeAll()
    {
        Assert.Null(FreeformRecordAccess.CanAdd(Anon(), D("public")));
        Assert.Null(FreeformRecordAccess.CanAdd(Outsider(), D("public")));
        Assert.False(FreeformRecordAccess.SeesAll(Anon(), D("public")));
        Assert.False(FreeformRecordAccess.SeesAll(Outsider(), D("public")));
        Assert.True(FreeformRecordAccess.SeesAll(Member(), D("public")));
        Assert.True(FreeformRecordAccess.SeesAll(new("m", "o2", null, true, false), D()));
    }

    [Fact]
    public void Ownership_ByUserOrAnonymousToken_ManagerChangesAny()
    {
        var mine = new FreeformRecord { CreatedByUserId = "U1" };
        var anonRec = new FreeformRecord { AnonId = "0123456789abcdef0123456789abcdef" };
        Assert.True(FreeformRecordAccess.Owns(Member("u1"), mine));
        Assert.False(FreeformRecordAccess.Owns(Member("u2"), mine));
        Assert.True(FreeformRecordAccess.Owns(Anon(), anonRec));
        Assert.False(FreeformRecordAccess.Owns(Anon("ffffffffffffffffffffffffffffffff"), anonRec));
        Assert.False(FreeformRecordAccess.Owns(Anon(), mine));
        Assert.True(FreeformRecordAccess.CanChange(new("m", "o1", null, true, false), mine));
        Assert.Null(FreeformRecordAccess.NormalizeAnon("short"));
        Assert.Null(FreeformRecordAccess.NormalizeAnon("<script>alert(1)</script>xxxxxxx"));
        Assert.Equal("abcdef0123456789", FreeformRecordAccess.NormalizeAnon("ABCDEF0123456789"));
    }

    [Fact]
    public void Validation_CollectionNames_AndRecordShape()
    {
        Assert.True(FreeformRecordStore.IsValidCollection("الطلبات"));
        Assert.True(FreeformRecordStore.IsValidCollection("survey_2026"));
        Assert.False(FreeformRecordStore.IsValidCollection(""));
        Assert.False(FreeformRecordStore.IsValidCollection("a/b"));
        Assert.False(FreeformRecordStore.IsValidCollection(new string('x', 61)));
        Assert.Null(FreeformRecordStore.Validate(J("""{"الاسم":"سارة","العدد":3}""")));
        Assert.NotNull(FreeformRecordStore.Validate(J("[1,2]")));
        Assert.NotNull(FreeformRecordStore.Validate(J("\"نص\"")));
        Assert.NotNull(FreeformRecordStore.Validate(J("{\"x\":\"" + new string('a', 40_000) + "\"}")));
        var many = "{" + string.Join(",", Enumerable.Range(0, 101).Select(i => $"\"f{i}\":1")) + "}";
        Assert.NotNull(FreeformRecordStore.Validate(J(many)));
    }

    [Fact]
    public void Merge_UpdatesFields_AndNullRemoves()
    {
        var merged = J(FreeformRecordStore.Merge("""{"a":1,"b":"x","c":true}""", J("""{"b":"y","c":null,"d":[1]}""")));
        Assert.Equal(1, merged.GetProperty("a").GetInt32());
        Assert.Equal("y", merged.GetProperty("b").GetString());
        Assert.False(merged.TryGetProperty("c", out _));
        Assert.Equal(1, merged.GetProperty("d").GetArrayLength());
    }

    [Fact]
    public void Csv_UnionOfFields_QuotesAndEntryColumns()
    {
        var rows = new List<FreeformRecord>
        {
            new() { Id = 1, Json = """{"الاسم":"سارة","ملاحظة":"أ, ب"}""", CreatedByName = "سارة", CreatedAt = new DateTime(2026, 11, 25, 9, 0, 0) },
            new() { Id = 2, Json = """{"الاسم":"علي","العدد":3}""", CreatedByName = null, CreatedAt = new DateTime(2026, 11, 25, 10, 0, 0) },
        };
        var lines = FreeformRecordsService.ToCsv(rows).TrimEnd().Split(Environment.NewLine);
        Assert.Equal("id,الاسم,ملاحظة,العدد,أدخله,تاريخ الإدخال", lines[0]);
        Assert.Equal("1,سارة,\"أ, ب\",,سارة,2026-11-25 09:00:00", lines[1]);
        Assert.Equal("2,علي,,3,,2026-11-25 10:00:00", lines[2]);
    }
}
