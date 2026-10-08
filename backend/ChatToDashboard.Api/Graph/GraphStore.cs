using System.Text.Json;
using ChatToDashboard.Api.Data;
using Dapper;

namespace ChatToDashboard.Api.Graph;

/// <summary>
/// Persists network-graph data (nodes/edges) for the "لوحات تفاعلية" network dashboards — same
/// shared-DB/Dapper pattern as AnalystStore. Every row carries ProjectId, so each project gets
/// its own network per NetworkKey.
///
/// Phase 2 of the interactive-dashboards handoff ports network-graph.js/graph-drill.js as pure,
/// domain-agnostic rendering engines (see wwwroot/js) that read nodes/edges from
/// GET /api/graph/{networkKey} — never a hardcoded window.JEEM_NETWORK literal in the frontend,
/// per the handoff's golden rule. This app has no feature yet that lets a project author its own
/// regulatory/systems network from real uploaded sources (that would be a separate, much larger
/// AI-authoring feature — explicitly out of scope per the handoff's own "known gaps" section), so
/// SeedIfEmptyAsync seeds the reference's own demo scenario as real rows in this table on first
/// read per project. That keeps the letter of the golden rule (the frontend never embeds demo
/// data; every number it renders came from a real API call) while being honest that the seeded
/// *content* is still a placeholder scenario, same as every other demo seed in this app (e.g.
/// the startup-seeded demo user accounts).
/// </summary>
public class GraphStore
{
    private readonly DataStore _db;

    public GraphStore(DataStore db) => _db = db;

    private string NodesTable => _db.Provider == DbProvider.Sqlite ? "\"GraphNodes\"" : "[staging].[GraphNodes]";
    private string EdgesTable => _db.Provider == DbProvider.Sqlite ? "\"GraphEdges\"" : "[staging].[GraphEdges]";

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        await _db.CreateContainerIfMissingAsync(connection, ct);

        var statements = _db.Provider == DbProvider.Sqlite
            ? new[]
            {
                $"""
                 CREATE TABLE IF NOT EXISTS {NodesTable} (
                   "Id" TEXT PRIMARY KEY, "ProjectId" TEXT, "NetworkKey" TEXT, "NodeKey" TEXT,
                   "Type" TEXT, "Name" TEXT, "Description" TEXT, "Year" INTEGER, "DataJson" TEXT, "CreatedAt" TEXT)
                 """,
                $"""
                 CREATE TABLE IF NOT EXISTS {EdgesTable} (
                   "Id" TEXT PRIMARY KEY, "ProjectId" TEXT, "NetworkKey" TEXT, "FromKey" TEXT,
                   "ToKey" TEXT, "RelType" TEXT, "Year" INTEGER, "CreatedAt" TEXT)
                 """,
            }
            : new[]
            {
                $"""
                 IF OBJECT_ID('staging.GraphNodes') IS NULL
                 CREATE TABLE {NodesTable} (
                   [Id] NVARCHAR(64) PRIMARY KEY, [ProjectId] NVARCHAR(64), [NetworkKey] NVARCHAR(40), [NodeKey] NVARCHAR(40),
                   [Type] NVARCHAR(40), [Name] NVARCHAR(400), [Description] NVARCHAR(MAX), [Year] INT, [DataJson] NVARCHAR(MAX), [CreatedAt] DATETIME2)
                 """,
                $"""
                 IF OBJECT_ID('staging.GraphEdges') IS NULL
                 CREATE TABLE {EdgesTable} (
                   [Id] NVARCHAR(64) PRIMARY KEY, [ProjectId] NVARCHAR(64), [NetworkKey] NVARCHAR(40), [FromKey] NVARCHAR(40),
                   [ToKey] NVARCHAR(40), [RelType] NVARCHAR(40), [Year] INT, [CreatedAt] DATETIME2)
                 """,
            };

        foreach (var text in statements)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = text;
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task<(IReadOnlyList<GraphNode> Nodes, IReadOnlyList<GraphEdge> Edges)> GetNetworkAsync(
        string projectId, string networkKey, CancellationToken ct = default)
    {
        await EnsureSchemaAsync(ct);
        await SeedIfEmptyAsync(projectId, networkKey, ct);

        await using var connection = await _db.OpenConnectionAsync(ct);
        var nodes = await connection.QueryAsync<GraphNode>(
            $"SELECT * FROM {NodesTable} WHERE ProjectId = @projectId AND NetworkKey = @networkKey",
            new { projectId, networkKey });
        var edges = await connection.QueryAsync<GraphEdge>(
            $"SELECT * FROM {EdgesTable} WHERE ProjectId = @projectId AND NetworkKey = @networkKey",
            new { projectId, networkKey });
        return (nodes.ToList(), edges.ToList());
    }

    private async Task SeedIfEmptyAsync(string projectId, string networkKey, CancellationToken ct)
    {
        await using var connection = await _db.OpenConnectionAsync(ct);
        var existing = await connection.ExecuteScalarAsync<long>(
            $"SELECT COUNT(1) FROM {NodesTable} WHERE ProjectId = @projectId AND NetworkKey = @networkKey",
            new { projectId, networkKey });
        if (existing > 0) return;

        if (networkKey != GraphNetworkKeys.Policy) return; // No seed defined for this key yet.

        var now = DateTime.UtcNow;
        var nodes = PolicySeed.Nodes(projectId, now);
        var edges = PolicySeed.Edges(projectId, now);

        await using var transaction = await connection.BeginTransactionAsync(ct);
        foreach (var n in nodes)
            await connection.ExecuteAsync($"""
                INSERT INTO {NodesTable} (Id, ProjectId, NetworkKey, NodeKey, Type, Name, Description, Year, DataJson, CreatedAt)
                VALUES (@Id, @ProjectId, @NetworkKey, @NodeKey, @Type, @Name, @Description, @Year, @DataJson, @CreatedAt)
                """, n, transaction);
        foreach (var e in edges)
            await connection.ExecuteAsync($"""
                INSERT INTO {EdgesTable} (Id, ProjectId, NetworkKey, FromKey, ToKey, RelType, Year, CreatedAt)
                VALUES (@Id, @ProjectId, @NetworkKey, @FromKey, @ToKey, @RelType, @Year, @CreatedAt)
                """, e, transaction);
        await transaction.CommitAsync(ct);
    }

    /// <summary>"شبكة الأنظمة واللوائح" demo seed — ported verbatim (ids, names, metrics) from the
    /// reference's assets/js/policy-graph-data.js. Placeholder content per the handoff's golden
    /// rule, but stored as real rows, never a frontend literal.</summary>
    private static class PolicySeed
    {
        private static string Json(int c, int cm, int s) => JsonSerializer.Serialize(new { c, cm, s });

        public static List<GraphNode> Nodes(string projectId, DateTime now)
        {
            GraphNode N(string key, string type, string name, string desc, int year, int c, int cm, int s) => new()
            {
                Id = Guid.NewGuid().ToString("N"), ProjectId = projectId, NetworkKey = GraphNetworkKeys.Policy,
                NodeKey = key, Type = type, Name = name, Description = desc, Year = year, DataJson = Json(c, cm, s), CreatedAt = now,
            };
            return new List<GraphNode>
            {
                N("L1", "law", "نظام المنافسات والمشتريات الحكومية", "ينظم طرح المنافسات الحكومية وترسيتها والتعاقد عليها، ويضمن الشفافية وتكافؤ الفرص بين المتنافسين.", 2019, 1284, 82, 18),
                N("L2", "law", "نظام حماية البيانات الشخصية", "يحدد حقوق أصحاب البيانات والتزامات الجهات في جمع البيانات الشخصية ومعالجتها ونقلها.", 2021, 642, 71, 9),
                N("L3", "law", "نظام التخصيص", "ينظم عقود الشراكة بين القطاعين العام والخاص ونقل ملكية الأصول والخدمات الحكومية.", 2021, 214, 77, 22),

                N("C1", "chapter", "التأهيل المسبق", "متطلبات تأهيل الموردين قبل الطرح وتصنيفهم.", 2019, 188, 84, 6),
                N("C2", "chapter", "الطرح والإعلان", "إعلان المنافسات ومددها ونشر الكراسات.", 2019, 96, 91, 24),
                N("C3", "chapter", "فحص العروض والترسية", "لجان الفحص ومعايير التقييم وقرار الترسية.", 2019, 412, 76, -14),
                N("C4", "chapter", "التعاقد والضمانات", "صيغ العقود والضمانات الابتدائية والنهائية.", 2019, 154, 86, 4),
                N("C5", "chapter", "الغرامات والجزاءات", "غرامات التأخير وسحب الأعمال والاستبعاد.", 2019, 236, 79, -21),
                N("C6", "chapter", "التظلم والاعتراض", "مسار تظلم المتنافسين من قرارات الجهة ولجنة النظر.", 2019, 198, 88, -6),
                N("C7", "chapter", "حقوق صاحب البيانات", "الحق في العلم والوصول والتصحيح والإتلاف.", 2021, 174, 68, 12),
                N("C8", "chapter", "الإفصاح عن الحوادث", "إبلاغ الجهة المختصة عند تسرب البيانات خلال المدة المحددة.", 2023, 96, 63, -18),
                N("C9", "chapter", "نقل البيانات خارج المملكة", "شروط نقل البيانات الشخصية والإفصاح عنها خارج المملكة.", 2023, 58, 70, -4),
                N("C10", "chapter", "عقود الشراكة", "هيكلة عقود الشراكة وتوزيع المخاطر.", 2021, 64, 80, 15),

                N("R1", "reg", "اللائحة التنفيذية لنظام المنافسات", "تفصّل إجراءات الطرح والفحص والترسية والتعاقد.", 2019, 342, 83, 10),
                N("R2", "reg", "لائحة تفضيل المحتوى المحلي والمنشآت الصغيرة", "تمنح الأفضلية السعرية للمنتج الوطني والمنشآت الصغيرة والمتوسطة.", 2020, 188, 74, 31),
                N("R3", "reg", "دليل إجراءات الشراء المباشر", "حالات الشراء المباشر وحدوده وموافقاته.", 2022, 72, 81, 2),
                N("R4", "reg", "ضوابط التعاقد والشراء", "ضوابط تفصيلية لإعداد الكراسات والكميات وتقدير التكلفة.", 2024, 46, 69, 7),
                N("R5", "reg", "تعديل مدد الاعتراض والتظلم", "يعدّل مدد تقديم التظلم والبت فيه.", 2025, 21, 74, 16),
                N("R6", "reg", "اللائحة التنفيذية لنظام حماية البيانات", "تفصّل الأسس النظامية للمعالجة وسجلات الأنشطة.", 2023, 118, 66, 5),
                N("R7", "reg", "لائحة نقل البيانات خارج المملكة", "تحدد معايير الملاءمة والضمانات المطلوبة للنقل.", 2023, 37, 64, -9),
                N("R8", "reg", "اللائحة التنفيذية لنظام التخصيص", "تفصّل مراحل مشاريع التخصيص واعتمادها.", 2021, 41, 79, 13),

                N("E1", "entity", "وزارة المالية", "الجهة المشرفة على تطبيق نظام المنافسات وإصدار لوائحه.", 2019, 96, 92, 21),
                N("E2", "entity", "هيئة كفاءة الإنفاق والمشروعات الحكومية", "تراجع المشروعات والعقود الكبرى وتصدر ضوابط الكفاءة.", 2019, 58, 90, 17),
                N("E3", "entity", "هيئة المحتوى المحلي والمشتريات الحكومية", "تطبّق سياسات المحتوى المحلي وتدير الاتفاقيات الإطارية.", 2019, 112, 87, 26),
                N("E4", "entity", "الديوان العام للمحاسبة", "يراقب سلامة الصرف والتعاقد في الجهات الحكومية.", 2019, 18, 95, 8),
                N("E5", "entity", "هيئة الرقابة ومكافحة الفساد", "تتلقى البلاغات المتعلقة بمخالفات التعاقد والمصالح المتعارضة.", 2019, 34, 94, 12),
                N("E6", "entity", "الهيئة السعودية للبيانات والذكاء الاصطناعي", "الجهة المختصة بالإشراف على نظام حماية البيانات الشخصية.", 2021, 74, 89, 19),
                N("E7", "entity", "المركز الوطني للتخصيص", "يدير برامج التخصيص ومشاريع الشراكة.", 2021, 22, 88, 24),
                N("E8", "entity", "وزارة الصحة", "جهة حكومية مُلزمة بالأنظمة الثلاثة.", 2019, 164, 78, -7),
                N("E9", "entity", "وزارة التعليم", "جهة حكومية مُلزمة بنظامي المنافسات وحماية البيانات.", 2019, 128, 81, -3),
                N("E10", "entity", "أمانة منطقة الرياض", "جهة بلدية ذات حجم تعاقد مرتفع.", 2019, 146, 74, -12),
                N("E11", "entity", "وزارة النقل والخدمات اللوجستية", "جهة ذات مشاريع بنية تحتية ومشاريع شراكة.", 2019, 92, 80, 3),
                N("E12", "entity", "المنشآت الصغيرة والمتوسطة", "شريحة الموردين المستفيدة من لائحة التفضيل.", 2020, 210, 72, 14),
                N("E13", "entity", "الموردون والمقاولون", "كل المتنافسين المسجلين في المنصات الحكومية.", 2019, 386, 70, -9),
                N("E14", "entity", "مقدمو الخدمات الصحية الخاصة", "جهات خاصة مُلزمة بنظام حماية البيانات.", 2023, 88, 61, -15),

                N("P1", "platform", "منصة اعتماد", "المنصة الموحدة لطرح المنافسات واستقبال العروض والمستحقات.", 2019, 228, 93, 11),
                N("P2", "platform", "منصة التظلمات الإلكترونية", "استقبال تظلمات المتنافسين ومتابعة البت فيها.", 2022, 64, 90, -2),
                N("P3", "platform", "منصة حوكمة البيانات الوطنية", "تسجيل الجهات ورفع سجلات المعالجة والإبلاغ عن الحوادث.", 2023, 41, 85, 6),
                N("P4", "platform", "منصة الشراكة مع القطاع الخاص", "عرض فرص التخصيص واستقبال إبداء الرغبة.", 2022, 12, 91, 28),

                N("K1", "complaint", "تظلمات قرارات الترسية", "اعتراض متنافسين على نتائج الترسية أو معايير التقييم.", 2019, 318, 0, -38),
                N("K2", "complaint", "تأخر صرف المستحقات", "شكاوى الموردين من تأخر صرف الدفعات بعد الإنجاز.", 2020, 274, 0, -46),
                N("K3", "complaint", "شروط تأهيل مقيِّدة للمنافسة", "شروط خبرة أو تصنيف تحدّ من دخول المنشآت الصغيرة.", 2020, 142, 0, -31),
                N("K4", "complaint", "غرامات تأخير متنازع عليها", "اعتراضات على احتساب غرامات التأخير وسحب الأعمال.", 2021, 126, 0, -27),
                N("K5", "complaint", "عدم تطبيق تفضيل المحتوى المحلي", "بلاغات عن عدم احتساب الأفضلية السعرية.", 2021, 88, 0, -22),
                N("K6", "complaint", "حوادث تسرب بيانات شخصية", "بلاغات عن وصول غير مصرح به لبيانات المستفيدين.", 2023, 64, 0, -52),
                N("K7", "complaint", "رسائل تسويقية دون موافقة", "شكاوى أصحاب البيانات من استخدام بياناتهم دون موافقة.", 2023, 152, 0, -35),
                N("K8", "complaint", "طول مدة البت في التظلم", "شكاوى من تجاوز المدد النظامية للبت.", 2022, 58, 0, -24),
                N("K9", "complaint", "غموض توزيع المخاطر في عقود الشراكة", "ملاحظات المستثمرين على بنود المخاطر.", 2022, 23, 0, -12),
            };
        }

        public static List<GraphEdge> Edges(string projectId, DateTime now)
        {
            GraphEdge E(string a, string b, string r, int? y = null) => new()
            {
                Id = Guid.NewGuid().ToString("N"), ProjectId = projectId, NetworkKey = GraphNetworkKeys.Policy,
                FromKey = a, ToKey = b, RelType = r, Year = y, CreatedAt = now,
            };
            return new List<GraphEdge>
            {
                E("L1", "C1", "has"), E("L1", "C2", "has"), E("L1", "C3", "has"), E("L1", "C4", "has"), E("L1", "C5", "has"), E("L1", "C6", "has"),
                E("L2", "C7", "has"), E("L2", "C8", "has"), E("L2", "C9", "has"), E("L3", "C10", "has"),
                E("R1", "L1", "exec"), E("R2", "L1", "exec"), E("R3", "L1", "exec"), E("R4", "R1", "amend", 2024), E("R5", "C6", "amend", 2025), E("R4", "C1", "amend", 2024),
                E("R6", "L2", "exec"), E("R7", "C9", "exec"), E("R8", "L3", "exec"),
                E("E1", "L1", "super"), E("E2", "R4", "super"), E("E3", "R2", "super"), E("E4", "L1", "super"), E("E5", "L1", "report"),
                E("E6", "L2", "super"), E("E7", "L3", "super"), E("E1", "L3", "super"),
                E("E8", "L1", "apply"), E("E8", "L2", "apply"), E("E8", "L3", "apply"), E("E9", "L1", "apply"), E("E9", "L2", "apply"),
                E("E10", "L1", "apply"), E("E11", "L1", "apply"), E("E11", "L3", "apply"), E("E14", "L2", "apply"),
                E("E12", "R2", "benefit"), E("E13", "L1", "apply"), E("E13", "P1", "apply"),
                E("L1", "P1", "via"), E("C6", "P2", "via"), E("L2", "P3", "via"), E("C8", "P3", "via"), E("L3", "P4", "via"),
                E("L1", "L3", "cross"), E("L1", "L2", "cross", 2023),
                E("K1", "C3", "under"), E("K1", "E10", "against"), E("K1", "E8", "against"), E("K1", "P2", "under", 2022),
                E("K2", "C4", "under"), E("K2", "P1", "against"), E("K2", "E8", "against"), E("K2", "E11", "against"),
                E("K3", "C1", "under"), E("K3", "E12", "against"), E("K3", "E9", "against"),
                E("K4", "C5", "under"), E("K4", "E10", "against"), E("K4", "E13", "against"),
                E("K5", "R2", "under"), E("K5", "E9", "against"),
                E("K6", "C8", "under"), E("K6", "E14", "against"), E("K6", "E8", "against"),
                E("K7", "C7", "under"), E("K7", "E14", "against"),
                E("K8", "C6", "under"), E("K8", "P2", "against"),
                E("K9", "C10", "under"), E("K9", "E7", "against"),
            };
        }
    }
}
