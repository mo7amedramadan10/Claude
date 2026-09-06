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
from the same URL. The UI is plain HTML/CSS/JavaScript — no npm, no build step. Its one
dependency, ApexCharts, is vendored locally (`wwwroot/lib/`) rather than pulled from a CDN,
so the whole thing still works on a machine with no internet access.

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

## Accounts & permissions

Every screen and API call requires a signed-in account, with one deliberate exception: an
opened share link (see "Sharing a dashboard" below). There is no self-signup — an admin
creates every account, either as a **local** account (username + password) or an **Active
Directory** account (the username has to exist as an account here too, but the password is
never stored — it's verified against your directory on every login).

**First login:** since accounts are admin-created and there's no admin yet on a fresh install,
the app creates one automatically the first time it starts with zero accounts in the database.
Set it yourself before first run:

```bash
dotnet user-secrets set "Auth:SeedAdmin:Username" "admin"
dotnet user-secrets set "Auth:SeedAdmin:Password" "<a real password>"
```

or leave `Auth:SeedAdmin:Password` unset and a random one is generated and printed **once** in
the startup log (`docker compose logs` or the console) — sign in with it and go create real
accounts. Either way the account it creates is `Admin`, with full access to everything; change
its password (or make yourself a proper account and deactivate it) once you're in.

**Roles:** `Admin` can manage accounts (the **المستخدمون** tab), bulk-refresh data
(`POST /api/data/refresh`, the per-system ⟳ button), and see the `/usage` cost/prompt log.
`User` can chat, use history, export, and share — nothing administrative.

**Per-source permissions:** independent of role, every account also has its own allowed
systems and repository categories — the same "Sources" dropdown every question already gates
against, now enforced per account rather than trusted from the client. When creating or
editing a user, leave "الوصول لكل الأنظمة" / "لكل تصنيفات المستودع" checked for full access, or
uncheck it and pick specific systems/categories. **The server always intersects what a user
asks for with what they're actually allowed** — a user cannot widen their own access by
editing the request, only narrow it further via the Sources dropdown. Admins are never
restricted by this, regardless of what's ticked on their own account.

**Active Directory**, off by default — turn it on and point it at your domain controller via
user-secrets:

```bash
dotnet user-secrets set "ActiveDirectory:Enabled" "true"
dotnet user-secrets set "ActiveDirectory:Host" "dc01.company.local"
dotnet user-secrets set "ActiveDirectory:Domain" "company.local"
# Port defaults to 389; set ActiveDirectory:UseSsl=true and Port=636 for LDAPS.
```

A login attempt for an AD account binds to the directory **as that user** with the password
they typed — the standard way to check an AD password without needing a privileged service
account. Nothing else is read from the directory (no group sync); admins still control role
and per-source permissions here, same as a local account — only where the password lives is
different.

## Continuing a dashboard

By default, every new question **continues the dashboard currently on screen** rather than
starting from scratch: the frontend sends the last response's full `summary` and `widgets`
array (every field, `source` included) as explicit context ahead of the new question, and the
system prompt instructs the model to treat it as a base to refine or extend — narrowing/
filtering the same data, changing a time range, adding a widget alongside the existing ones —
unless the question is clearly about something else entirely.

This is a deliberate, structural decision, not the model guessing "new topic vs. follow-up"
from wording: click **"🆕 ابدأ لوحة جديدة"** next to the chat input to empty the dashboard
immediately — the panel resets to its pristine "ask a question" state right there, no need to
also ask something first. The chat transcript itself is untouched; only the dashboard clears.
Since the next question is then sent with nothing to continue, it starts completely from
scratch, exactly like the first question of a session — no separate "armed" state to track or
undo, since an empty dashboard already *is* the fresh-start signal.

The backend holds no server-side session state for this — the frontend already owns "the
dashboard currently shown to the user" (`state.dashboard`) for rendering, and simply forwards
it as `currentDashboard` on `POST /api/chat` when continuing (see `ComposeUserMessage` in
`AnalyticsTools.cs`). Earlier versions replayed the last several chat turns as text history on
every question, growing with the conversation; this replaces that with a single, complete
snapshot of what's actually on screen — no multi-turn tool-calling history is resent.

## Dashboard filters

A dashboard's suggested filters (the model's own `filters` field, or one added through the
editor) only affect a widget that carries query lineage — enough information to re-run its
query with an extra condition. A widget with none is marked **"غير متأثر بالفلتر"** rather than
silently ignoring the filter.

Two independent paths supply that lineage:
- **Wizard-built widgets** carry the full structured query (table/metric/aggregation/
  dimension/time range) chosen through **+ إضافة عنصر**, rebuilt entirely server-side
  (`WidgetQueryService.ExecuteAsync`) — never client SQL.
- **Chat-built widgets** carry just `{ table, sql }` — the exact SELECT the model used for that
  widget (see the system prompt's "حقل query" section) — re-run by `WidgetQueryService.
  ExecuteSqlFilterAsync` with the active filter spliced in as an extra `WHERE`/`AND` condition,
  found by scanning the statement's real structure (parenthesis depth, quoted strings) rather
  than naive text search, so a subquery's or CTE's own clauses are never mistaken for the outer
  statement's.

Splicing text into an already-written SELECT can't handle every shape a model might write (a
filtered column that only exists inside a subquery's own scope, for instance) — a widget that
doesn't fit stays "غير متأثر بالفلتر" rather than risking a wrong number. Accepting that SQL
from the client (it can only live there — the model returns it once and the frontend holds it
from then on) is the one place a filter's execution isn't built entirely from schema-verified
names: `ValidateReadOnlySql` (single read-only `SELECT`/`WITH` only) and `CheckSourcePermission`
(the same disabled-system/category scan `query_data` applies) both run against it before and
after the filter is spliced in, and the filter's own column/values still go through the same
schema-verified, parameterized path as the wizard's filters — never string-interpolated.

Filters and the user's current selection both survive a continuation question (e.g. "زوّد رسم
لـ...") on the frontend, not by asking the model to remember them: only `summary`+`widgets`
travel as continuation context (see "Continuing a dashboard" above), so the model has no way to
deliberately keep a filter it was never shown. `ask()` instead merges by filter `id` — every
existing filter stays exactly as it was, and only a genuinely new `id` the model proposed (for
a widget it just added, say) gets appended — and leaves `state.activeFilters` (the values
actually picked) untouched, re-running it against the updated widget set right after so a
newly-added widget reflects it immediately rather than waiting for the next filter click. A
fresh start (see above) clears both, same as the first question of a session.

## History (`السجل`)

Every question that produces at least one widget is saved automatically — no extra click.
The **السجل** tab lists saved dashboards newest-first (question, summary, timestamp, widget
count); **فتح** reopens one instantly by re-rendering the saved widgets in the browser —
it does **not** call the model again, and does not re-run any query either (see "🔄 تحديث"
below for that) — and **حذف** / **مسح الكل** remove one entry or all of them.

A saved entry carries the dashboard's filter definitions and whichever values were actually
selected when it was saved, not just the widgets — reopening it puts the filter controls back
exactly as they were, already showing the same filtered numbers, rather than losing the filter
UI entirely (leaving stale-looking data with no way to tell it had been filtered).

History is per-account — "the current user" is whoever is signed in. Each account's history
is capped at the latest 60 dashboards; older ones are dropped automatically on save.
`POST /api/history`, `GET /api/history`, `DELETE /api/history/{id}` and `DELETE /api/history`
are the underlying endpoints, backed by a `DashboardHistory` table.

## Refreshing a dashboard's data ("🔄 تحديث")

Re-runs every widget that carries query lineage (see "Dashboard filters" above) straight
against the live database, keeping whichever filter is currently applied — it's the same
`applyFilters()` path a filter click already uses, just triggered on demand instead of only
after picking a filter value. A widget with no query lineage at all (built from several
combined tool calls, or a forecast) can't be refreshed this way and simply keeps its existing
data, exactly like it already does for filtering.

## Building a dashboard from an image

The 🖼️ button next to the chat input attaches a reference image — a screenshot of a
dashboard from another tool, a mockup, a hand-drawn sketch. The browser downscales it to a
manageable size, and it goes to the model as part of the question (both Claude and OpenAI
vision models are supported; no extra config needed). The model treats the image purely as a
**layout reference** — how many widgets, what chart types, roughly what they're titled — and
still has to build every number the normal way, through `list_files`/`query_data` against your
real data; it never reads numbers off the picture. If a widget in the image has no real data to
back it, the model substitutes something it can actually support and says so in the summary. You
can submit with just the image and no typed question — a default prompt ("rebuild this
dashboard from my real data") is used.

## Sharing a dashboard

**🔗 مشاركة** publishes the current dashboard under a random link (`/?share=<id>`) and shows
it in a copyable box. Opening that link is the **one deliberate exception** to "everything
requires an account": anyone with it gets a read-only page — just the widgets, a banner naming
the original question, and a link back to the full app — no sign-in needed. It's served by the
same `index.html`; a `?share=` query string switches it into that stripped-down view instead of
the normal chat UI.

"Who can see it" is exactly "who has the link" — the same trust model as most lightweight
share links (Google Docs, Notion, etc.): the id is 16 random hex characters (64 bits), not
sequential or guessable, but anyone holding it can view. Manage what you've published with
`GET /api/share` (your own links) and `DELETE /api/share/{id}`; deleting one immediately breaks
that link for everyone.

## Exporting a dashboard (PDF / PowerPoint)

Two buttons appear above any generated dashboard:

- **تصدير PDF** — the browser's own print-to-PDF: it prints just the dashboard (no header,
  chat rail, or buttons — see the `@media print` rules in `index.html`), so "Save as PDF" in
  the print dialog is the export. No server round trip.
- **تصدير PowerPoint** — downloads a real, editable `.pptx`: a title slide (question +
  summary) followed by one slide per widget. KPI values and tables are sent as plain data and
  land as native, still-editable PowerPoint text/tables; bar/line/pie widgets are rendered as
  inline SVG by ApexCharts, which can export its own current state to a PNG (`chart.dataURI()`)
  and only that image is sent — `POST /api/export/pptx` has no charting code of its own,
  `Export/PptxBuilder.cs` just assembles the OOXML package (`DocumentFormat.OpenXml`) from a
  title, a summary, and that per-widget data.

## Dashboard design system

The model only ever controls a widget's **content** — `type`, `title`, `data` (and `xKey`/
`yKey` for bar/line) — never its layout or styling. Everything visual is a fixed rule in the
frontend, so every dashboard looks like one consistent product no matter what question or
model produced it:

- **Grid rules keyed off `type` alone** (`index.html`, `.widget`/`.widget.kpi`/`.widget.chart`/
  `.widget.table`): a `kpi` is always the small card; `bar`, `line`, `pie` and `table` always
  span 2 grid columns. A widget's data volume or title length never changes its size.
- **One design-token block** — the `:root` CSS custom properties (colors, fonts, radius) plus
  the matching `THEMES` JS object (chart palette, grid/tick color, font family) a few lines
  below it — is the single source every card and every chart pulls from. No component sets a
  color or font ad hoc.
- **Light and dark mode** — the 🌙/☀️ button in the header (`applyTheme()`) toggles a
  `data-theme` attribute on `<html>`, which switches which block of `:root` custom properties
  is active; the choice is remembered in `localStorage` and applied by an inline script in
  `<head>` before the stylesheet paints, so there's no flash of the wrong theme on load. CSS
  can't recolor an already-drawn chart's SVG, so `applyTheme()` also rebuilds any dashboard
  that's currently on screen (`renderDashboard()`) so its charts pick up the new palette too.
  The chart color palette itself is identical in both modes (only grid/tick/accent colors
  change) so a category means the same color regardless of theme.
- **Five fixed widget components** — `KpiCard`, `BarChartCard`/`LineChartCard` (sharing
  `buildXyChart`), `PieChartCard`, `TableCard` — and every widget routes through exactly one of
  them, chosen by `buildWidget()`. Chart components build their ApexCharts options through one
  shared `chartConfig()`, so the palette/grid/tick/font are set in exactly one place rather
  than per chart instance. `mountChart()` only *queues* a chart (in `pendingCharts`); the
  actual `new ApexCharts(...)` construction happens in `renderDashboard()` right after the
  widget grid is attached to the document — ApexCharts measures its container's real size at
  construction time, so building it any earlier renders at 0×0.
- **Strict whitelist** — `WIDGET_TYPES = ['kpi','bar','line','pie','table']`. A type outside
  that list (which in practice the backend already rejects — see `DashboardWidget.Validate()`
  in `Models/DashboardSpec.cs` — but a saved History/Share entry could in principle carry
  anything) renders as a `TableCard` instead, with a `console.warn`, rather than breaking the
  grid or crashing the page.
- **Frontend data-volume guard** (`capRows()`) — independent of whatever the system prompt
  asks the model to do, a bar/line/pie chart never renders more than 15 points; anything past
  that is folded into one summed "+N more" bucket rather than an unreadably dense chart.

`wwwroot/lib/apexcharts.min.js` is ApexCharts vendored locally rather than pulled from a CDN.
**Pinned to 5.0.0** — the last MIT-licensed release; 5.1+ switched to a dual license that
requires a paid commercial license once the organization using it clears $2M/year in revenue
(see the package's own `LICENSE`), so do not casually bump this past 5.0.0. To update within
that constraint: `npm pack apexcharts@5.0.0`, extract the tarball, and copy
`package/dist/apexcharts.min.js` over that file.

## Forecasting

A predicted future value is always computed by a real statistic — ordinary least-squares
linear regression, with an additive seasonal adjustment when the series covers at least two
full cycles (`Widgets/ForecastService.cs`) — never guessed by the model or the UI. Two ways to
get one, both going through that same service:

- **The "🔮 توقّع الأشهر الجاية" button** on any bar/line widget (`POST /api/widgets/forecast`
  in `WidgetsController.cs`) works on whatever data the chart already has client-side — no SQL,
  no LLM call, identical for a wizard-built widget or a chat-authored one.
- **The `forecast_data` tool** (`AnalyticsTools.cs`, alongside `query_data`) lets the model
  answer a chat-typed forecast request with real numbers: it runs the model's own two-column
  time-series SQL, then hands the value column to `ForecastService`.

Either way the result lands in a widget's `forecast` field (`Models/DashboardSpec.cs`) —
labels/values/lower/upper/method/note/r2 — kept strictly separate from the widget's real `data`.
The frontend (`forecastChartConfig()` in `index.html`) renders it as a dashed continuation in a
different color from the historical series, with the forecast's own legend entry, never blended
in as if it were an observed value. The confidence interval itself widens automatically for a
shorter or noisier historical series (the standard OLS prediction-interval formula) — a short
enough series also gets an explicit note saying so. It renders as two dashed bound lines rather
than a shaded band: ApexCharts' `rangeArea` series type (verified against the exact vendored
5.0.0) silently fails to render when mixed into a combo chart with a bar/line series, so a
shaded fill wasn't a reliable option here.

## Narration vs. summary vs. source

Every response carries three distinct pieces of user-facing text, each with its own job and
its own audience:

- **`summary`** — one or two sentences, compact. Shown as the dashboard pane's own header, and
  reused internally (the "current dashboard" context on a follow-up question, the one-line
  preview in **السجل**) — anywhere a short label is all that's needed.
- **`narration`** — a richer walkthrough in Modern Standard Arabic (الفصحى), roughly 4–8
  sentences, that reads through the widgets in order with real narrative transitions instead of
  a repeated per-widget template. This is what the chat bubble displays and what 🔊/auto-read
  speaks. It never mentions how the answer was produced — no table, file, category, "قاعدة
  بيانات", "استعلام", or column name — only what the data means.
- **`source`** (per widget, behind its ⓘ button) — exactly the technical provenance: which
  table/file/system the number came from, and how it was calculated. `narration` and `source`
  are deliberately disjoint: provenance lives only in `source`, meaning lives only in
  `narration`.

## Voice

Two independent, browser-native features — no server-side speech code, no cloud speech API,
each usable without the other:

- **Speech-to-text** (🎤 button by the chat input): the Web Speech API transcribes into the
  same `#q` box any typed question already goes through — voice is just an alternate way to
  fill that box, never a separate path, so every existing rule (sources, filters, the tool
  loop) applies unchanged. It never auto-sends: Arabic recognition can mishear a dialect word,
  a technical term, or an exact entity name, so the transcript stays editable until the user
  presses "إرسال" themselves. Only shown when the browser actually implements
  `SpeechRecognition`/`webkitSpeechRecognition` — Chrome and Edge; Firefox and Safari don't, at
  least not without a flag.
- **Text-to-speech** (🔊 button on any bot message, plus an opt-in "قراءة تلقائية" toggle in the
  header that speaks every *new* answer as it arrives): reads the rich `narration` field (see
  "Narration vs. summary vs. source" below) — never the short `summary`, and never a widget's
  `source` — since a chart or a table has no meaningful spoken form and the UI never tries to
  read one aloud. Auto-read defaults to off and is remembered
  per-browser (`localStorage`), since an unannounced voice reading a business number aloud is
  the wrong call in a shared office; the manual per-message button has no such restriction.
  While it reads, the widget the current sentence is talking about gets a soft highlight
  (`.tts-highlight`) so the user can follow along on the dashboard — a best-effort match
  (a sentence "names" a widget when the widget's title appears in it verbatim), driven by
  `SpeechSynthesisUtterance`'s `boundary` event, so it costs nothing extra from the model or
  the data contract and simply shows no highlight when a sentence doesn't name a widget by
  title, or when a browser doesn't fire `boundary` events at all.

Both rely on the browser's own built-in engine (`SpeechRecognition`/`speechSynthesis`) rather
than a cloud speech service — free and immediate, but recognition quality varies by OS/browser
and Arabic dialect. If this graduates from pilot to production and that quality gap matters,
Azure Speech Services is the natural next step (this deployment already lives in Azure) — it
keeps audio inside the same Azure tenant rather than sending it to a third party, and supports
picking a specific Arabic dialect. That's a deliberate scope cut for now, not an oversight.

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
| `Auth:SeedAdmin:Username` | `admin` | Username for the auto-created first admin account |
| `Auth:SeedAdmin:Password` | — | Set via user-secrets; leave unset to get a random generated one, logged once |
| `ActiveDirectory:Enabled` | `false` | Turn on AD login |
| `ActiveDirectory:Host` | — | Domain controller hostname/IP, via user-secrets |
| `ActiveDirectory:Domain` | — | DNS domain used to build the login UPN (`user@domain`) |
| `ActiveDirectory:Port` / `:UseSsl` | `389` / `false` | `636` + `true` for LDAPS |

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
