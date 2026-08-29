using System.Text.Json.Nodes;
using ChatToDashboard.Api.Data;

namespace ChatToDashboard.Api.Claude;

/// <summary>
/// JSON schemas for the tools exposed to Claude via the Anthropic Messages API.
/// </summary>
public static class ToolDefinitions
{
    public static JsonArray Build(DataStore db, bool includeSearchDocuments)
    {
        var rowCap = db.Provider == DbProvider.Sqlite ? "LIMIT 500" : "TOP 500";
        var tools = new JsonArray
        {
            new JsonObject
            {
                ["name"] = "list_files",
                ["description"] =
                    "Lists the available data tables with their column names and data types. " +
                    "Call this first to see what data exists.",
                ["input_schema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray(),
                },
            },
            new JsonObject
            {
                ["name"] = "query_data",
                ["description"] =
                    $"Runs a read-only SELECT query ({db.DialectName} dialect) against the loaded data tables " +
                    $"and returns the rows as JSON. Only SELECT statements are allowed; results are capped at " +
                    $"500 rows, so always use {rowCap} (or less). Reference tables as {db.TableNamingHint}.",
                ["input_schema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["sql"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = $"A single SELECT statement ({db.DialectName} dialect).",
                        },
                    },
                    ["required"] = new JsonArray { "sql" },
                },
            },
        };

        if (includeSearchDocuments)
        {
            tools.Add(new JsonObject
            {
                ["name"] = "search_documents",
                ["description"] =
                    "Searches the unstructured documents (PDF, DOCX) in the data folder and returns the " +
                    "most relevant text passages for the given query.",
                ["input_schema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["query"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "Natural-language search query.",
                        },
                    },
                    ["required"] = new JsonArray { "query" },
                },
            });
        }

        return tools;
    }
}
