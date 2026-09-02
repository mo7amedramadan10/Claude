# Deploying Chat to Dashboard to a server

Deploy once; after that, everyone — including you — uses the app from a browser URL with
nothing installed on their machines.

## Option A: Any server with Docker (VPS, on-prem box, cloud VM)

This is the simplest path. The included `docker-compose.yml` runs the whole stack: the app
(one container serving both the API and the UI) plus SQL Server, with the database created automatically
on first start.

### 1. Get a server

Any Linux VM with ~4 GB RAM (SQL Server's minimum is 2 GB). Examples: Azure VM, AWS
Lightsail, DigitalOcean, Hetzner, or a machine on your office network.

### 2. Install Docker on the server

```bash
curl -fsSL https://get.docker.com | sh
```

### 3. Clone and configure

```bash
git clone -b claude/chat-to-dashboard-app-at0up6 https://github.com/mo7amedramadan10/Claude.git chat-to-dashboard
cd chat-to-dashboard
```

Create a `.env` file next to `docker-compose.yml`:

```env
SQL_PASSWORD=A_Strong_Passw0rd!       # SQL Server 'sa' password (min 8 chars, upper+lower+digit)
ANTHROPIC_API_KEY=sk-ant-...          # your Anthropic API key
# Optional:
# ANTHROPIC_MODEL=claude-sonnet-5
# ENABLE_RAG=true
```

### 4. Start

```bash
docker compose up -d --build
```

First start takes a couple of minutes (SQL Server initialization). Then open
`http://<server-ip>:8080` — the chat UI and the API are served from the same port.

### 5. Your data

Drop `.csv` / `.xlsx` / `.json` files into the `data/` folder on the server (it is mounted
into the app container), then reload:

```bash
curl -X POST http://<server-ip>:8080/api/data/refresh
```

Files can also be synced to that folder by any means you like (rsync, OneDrive client,
scheduled copy) — the refresh endpoint picks up whatever is there.

### Updating the app

```bash
git pull && docker compose up -d --build
```

## Option B: Azure App Service + Azure SQL (no VM to manage)

1. **Azure SQL**: create a serverless Azure SQL Database (e.g. `ChatToDashboard`). Copy its
   ADO.NET connection string. Allow Azure services in its firewall.
2. **Container build**: build and push the image from the repo root:
   ```bash
   az acr create -n <registry> -g <rg> --sku Basic --admin-enabled true
   az acr build -r <registry> -t chat-to-dashboard:latest .
   ```
3. **Web App for Containers**: create an App Service (Linux, container) pointing at
   `<registry>.azurecr.io/chat-to-dashboard:latest`, and set these application settings:
   - `ConnectionStrings__DataDb` = the Azure SQL connection string
   - `Anthropic__ApiKey` = your key
   - `WEBSITES_PORT` = `8080`
   - optionally `DataFolderPath` = `/data` with an Azure Files mount at `/data` so you can
     update data files without rebuilding the image (the baked-in seed data is used otherwise).
4. Open `https://<app>.azurewebsites.net`. HTTPS is automatic.

Note: on Azure SQL the app cannot create the database itself (no `master` access) — create
it in step 1; the app still creates the `staging` schema and tables on its own.

## Security checklist (read before sharing the URL)

- **Every page and API call requires a signed-in account** (see README "Accounts &
  permissions") except one deliberate exception: a published share link
  (`/?share=<id>`) is viewable by anyone who has it, by design. The very first account
  is either the one you set via `Auth:SeedAdmin:Username`/`Password` (user-secrets)
  before first run, or a random one generated and logged once on first startup — check
  the startup log if you didn't set it yourself. Change that password (or replace the
  account) before sharing the URL beyond yourself.
- **Use HTTPS** for anything reachable from the internet (App Service gives it for free; on
  a VPS put Caddy or nginx with Let's Encrypt in front of port 8080).
- **Database user**: the compose file uses `sa` for simplicity. For a hardened setup, create
  a dedicated login owning just the app database and use it in `ConnectionStrings__DataDb`.
  The app needs to create/drop tables in the `staging` schema; generated queries are
  validated to be single read-only `SELECT` statements before execution.
- **Secrets** live only in the server's `.env` / App Service settings — never commit them.
