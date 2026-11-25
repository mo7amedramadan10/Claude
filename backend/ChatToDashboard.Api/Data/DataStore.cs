using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace ChatToDashboard.Api.Data;

/// <summary>
/// كل ما يخص قاعدة البيانات (SQL Server حصرًا منذ 2026-11-02 — أُزيل دعم SQLite بقرار المستخدم):
/// الاتصالات، تسمية جداول الملفات وإنشاؤها، قراءة أعمدتها، ولهجة SQL التي يُطلب من الموديل كتابتها.
/// </summary>
public class DataStore
{
    private const string SqlServerSchema = "dbo";

    private readonly string _connectionString;

    public DataStore(IConfiguration configuration)
    {
        if (string.Equals(configuration["DatabaseProvider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "لم يعد تشغيل التطبيق على SQLite مدعومًا. اضبط \"DatabaseProvider\": \"SqlServer\" وسلسلة الاتصال " +
                "ConnectionStrings:DataDb (محليًا عبر user-secrets ببيئة Development).");

        var configured = configuration.GetConnectionString("DataDb");
        _connectionString = !string.IsNullOrWhiteSpace(configured)
            ? configured
            : throw new InvalidOperationException(
                "Connection string 'DataDb' is not configured. Set it with: " +
                "dotnet user-secrets set \"ConnectionStrings:DataDb\" \"<connection string>\" " +
                "(SQL Server is the only supported database).");
    }

    public DbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(ct);
        return connection;
    }

    /// <summary>
    /// Creates the target database if it doesn't exist yet (fresh SQL Server container, new
    /// local instance). Requires permission to connect to master; where that's denied (e.g. Azure SQL) the
    /// failure is logged and the database is assumed to be pre-provisioned.
    /// </summary>
    public async Task EnsureDatabaseExistsAsync(ILogger logger, CancellationToken ct = default)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var database = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(database)) return;

        try
        {
            builder.InitialCatalog = "master";
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"IF DB_ID(N'{database.Replace("'", "''")}') IS NULL CREATE DATABASE [{database.Replace("]", "]]")}]";
            await command.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex)
        {
            logger.LogWarning(
                "Could not verify/create database '{Database}' via master ({Message}); assuming it already exists.",
                database, ex.Message);
        }
    }

    /// <summary>ترحيل لمرة واحدة من المخطط القديم <c>staging</c> إلى <c>dbo</c> (قرار 2026-11-02): أي قاعدة نُشرت قبل
    /// ذلك (Azure مثلًا) ما زالت جداولها كلها تحت <c>staging</c>، وبدون هذا الترحيل كان الكود سينشئ جداول <c>dbo</c>
    /// فارغة ويترك الحسابات والبيانات معزولة فلا يدخل أحد. يعمل عند كل إقلاع قبل أن يلمس أي مخزن قاعدته: ينقل كل
    /// جدول في <c>staging</c> لا نظير له في <c>dbo</c> بـ <c>ALTER SCHEMA TRANSFER</c> (البيانات والفهارس تبقى كما هي)،
    /// ويعيد كتابة أسماء جداول الملفات المسجَّلة بالبادئة القديمة، ثم يحذف المخطط إن فرغ. جدول له نظير في
    /// <c>dbo</c> بالفعل يُترك مكانه مع تحذير في السجل (لا يُدمج ولا يُحذف شيء تلقائيًا).</summary>
    /// <summary>جداول التطبيق التي تحمل معرّف الكيان (كان اسمه «مشروع») — قائمة صريحة لأن مخطط dbo فيه جداول ليست من التطبيق
    /// (KPI/OKR…) قد يكون فيها عمود بالاسم القديم ولا تُمس. أي مخزن جديد بعمود EntityId يُضاف هنا فقط إن وُجد قبل هذا الترحيل.</summary>
    internal static readonly string[] EntityColumnTables =
    {
        "EntityRoles", "FreeformAssets", "FreeformTemplates", "FreeformDesigns", "FreeformMessages", "FreeformQueries", "FreeformShares",
        "FreeformVersions", "PublishedDashboards", "GraphEdges", "GraphNodes", "DashboardHistory", "DashboardRoles", "BrandIdentities",
        "DeckBrandTemplatePages", "DeckBrandTemplates", "DeckBrands", "DeckBrandFonts", "Decks", "repo_FilePermissions", "repo_FileRelationships",
        "repo_Files", "ColumnSemantics", "AnalystConversations", "AnalystMessages", "AnalystResultSources", "AnalystResults",
        "ExternalIntegrations", "IntegrationPublishLog", "PublishedDashboardSlots", "WidgetDataFilters",
    };

    /// <summary>SQL الترحيل من «المشروع» إلى «الكيان» (مُختبَر نصيًا): إعادة تسمية الجدولين ثم العمودين ProjectId/ProjectKey في جداول التطبيق.
    /// يعيد التشغيل بأمان: كل خطوة مشروطة بوجود القديم وغياب الجديد؛ وإن وُجد الاثنان (مخزن أضاف العمود الجديد فارغًا) تُنسخ القيم إلى الجديد.</summary>
    internal static string EntityRenameSql()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var (from, to) in new[] { ("Projects", "Entities"), ("ProjectRoles", "EntityRoles") })
            sb.Append($"IF OBJECT_ID('dbo.{from}') IS NOT NULL AND OBJECT_ID('dbo.{to}') IS NULL EXEC sp_rename 'dbo.{from}', '{to}';\n");
        foreach (var t in EntityColumnTables)
            foreach (var (oldCol, newCol) in new[] { ("ProjectId", "EntityId"), ("ProjectKey", "EntityKey") })
            {
                sb.Append($"IF OBJECT_ID('dbo.{t}') IS NOT NULL AND COL_LENGTH('dbo.{t}', '{oldCol}') IS NOT NULL\nBEGIN\n");
                sb.Append($"  IF COL_LENGTH('dbo.{t}', '{newCol}') IS NULL EXEC sp_rename 'dbo.{t}.{oldCol}', '{newCol}', 'COLUMN';\n");
                sb.Append($"  ELSE EXEC('UPDATE dbo.[{t}] SET [{newCol}] = [{oldCol}] WHERE [{newCol}] IS NULL');\n");
                sb.Append("END;\n");
            }
        return sb.ToString();
    }

    /// <summary>ترحيل «المشروع» إلى «الكيان» في القاعدة عند الإقلاع، قبل أي مخزن (وإلا أنشأ مخزن عمودًا جديدًا فارغًا بجوار القديم).
    /// مخططات بيانات الكيانات تُرحَّل في MigrateEntitySchemasAsync.</summary>
    public async Task MigrateProjectToEntityAsync(ILogger logger, CancellationToken ct = default)
    {
        await using var connection = await OpenConnectionAsync(ct);
        int pending;
        await using (var probe = connection.CreateCommand())
        {
            var checks = string.Join(" + ", EntityColumnTables.Select(t => $"CASE WHEN COL_LENGTH('dbo.{t}','ProjectId') IS NOT NULL OR COL_LENGTH('dbo.{t}','ProjectKey') IS NOT NULL THEN 1 ELSE 0 END"));
            probe.CommandText = $"SELECT (CASE WHEN OBJECT_ID('dbo.Projects') IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN OBJECT_ID('dbo.ProjectRoles') IS NOT NULL THEN 1 ELSE 0 END) + {checks}";
            pending = Convert.ToInt32(await probe.ExecuteScalarAsync(ct));
        }
        if (pending == 0) return;
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = (SqlTransaction)tx;
            cmd.CommandText = EntityRenameSql();
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct);
        logger.LogWarning("Migrated {Count} legacy Project names to Entity (tables Projects/ProjectRoles and ProjectId/ProjectKey columns).", pending);
    }

    /// <summary>جداول التطبيق التي قد تحمل أسماء جداول الكيانات نصًّا (SQL اللوحات والتصاميم والعروض، أسماء الملفات، المعاني…) — قائمة صريحة
    /// (لا تُمس جداول dbo التي ليست من التطبيق). مستثنى: usage_Log (سجل تاريخي ضخم لا يؤثر على عمل شيء) وجداول الحسابات والمفاتيح والإعدادات.</summary>
    internal static readonly string[] EntitySchemaTextTables =
    {
        "AnalystConversations", "AnalystMessages", "AnalystResultSources", "AnalystResults", "BrandIdentities", "ClaudeChatMessages", "ClaudeChats",
        "ColumnSemantics", "DashboardHistory", "DeckBrandTemplates", "Decks", "ExternalIntegrations", "FreeformAssets", "FreeformDesigns",
        "FreeformMessages", "FreeformQueries", "FreeformRecords", "FreeformTemplates", "FreeformVersions", "GraphEdges", "GraphNodes",
        "InquiryConversations", "IntegrationPublishLog", "PresentationData", "Presentations", "PromptTemplateOverrides", "PublishedDashboardSlots",
        "PublishedDashboards", "SchemaAliasEntries", "SharedDashboard", "WidgetDataFilters", "repo_FilePermissions", "repo_FileRelationships", "repo_Files",
    };

    /// <summary>مخطط النسخ الاحتياطية للصفوف التي يغيّرها ترحيل المخططات — خارج dbo فلا يراها الموديل ولا كتالوج البيانات.</summary>
    internal const string EntitySchemaBackup = "bak_proj2ent";

    /// <summary>معرّف كيان صالح من اسم مخطط قديم (<c>proj_&lt;hex&gt;</c>)، وإلا null.</summary>
    internal static string? LegacySchemaEntityId(string schema)
        => schema.StartsWith(LegacyEntitySchemaPrefix, StringComparison.Ordinal) && schema.Length > LegacyEntitySchemaPrefix.Length
           && IsValidEntityId(schema[LegacyEntitySchemaPrefix.Length..]) ? schema[LegacyEntitySchemaPrefix.Length..] : null;

    /// <summary>
    /// ترحيل مخططات بيانات الكيانات عند الإقلاع: <c>proj_&lt;id&gt;</c> ← <c>ent_&lt;id&gt;</c>. داخل معاملة واحدة:
    /// (1) نسخة احتياطية في <see cref="EntitySchemaBackup"/> لكل صف في جداول <see cref="EntitySchemaTextTables"/> فيه «proj_» (مرة واحدة لكل جدول)؛
    /// (2) استبدال «proj_&lt;id&gt;» بـ«ent_&lt;id&gt;» حرفيًا لكل معرّف كيان في كل الأعمدة النصية لتلك الجداول (SQL محفوظ، أسماء جداول، صفحات منشورة…)؛
    /// (3) إنشاء <c>ent_&lt;id&gt;</c> ونقل كل جدول إليه بـ ALTER SCHEMA TRANSFER (البيانات والفهارس كما هي) مع وصف المخطط، ثم حذف القديم إن فرغ.
    /// يعيد التشغيل بأمان (لا مخطط قديم ← لا شيء). فشله يوقف الإقلاع (انظر Program.cs).
    /// </summary>
    public async Task MigrateEntitySchemasAsync(ILogger logger, CancellationToken ct = default)
    {
        await using var connection = await OpenConnectionAsync(ct);
        var ids = new List<string>();
        await using (var list = connection.CreateCommand())
        {
            list.CommandText = "SELECT name FROM sys.schemas WHERE name LIKE 'proj[_]%'";
            await using var reader = await list.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) if (LegacySchemaEntityId(reader.GetString(0)) is { } id) ids.Add(id);
        }
        if (ids.Count == 0) return;

        await using var tx = (SqlTransaction)await connection.BeginTransactionAsync(ct);
        async Task Exec(string sql, params SqlParameter[] ps)
        {
            await using var c = connection.CreateCommand();
            c.Transaction = tx; c.CommandTimeout = 600; c.CommandText = sql; c.Parameters.AddRange(ps);
            await c.ExecuteNonQueryAsync(ct);
        }
        async Task<List<string>> Strings(string sql, params SqlParameter[] ps)
        {
            var r = new List<string>();
            await using var c = connection.CreateCommand();
            c.Transaction = tx; c.CommandTimeout = 600; c.CommandText = sql; c.Parameters.AddRange(ps);
            await using var rd = await c.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct)) r.Add(rd.GetString(0));
            return r;
        }

        // (1) و(2) النصوص المحفوظة
        await Exec($"IF SCHEMA_ID('{EntitySchemaBackup}') IS NULL EXEC('CREATE SCHEMA [{EntitySchemaBackup}]')");
        var rewritten = 0;
        foreach (var table in EntitySchemaTextTables)
        {
            var cols = await Strings("""
                SELECT c.name FROM sys.columns c JOIN sys.types ty ON ty.user_type_id = c.user_type_id
                WHERE c.object_id = OBJECT_ID(@t) AND ty.name IN ('nvarchar', 'varchar')
                """, new SqlParameter("@t", "dbo." + table));
            if (cols.Count == 0) continue;
            var any = string.Join(" OR ", cols.Select(c => $"[{c}] LIKE N'%proj[_]%'"));
            await Exec($"IF OBJECT_ID('{EntitySchemaBackup}.{table}') IS NULL AND EXISTS (SELECT 1 FROM [dbo].[{table}] WHERE {any}) SELECT * INTO [{EntitySchemaBackup}].[{table}] FROM [dbo].[{table}] WHERE {any}");
            foreach (var id in ids)
            {
                var set = string.Join(", ", cols.Select(c => $"[{c}] = REPLACE([{c}], @old, @new)"));
                var where = string.Join(" OR ", cols.Select(c => $"[{c}] LIKE @pat"));
                await using var c = connection.CreateCommand();
                c.Transaction = tx; c.CommandTimeout = 600;
                c.CommandText = $"UPDATE [dbo].[{table}] SET {set} WHERE {where}";
                c.Parameters.Add(new SqlParameter("@old", LegacyEntitySchemaPrefix + id));
                c.Parameters.Add(new SqlParameter("@new", EntitySchemaPrefix + id));
                c.Parameters.Add(new SqlParameter("@pat", "%" + "proj[_]" + id + "%"));
                rewritten += await c.ExecuteNonQueryAsync(ct);
            }
        }

        // (3) المخططات
        var moved = 0;
        foreach (var id in ids)
        {
            string from = LegacyEntitySchemaPrefix + id, to = EntitySchemaPrefix + id;
            await Exec($"IF SCHEMA_ID('{to}') IS NULL EXEC('CREATE SCHEMA [{to}]')");
            await Exec($"""
                IF EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'{from}', NULL, NULL, NULL, NULL))
                   AND NOT EXISTS (SELECT 1 FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'{to}', NULL, NULL, NULL, NULL))
                BEGIN
                  DECLARE @d sql_variant = (SELECT value FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'{from}', NULL, NULL, NULL, NULL));
                  EXEC sp_addextendedproperty @name = N'MS_Description', @value = @d, @level0type = N'SCHEMA', @level0name = N'{to}';
                END
                """);
            var tables = await Strings("SELECT o.name FROM sys.objects o WHERE o.schema_id = SCHEMA_ID(@s) AND o.type IN ('U', 'V')", new SqlParameter("@s", from));
            foreach (var t in tables)
            {
                var exists = await Strings("SELECT o.name FROM sys.objects o WHERE o.schema_id = SCHEMA_ID(@s) AND o.name = @n", new SqlParameter("@s", to), new SqlParameter("@n", t));
                if (exists.Count > 0) { logger.LogWarning("Entity schema migration: {To}.{Table} already exists; {From}.{Table} left in place.", to, t, from, t); continue; }
                await Exec($"ALTER SCHEMA [{to}] TRANSFER [{from}].[{t.Replace("]", "]]")}]");
                moved++;
            }
            await Exec($"IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE schema_id = SCHEMA_ID('{from}')) EXEC('DROP SCHEMA [{from}]')");
        }
        await tx.CommitAsync(ct);
        logger.LogWarning("Entity schema migration: {Schemas} proj_ schema(s) → ent_, {Tables} table(s) moved, {Rows} stored row(s) rewritten (originals backed up in [{Backup}]).",
            ids.Count, moved, rewritten, EntitySchemaBackup);
    }

    public async Task MigrateLegacyStagingSchemaAsync(ILogger logger, CancellationToken ct = default)
    {
        const string legacy = "staging";
        await using var connection = await OpenConnectionAsync(ct);

        await using (var probe = connection.CreateCommand())
        {
            probe.CommandText = "SELECT SCHEMA_ID(@s)";
            probe.Parameters.Add(new SqlParameter("@s", legacy));
            if (await probe.ExecuteScalarAsync(ct) is null or DBNull) return;
        }

        var tables = new List<string>();
        await using (var list = connection.CreateCommand())
        {
            list.CommandText = "SELECT t.name FROM sys.tables t WHERE t.schema_id = SCHEMA_ID(@s) ORDER BY t.name";
            list.Parameters.Add(new SqlParameter("@s", legacy));
            await using var reader = await list.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) tables.Add(reader.GetString(0));
        }

        var moved = 0;
        foreach (var table in tables)
        {
            await using var exists = connection.CreateCommand();
            exists.CommandText = "SELECT OBJECT_ID(@t)";
            exists.Parameters.Add(new SqlParameter("@t", $"{SqlServerSchema}.{table}"));
            if (await exists.ExecuteScalarAsync(ct) is not (null or DBNull))
            {
                logger.LogWarning(
                    "Legacy table {Legacy}.{Table} left in place: {Schema}.{Table} already exists — merge or drop it by hand.",
                    legacy, table, SqlServerSchema);
                continue;
            }

            await using var transfer = connection.CreateCommand();
            transfer.CommandText = $"ALTER SCHEMA [{SqlServerSchema}] TRANSFER [{legacy}].[{table.Replace("]", "]]")}]";
            await transfer.ExecuteNonQueryAsync(ct);
            moved++;
        }

        if (moved > 0)
        {
            // أسماء جداول الملفات المحفوظة في الفهرس بالبادئة القديمة (ملفات المخطط المشترك فقط؛ ملفات المشاريع
            // تحمل ent_<id>. ولا تتأثر).
            await using var rename = connection.CreateCommand();
            rename.CommandText =
                $"IF OBJECT_ID('{SqlServerSchema}.repo_Files') IS NOT NULL " +
                $"UPDATE [{SqlServerSchema}].[repo_Files] SET TableName = '{SqlServerSchema}.' + SUBSTRING(TableName, {legacy.Length + 2}, 4000) " +
                $"WHERE TableName LIKE '{legacy}.%'";
            await rename.ExecuteNonQueryAsync(ct);
            logger.LogWarning("Migrated {Count} table(s) from the legacy [{Legacy}] schema to [{Schema}].", moved, legacy, SqlServerSchema);
        }

        await using (var remaining = connection.CreateCommand())
        {
            remaining.CommandText = "SELECT COUNT(*) FROM sys.objects WHERE schema_id = SCHEMA_ID(@s)";
            remaining.Parameters.Add(new SqlParameter("@s", legacy));
            if (Convert.ToInt32(await remaining.ExecuteScalarAsync(ct)) == 0)
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA [{legacy}]";
                await drop.ExecuteNonQueryAsync(ct);
            }
        }
    }

    // ---- Naming ----

    /// <summary>The schema a project's own uploaded-file tables live in. Falls back to the shared
    /// "dbo" schema when <paramref name="entityId"/> is null/blank or doesn't look like one
    /// of this app's own ids (defensive: an unexpected value never flows into a dynamically
    /// built schema name). System-sourced and data-folder tables (see SystemApiLoader)
    /// have no project of their own and always stay in "dbo" — only
    /// RepositoryStore's uploaded files get their own project schema.</summary>
    internal string EntitySchema(string? entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId) || !IsValidEntityId(entityId)) return SqlServerSchema;
        return EntitySchemaPrefix + entityId;
    }

    /// <summary>بادئة مخطط بيانات الكيان (كانت <c>proj_</c> حتى 2026-11-25 — انظر <see cref="MigrateEntitySchemasAsync"/>).</summary>
    internal const string EntitySchemaPrefix = "ent_";
    internal const string LegacyEntitySchemaPrefix = "proj_";

    internal static bool IsValidEntityId(string entityId) =>
        entityId.Length is > 0 and <= 64 && entityId.All(char.IsAsciiHexDigit);

    /// <summary>Quoted table reference for generated DDL/DML.</summary>
    internal string QualifiedTable(string schema, string tableName) =>
        $"[{schema}].[{tableName.Replace("]", "]]")}]";

    /// <summary>Same as the two-argument overload, but for a display name already carrying its
    /// own schema (e.g. "ent_xxx.Sales" or "dbo.Sales") — as returned by
    /// <see cref="GetSchemaAsync"/>, never built from client input. Generic over whichever
    /// schema the name actually carries (shared or any project's), so callers never need to
    /// know or re-derive which one a given table lives in.</summary>
    internal string QualifiedTable(string displayName)
    {
        var parts = displayName.Split('.');
        return string.Join(".", parts.Select(p => $"[{p.Replace("]", "]]")}]"));
    }

    /// <summary>The name Claude sees and writes in its queries, for a freshly loaded file —
    /// under <paramref name="entityId"/>'s own schema.</summary>
    public string DisplayTable(string? entityId, string tableName) =>
        $"{EntitySchema(entityId)}.{tableName}";

    /// <summary>Strips the schema from a display name, giving the bare table name. Works
    /// the same regardless of which SQL Server schema (shared or per-project) the name carries —
    /// it only looks for the single '.' separator, not a specific schema name.</summary>
    public string BareTableName(string displayName) =>
        displayName.Contains('.') ? displayName.Split('.').Last() : displayName;

    public async Task DropTableAsync(DbConnection connection, string? entityId, string bareName, CancellationToken ct = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {QualifiedTable(EntitySchema(entityId), bareName)}";
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>The name Claude sees, for a table name as returned by the catalog query —
    /// <paramref name="schema"/> is whichever schema that particular row actually came from
    /// (shared or a specific project's), since <see cref="GetSchemaAsync"/> can return rows from
    /// both in one call.</summary>
    private static string CatalogDisplayTable(string schema, string rawName) => $"{schema}.{rawName}";

    private static string Quote(string identifier) => $"[{identifier.Replace("]", "]]")}]";

    private static string SqlTypeFor(Type type)
    {
        return type == typeof(long) ? "BIGINT"
            : type == typeof(decimal) ? "DECIMAL(18,4)"
            : type == typeof(DateTime) ? "DATETIME2"
            : type == typeof(bool) ? "BIT"
            : "NVARCHAR(MAX)";
    }

    // ---- Schema ----

    /// <summary>Every column of every table visible to <paramref name="entityId"/>: the shared
    /// schema (system sources, data-folder reference tables) on its own when
    /// <paramref name="entityId"/> is null, or unioned with that project's own uploaded-file
    /// schema when it isn't. On SQL Server this scopes the scan at the SQL level — a project
    /// never sees, and the scan never even reads, another project's tables — rather than relying
    /// solely on the application-level <c>IsTableKnownToEntity</c> filter every caller still
    /// applies on top (kept as a second, now-redundant layer of defense).</summary>
    public async Task<IReadOnlyList<TableSchema>> GetSchemaAsync(string? entityId = null, CancellationToken ct = default)
    {
        var entitySchema = EntitySchema(entityId);
        var scopedToEntity = !string.Equals(entitySchema, SqlServerSchema, StringComparison.Ordinal);

        var sql = scopedToEntity
            ? """
              SELECT c.TABLE_SCHEMA AS TableSchema, c.TABLE_NAME AS TableName, c.COLUMN_NAME AS ColumnName, c.DATA_TYPE AS DataType
              FROM INFORMATION_SCHEMA.COLUMNS c
              WHERE c.TABLE_SCHEMA IN (@shared, @project)
              ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION
              """
            : """
              SELECT c.TABLE_SCHEMA AS TableSchema, c.TABLE_NAME AS TableName, c.COLUMN_NAME AS ColumnName, c.DATA_TYPE AS DataType
              FROM INFORMATION_SCHEMA.COLUMNS c
              WHERE c.TABLE_SCHEMA = @shared
              ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION
              """;

        await using var connection = await OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        // Same slow/shared-SQL-Server reasoning as LoadCommandTimeoutSeconds's own comment:
        // this runs ahead of every new upload's save (see RepositoryStore.ResolveBareTable-
        // NameAsync's collision check) and was seen live hitting SqlCommand's 30s default on
        // its own, as "Execution Timeout Expired" on save well before the actual table load
        // — scanning INFORMATION_SCHEMA.COLUMNS is cheap on a fast link but not free on a
        // loaded one, even now that it's scoped to (at most) two schemas instead of every table
        // on the server.
        command.CommandTimeout = LoadCommandTimeoutSeconds;
        command.Parameters.Add(new SqlParameter("@shared", SqlServerSchema));
        if (scopedToEntity) command.Parameters.Add(new SqlParameter("@project", entitySchema));

        var tables = new Dictionary<string, (string Schema, string Table, List<TableColumn> Columns)>();
        var order = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            var key = schema + "." + table;
            if (!tables.TryGetValue(key, out var entry))
            {
                entry = (schema, table, new List<TableColumn>());
                tables[key] = entry;
                order.Add(key);
            }
            entry.Columns.Add(new TableColumn(reader.GetString(2), reader.GetString(3)));
        }

        return order.Select(k =>
        {
            var entry = tables[k];
            return new TableSchema(CatalogDisplayTable(entry.Schema, entry.Table), entry.Columns);
        }).ToList();
    }

    public async Task CreateContainerIfMissingAsync(DbConnection connection, CancellationToken ct = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"IF SCHEMA_ID('{SqlServerSchema}') IS NULL EXEC('CREATE SCHEMA [{SqlServerSchema}]')";
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Same as <see cref="CreateContainerIfMissingAsync"/> but for one project's own
    /// schema (a no-op when <paramref name="entityId"/> resolves to the shared schema, which <see cref="CreateContainerIfMissingAsync"/> already
    /// covers). Called lazily, right before a project's first upload needs its schema to exist —
    /// the same lazy, idempotent, first-use pattern every schema in this app is already created
    /// by; no eager hook at project-creation time.</summary>
    /// <param name="entityName">When given, stamped onto the schema as its MS_Description
    /// extended property (what SSMS/Object Explorer shows as the schema's description/tooltip),
    /// purely so a human browsing the database can tell which project <c>ent_&lt;id&gt;</c>
    /// belongs to without a lookup. Never affects the schema's own name — every query, DDL
    /// statement and the LLM's own prompt keep referencing the short, stable <c>ent_&lt;id&gt;</c>
    /// form; embedding the (often Arabic, always mutable) display name into the identifier
    /// itself would make every generated query longer and would orphan the name the moment the
    /// project is renamed. Kept up to date on every call (not just the first) via
    /// sp_updateextendedproperty, so a later rename is reflected without a special migration
    /// path of its own.</param>
    public async Task CreateEntitySchemaIfMissingAsync(
        DbConnection connection, string? entityId, string? entityName = null, CancellationToken ct = default)
    {
        var schema = EntitySchema(entityId);
        if (string.Equals(schema, SqlServerSchema, StringComparison.Ordinal)) return;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"IF SCHEMA_ID('{schema}') IS NULL EXEC('CREATE SCHEMA [{schema}]')";
            await command.ExecuteNonQueryAsync(ct);
        }

        if (string.IsNullOrWhiteSpace(entityName)) return;

        await using var prop = connection.CreateCommand();
        prop.CommandText = """
            IF EXISTS (SELECT 1 FROM sys.extended_properties
                       WHERE class = 3 AND major_id = SCHEMA_ID(@schema) AND name = N'MS_Description')
                EXEC sp_updateextendedproperty @name = N'MS_Description', @value = @entityName,
                    @level0type = N'SCHEMA', @level0name = @schema;
            ELSE
                EXEC sp_addextendedproperty @name = N'MS_Description', @value = @entityName,
                    @level0type = N'SCHEMA', @level0name = @schema;
            """;
        prop.Parameters.Add(new SqlParameter("@schema", schema));
        prop.Parameters.Add(new SqlParameter("@entityName", entityName));
        await prop.ExecuteNonQueryAsync(ct);
    }

    // ---- Loading ----

    /// <summary>The default SqlCommand/SqlBulkCopy timeout (30s) is plenty on a fast local
    /// connection, but real deployments hit it over a slower or more loaded shared SQL Server
    /// link — seen live as "Execution Timeout Expired" while loading a ~1000-row upload. A
    /// larger ceiling here costs nothing on a fast connection and gives a slow one the room a
    /// bigger table load can genuinely need.</summary>
    private const int LoadCommandTimeoutSeconds = 120;

    /// <summary>Drops and recreates the table under <paramref name="entityId"/>'s own schema
    /// (or the shared schema when null — used by SystemApiLoader, which has
    /// no project of their own), then loads every row of <paramref name="table"/>.</summary>
    public async Task RecreateAndLoadAsync(
        DbConnection connection, string? entityId, string tableName, DataTable table, CancellationToken ct = default)
    {
        var target = QualifiedTable(EntitySchema(entityId), tableName);
        var columnDefs = table.Columns.Cast<DataColumn>()
            .Select(c => $"{Quote(c.ColumnName)} {SqlTypeFor(c.DataType)} NULL");

        await using (var drop = connection.CreateCommand())
        {
            drop.CommandText = $"DROP TABLE IF EXISTS {target}";
            drop.CommandTimeout = LoadCommandTimeoutSeconds;
            await drop.ExecuteNonQueryAsync(ct);
        }
        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE TABLE {target} ({string.Join(", ", columnDefs)})";
            create.CommandTimeout = LoadCommandTimeoutSeconds;
            await create.ExecuteNonQueryAsync(ct);
        }

        if (table.Rows.Count == 0) return;

        await BulkCopyAsync((SqlConnection)connection, target, table, ct);
    }

    private static async Task BulkCopyAsync(
        SqlConnection connection, string target, DataTable table, CancellationToken ct)
    {
        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = target,
            BatchSize = 5000,
            BulkCopyTimeout = LoadCommandTimeoutSeconds,
        };
        foreach (DataColumn column in table.Columns)
            bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        await bulk.WriteToServerAsync(table, ct);
    }

    // ---- Dialect (fed into Claude's system prompt and tool descriptions) ----

    public string DialectName => "Microsoft SQL Server (T-SQL)";

    /// <summary><paramref name="entityId"/> picks which schema the hint names — a project's
    /// own uploaded files live under its own schema, not the shared one.</summary>
    public string TableNamingHint(string? entityId) =>
        $"{EntitySchema(entityId)}.<TableName> for your own uploaded files, or dbo.<TableName> for shared/system tables";

    /// <summary>Same <paramref name="entityId"/> scoping as <see cref="TableNamingHint"/>.</summary>
    public string DialectPrompt(string? entityId) =>
        $"""
          SQL DIALECT — this is Microsoft SQL Server (T-SQL), NOT PostgreSQL or MySQL:
          - Use TOP N, never LIMIT. Always cap results: SELECT TOP 500 ...
          - Use GETDATE() instead of NOW(); DATEADD/DATEDIFF for date math; FORMAT() or CONVERT() for formatting.
          - Quote identifiers with [square brackets], not double quotes.
          - Reference a table exactly as it was listed to you (either {EntitySchema(entityId)}.[TableName]
            or dbo.[TableName]) — never assume one schema for every table.
          - Only SELECT statements are allowed. Any INSERT/UPDATE/DELETE/DDL is rejected.
          - Every uploaded-file column is stored as nvarchar regardless of what it actually
            holds, even a column that is numeric in almost every row. A plain CAST/CONVERT to a
            numeric type fails the ENTIRE query the instant even one row holds a blank/non-
            numeric value — "Error converting data type nvarchar to numeric" — even though every
            other row would have cast fine. If you hit that exact error, retry immediately with
            TRY_CAST(... AS DECIMAL) / TRY_CONVERT(DECIMAL, ...) instead of CAST/CONVERT: it
            returns NULL for the one bad row instead of failing the whole statement, and
            SUM/AVG/COUNT already ignore NULLs on their own. Never give up on a widget (noData)
            over this specific error without trying TRY_CAST/TRY_CONVERT first. Once TRY_CAST
            actually excluded one or more real rows from the result, that is a data-quality
            problem in the user's own file, not a solved problem — run one more query (e.g.
            WHERE TRY_CAST(col AS DECIMAL) IS NULL AND col IS NOT NULL AND col <> '') to find
            exactly which rows and values were excluded, then report them on that widget's
            "dataIssues" field (see BuildSystemPrompt's own section on it) so the user can go
            fix their file — never silently absorb the exclusion into a quietly-smaller total.
          - If a query fails for any other reason, read the error message and correct your SQL.
          """;
}
