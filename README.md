# Chat to Dashboard

An AI-powered analytics assistant: ask natural-language questions about the data files in a
folder and get back a dynamically generated dashboard (KPI cards, bar/line/pie charts, tables)
for every question.

**How it works:** on startup (and on demand), the backend scans a configurable data folder and
bulk-loads every `.csv` / `.xlsx` / `.json` file into its own table in a `staging` schema on a
shared SQL Server instance. When you ask a question, the backend calls the Anthropic Claude API
with three tools — `list_files`, `query_data` (read-only T-SQL), and optionally
`search_documents` — loops through Claude's tool calls, and returns a validated dashboard JSON
spec that the built-in UI renders as charts.

Everything is **one ASP.NET Core project**: the API and the UI ship together and are served
from the same URL. The UI is plain HTML/CSS/JavaScript with hand-written SVG charts — no npm,
no build step, and no CDN, so it works even on a machine with no internet access.

Because the data lives in a central SQL Server (local or Azure SQL), multiple users on
different machines all query the same up-to-date data through the app — no one needs local
access to the source files. To try it on a single machine with nothing to install, it can also
run against a local SQLite file (`"DatabaseProvider": "Sqlite"`).

```
┌──────────┐   POST /api/chat   ┌─────────────────┐   Messages API + tools   ┌────────┐
│ Built-in │ ─────────────────▶ │ ASP.NET Core app │ ◀──────────────────────▶ │ Claude │
│    UI    │                    │  (serves the UI) │                          │        │
└──────────┘   dashboard JSON   └─────────────────┘                          └────────┘
                                        │  Dapper / SqlBulkCopy
                                        ▼
                                 ┌─────────────┐        ┌─────────────────┐
                                 │ SQL Server  │ ◀───── │ data folder      │
                                 │ [staging].* │  load  │ (.csv/.xlsx/...) │
                                 └─────────────┘        └─────────────────┘
```

> **Deploying to a server?** See [DEPLOY.md](DEPLOY.md) — one `docker compose up` runs the
> whole stack (app + SQL Server), and users only need a browser.

## Prerequisites

- [.NET SDK 8 or newer](https://dotnet.microsoft.com/download/dotnet/8.0) (the project targets
  net8.0 and rolls forward, so a newer SDK such as .NET 10 works on its own)
- An LLM API key — either [Anthropic](https://console.anthropic.com/) (default) or
  [OpenAI](https://platform.openai.com/api-keys); set `Llm:Provider` to pick between them
- A database — either:
  - **SQL Server** (local, Express, or Azure SQL) — the default, and what makes the data
    centrally queryable for everyone; or
  - **nothing to install** — set `"DatabaseProvider": "Sqlite"` and the app keeps its data in a
    local file. Good for trying it out on one machine; not for the shared multi-user setup.

## Setup

### 1. Configure secrets (never committed to git)

From `backend/ChatToDashboard.Api`:

```bash
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
dotnet user-secrets set "ConnectionStrings:DataDb" "Server=localhost;Database=ChatToDashboard;Trusted_Connection=True;TrustServerCertificate=True"
```

For Azure SQL, use its ADO.NET connection string instead. Both values live in
`dotnet user-secrets` — do **not** put them in `appsettings.Development.json` or commit them.
The app creates the database itself if it doesn't exist (where it has permission to; on Azure
SQL, create the database first).

**Out of the box the app runs on OpenAI with a local SQLite file**, so the only thing you have
to provide is the key:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."
```

To use Claude instead, set `"Provider": "Anthropic"` under `Llm` and store `Anthropic:ApiKey`.

Both providers run the identical agent — same tools, same read-only SQL validation, same
dashboard schema and retry logic; only the wire protocol differs (Anthropic tool use vs.
OpenAI tool calling). Change `OpenAI:Model` to pick a different model.

**No database server?** Set `"DatabaseProvider": "Sqlite"` in `appsettings.json` and skip the
connection string entirely — the app creates `chat-to-dashboard.db` next to the binary on first
run. Only the Anthropic key is then required. Everything else works the same; Claude is told to
write SQLite SQL instead of T-SQL, and tables are named `staging_<FileName>` (SQLite has no
schemas) rather than `staging.<FileName>`.

### 2. Configure the data folder

`DataFolderPath` in `backend/ChatToDashboard.Api/appsettings.json` defaults to `../../data`
(the `data/` folder at the repo root, which ships with `sample_sales.csv` so the app works out
of the box). Point it anywhere — including a **synced OneDrive folder**, which is just a normal
local path once OneDrive has synced it:

```jsonc
"DataFolderPath": "C:\\Users\\me\\OneDrive\\TeamData"
```

or override with the `DataFolderPath` environment variable.

Supported structured files: `.csv`, `.xlsx`, `.json` (array of objects). Each file becomes a
table `staging.<FileName>` — dropped and recreated on every load, so re-running or refreshing
always reflects the current files.

### 3. Run it

```bash
cd backend/ChatToDashboard.Api
dotnet run
```

Open `http://localhost:5000` — the UI and the API are both served there. The initial data load
runs at startup (a failure — e.g. SQL Server unreachable — is logged but doesn't stop the app).

## Connecting a back-office system to an API

A system in the `Sources` list becomes a real data source by giving it an `Api` block. Its
records are fetched on startup and on `POST /api/data/refresh`, flattened into a staging table
(`staging_sys_<id>`), and from then on queried with SQL like any loaded file — so the model can
aggregate over them and the dashboard charts them.

```jsonc
{
  "Id": "procurement",
  "Name": "نظام المشتريات",
  "Api": {
    "Url": "https://internal-host/api/services/app/ServiceRequests/GetAllServiceRequestsForAi",
    "Method": "GET",
    "ResultPath": "",              // empty = auto-detect result.items, result, items, data…
    "MaxRecords": 20000,
    "TimeoutSeconds": 60,
    "AllowInvalidCertificate": false,
    "Headers": { }                 // e.g. { "Authorization": "Bearer …" } if it needs one
  }
}
```

- **Response shape** — the array of records is found automatically for the common envelopes
  (`result.items`, `result`, `items`, `data`, or a bare top-level array). If the endpoint nests
  it somewhere else, set `ResultPath` to the dotted path.
- **Nested objects** are flattened into dotted columns: `vendor: { name }` becomes a
  `vendor.name` column. Arrays are kept as their JSON text.
- **Gating still applies** — switch the system off in the Sources dropdown and its table
  disappears from `list_files`, while a query touching it is refused by name.
- **The endpoint must be reachable from wherever the app runs.** For an intranet-only endpoint
  that means running the app inside the network (or on a host with a route to it); a failure is
  logged at startup and the system simply stays empty rather than stopping the app.
- `AllowInvalidCertificate` disables TLS validation for that one system. Use it only for an
  internal server with a self-signed certificate.

**Refreshing one system:** the Sources dropdown shows a ⟳ button beside every system that has
an endpoint, with its record count and the time of the last fetch underneath (or the error, if
the fetch failed). `POST /api/sources/{id}/refresh` does the same thing from a script.
`POST /api/data/refresh` still reloads everything — files and all systems — at once.

## Usage & observability (`/usage`)

A separate page — linked from the header, or open `http://localhost:5000/usage` directly — shows
exactly what was sent to the model and what it cost. Every question is logged with:

- totals: questions asked, input/output/cached tokens, estimated cost, average latency, and a
  per-model breakdown;
- one row per question: model, status, number of model round-trips and tool calls, tokens, cost
  and duration;
- click any row for the full record — the system prompt as sent, every tool call with its input
  and the result handed back to the model, each round-trip's complete request and response
  bodies, and the final answer.

Cost is estimated from the `Pricing` section of `appsettings.json` (price per **million**
tokens). Anthropic list prices are prefilled; fill in your own OpenAI rates from
platform.openai.com/pricing — a model with no price shows an em dash rather than a wrong number.
Token counts themselves always come from the provider's own `usage` block, not an estimate.

`DELETE /api/usage` (or the "مسح السجل" button) clears the log. Note the log stores prompts and
tool results verbatim, which includes rows from your data — it lives in your own database.

## Refreshing data

When files in the data folder change, reload all staging tables without restarting:

```bash
curl -X POST http://localhost:5000/api/data/refresh
```

`GET /api/data/tables` lists what is currently loaded.

## Optional: unstructured documents (RAG)

Set `"EnableRag": true` in `appsettings.json` to index `.pdf` and `.docx` files from the data
folder and expose a `search_documents` tool to Claude. PDF text is extracted with
[PdfPig](https://github.com/UglyToad/PdfPig); DOCX text is read straight from the document XML.
Search itself is a deliberately lightweight keyword-overlap scorer over text chunks (zero extra
infrastructure); it is isolated behind the feature flag and the tool interface so you can swap
in a real embeddings + vector-store pipeline (e.g. Qdrant) without touching the tool-use loop.

## Configuration reference (`appsettings.json`)

| Key | Default | Meaning |
|---|---|---|
| `DataFolderPath` | `../../data` | Folder scanned for data files (env var `DataFolderPath` overrides) |
| `EnableRag` | `false` | Index PDF/DOCX and expose `search_documents` |
| `DatabaseProvider` | `Sqlite` | `Sqlite` (local file, nothing to install) or `SqlServer` (shared/central) |
| `Llm:Provider` | `OpenAI` | `OpenAI` or `Anthropic` |
| `Anthropic:Model` | `claude-sonnet-5` | Claude model ID used for chat |
| `OpenAI:Model` | `gpt-4o` | OpenAI model ID (used when `Llm:Provider` is `OpenAI`) |
| `OpenAI:BaseUrl` | `https://api.openai.com/` | Override for Azure OpenAI or a compatible gateway |
| `Anthropic:MaxTokens` | `16000` | Max tokens per Claude response |
| `ConnectionStrings:DataDb` | — | Set via user-secrets |
| `Anthropic:ApiKey` | — | Set via user-secrets |
| `OpenAI:ApiKey` | — | Set via user-secrets |
| `Pricing:Models` | Anthropic list prices | Price per million tokens, per model, for the `/usage` cost estimate |

## Safety

Generated SQL is validated before execution: it must be a single `SELECT` (or `WITH … SELECT`)
statement; write/DDL keywords (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `EXEC`, …) and
multi-statement batches are rejected, and results are capped at 500 rows server-side. SQL
errors are fed back to Claude as tool results so it can self-correct its SQL (up to the
loop's retry limits).

## Opening in Visual Studio

Open `ChatToDashboard.sln` at the repo root and press F5. That runs the whole app — it opens
on `http://localhost:5000` with the UI included.

Secrets are easiest to set from the IDE: right-click the **ChatToDashboard.Api** project →
**Manage User Secrets**, which opens `secrets.json`:

```json
{
  "Anthropic:ApiKey": "sk-ant-api03-...",
  "ConnectionStrings:DataDb": "Server=localhost;Database=ChatToDashboard;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Omit the connection string when running with `"DatabaseProvider": "Sqlite"`.

To change the UI, edit `backend/ChatToDashboard.Api/wwwroot/index.html` and refresh the
browser — there is no build step.
