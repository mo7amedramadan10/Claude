using System.Text.Json.Nodes;

namespace ChatToDashboard.Api.Claude;

/// <summary>
/// JSON schemas for the tools exposed to Claude via the Anthropic Messages API.
/// </summary>
public static class ToolDefinitions
{
    public static JsonArray Build(bool includeSearchDocuments)
    {
        var tools = new JsonArray
        {
            new JsonObject
            {
                ["name"] = "list_files",
                ["description"] =
                    "Lists the data tables available in the SQL Server [staging] schema, " +
                    "with their column names and SQL data types. Call this first to see what data exists.",
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
                    "Runs a read-only T-SQL SELECT query (SQL Server dialect) against the [staging] schema " +
                    "and returns the rows as JSON. Only SELECT statements are allowed; results are capped at " +
                    "500 rows, so always use TOP 500 (or less). Reference tables as staging.<TableName>.",
                ["input_schema"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["sql"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "A single T-SQL SELECT statement (SQL Server dialect).",
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
