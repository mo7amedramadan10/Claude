using System.Text.Json;

namespace ChatToDashboard.Api.History;

/// <summary>
/// Distinct query.table values a widgets JSON array depends on — the data sources a
/// re-runnable widget references. Shared by every place that enforces "only the Owner may
/// introduce a new data source to an Active dashboard": HistoryController.Update (the
/// wizard/autosave path) and ChatController.Post (the chat-continuation path — see its
/// call site for why a chat answer needs the exact same guard, not just the autosave one).
/// A widget with no query (e.g. built from list_files/search_documents alone, from more
/// than one aggregated query_data call, or a frozen chat-answered snapshot) contributes
/// nothing, matching DashboardAccessService.ExtractQueryBlob's own table extraction.
/// </summary>
public static class WidgetTableExtractor
{
    public static HashSet<string> ExtractTables(string widgetsJson)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var widgets = JsonDocument.Parse(widgetsJson).RootElement;
            if (widgets.ValueKind != JsonValueKind.Array) return tables;
            foreach (var widget in widgets.EnumerateArray())
            {
                if (!widget.TryGetProperty("query", out var query) || query.ValueKind != JsonValueKind.Object) continue;
                if (query.TryGetProperty("table", out var table) && table.ValueKind == JsonValueKind.String
                    && !string.IsNullOrWhiteSpace(table.GetString()))
                    tables.Add(table.GetString()!);
            }
        }
        catch (JsonException) { }
        return tables;
    }
}
