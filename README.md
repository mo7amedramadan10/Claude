# Chat to Dashboard

An AI-powered analytics assistant: ask natural-language questions about the data files in a
folder and get back a dynamically generated dashboard (KPI cards, bar/line/pie charts, tables)
for every question.

**How it works:** on startup (and on demand), the backend scans a configurable data folder and
bulk-loads every `.csv` / `.xlsx` / `.json` file into its own table in a `staging` schema on a
shared SQL Server instance. When you ask a question, the backend calls the Anthropic Claude API
with three tools — `list_files`, `query_data` (read-only T-SQL), and optionally
`search_documents` — loops through Claude's tool calls, and returns a validated dashboard JSON
spec that the React frontend renders with Recharts.

Because the data lives in a central SQL Server (local or Azure SQL), multiple users on
different machines all query the same up-to-date data through the app — no one needs local
access to the source files.

```
┌──────────┐   POST /api/chat   ┌─────────────────┐   Messages API + tools   ┌────────┐
│ React UI │ ─────────────────▶ │ ASP.NET Core API │ ◀──────────────────────▶ │ Claude │
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

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 18+ (with npm)
- An accessible **SQL Server** instance (local SQL Server, SQL Express, or Azure SQL) and a
  connection string for a database where the app may create a `staging` schema and tables
- An [Anthropic API key](https://console.anthropic.com/)

## Setup

### 1. Configure secrets (never committed to git)

From `backend/ChatToDashboard.Api`:

```bash
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
dotnet user-secrets set "ConnectionStrings:DataDb" "Server=localhost;Database=ChatToDashboard;Trusted_Connection=True;TrustServerCertificate=True"
```

For Azure SQL, use its ADO.NET connection string instead. Both values live in
`dotnet user-secrets` — do **not** put them in `appsettings.Development.json` or commit them.

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

### 3. Run the backend

```bash
cd backend/ChatToDashboard.Api
dotnet run
```

Listens on `http://localhost:5000` and performs the initial data load on startup (a failed
load — e.g. SQL Server unreachable — is logged but doesn't stop the app).

### 4. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. The dev server proxies `/api` to the backend (CORS is also
enabled for this origin).

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
| `Anthropic:Model` | `claude-sonnet-5` | Claude model ID used for chat |
| `Anthropic:MaxTokens` | `16000` | Max tokens per Claude response |
| `ConnectionStrings:DataDb` | — | Set via user-secrets |
| `Anthropic:ApiKey` | — | Set via user-secrets |

## Safety

Generated SQL is validated before execution: it must be a single `SELECT` (or `WITH … SELECT`)
statement; write/DDL keywords (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `EXEC`, …) and
multi-statement batches are rejected, and results are capped at 500 rows server-side. SQL
errors are fed back to Claude as tool results so it can self-correct its T-SQL (up to the
loop's retry limits).
