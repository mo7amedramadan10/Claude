using System.Text.Json;
using ChatToDashboard.Api.Llm;
using ChatToDashboard.Api.Sources;
using ChatToDashboard.Api.Users;

namespace ChatToDashboard.Api.History;

/// <summary>
/// The permission surface for Active dashboards (Part 2): whether a given user is eligible
/// to be Owner (i.e. holds valid permission on every source the dashboard's widgets query),
/// and resolving who a dashboard's live query should run as. Reuses the exact same
/// category/system/per-file gate the chat tool-calling loop already applies to a fresh
/// question (<see cref="AnalyticsTools.CheckSourcePermission"/>) — a candidate is eligible
/// only if that same check passes against every table their widgets touch.
/// </summary>
public class DashboardAccessService
{
    private readonly AnalyticsTools _tools;
    private readonly UserStore _users;

    public DashboardAccessService(AnalyticsTools tools, UserStore users)
    {
        _tools = tools;
        _users = users;
    }

    /// <summary>Null if <paramref name="candidate"/> may own a dashboard whose widgets are
    /// <paramref name="widgetsJson"/>; otherwise the Arabic reason they can't.</summary>
    public async Task<string?> CheckEligibilityAsync(AppUser candidate, string widgetsJson, CancellationToken ct = default)
    {
        var blob = ExtractQueryBlob(widgetsJson);
        if (blob.Length == 0) return null; // no data-backed widgets to depend on

        var selection = PermissionsService.GetEffectiveSelection(candidate, SourceSelection.AllEnabled());
        var context = await _tools.DescribeSourcesAsync(selection, ct);
        return AnalyticsTools.CheckSourcePermission(blob, context);
    }

    /// <summary>Convenience overload resolving the user by id first.</summary>
    public async Task<string?> CheckEligibilityAsync(string userId, string widgetsJson, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId, ct);
        if (user is null) return "المستخدم غير موجود.";
        return await CheckEligibilityAsync(user, widgetsJson, ct);
    }

    /// <summary>
    /// Concatenates every widget's query.sql / query.table text into one blob that
    /// <see cref="AnalyticsTools.CheckSourcePermission"/> can scan with its existing
    /// substring-match logic — the same shape ShareController.RefreshWidget already reads
    /// per-widget, just gathered across all of them at once.
    /// </summary>
    private static string ExtractQueryBlob(string widgetsJson)
    {
        try
        {
            var widgets = JsonDocument.Parse(widgetsJson).RootElement;
            if (widgets.ValueKind != JsonValueKind.Array) return "";
            var parts = new List<string>();
            foreach (var widget in widgets.EnumerateArray())
            {
                if (!widget.TryGetProperty("query", out var query) || query.ValueKind != JsonValueKind.Object)
                    continue;
                if (query.TryGetProperty("sql", out var sql) && sql.ValueKind == JsonValueKind.String)
                    parts.Add(sql.GetString() ?? "");
                if (query.TryGetProperty("table", out var table) && table.ValueKind == JsonValueKind.String)
                    parts.Add(table.GetString() ?? "");
            }
            return string.Join("\n", parts);
        }
        catch (JsonException)
        {
            return "";
        }
    }
}
