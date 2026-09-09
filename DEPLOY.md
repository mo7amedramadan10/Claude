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
# OLLAMA_API_KEY=...                  # only if you switch the provider to Ollama in the dashboard
# OLLAMA_BASE_URL=http://...          # defaults to the internal gateway URL baked into the image
# OLLAMA_MODEL=qwen3:14b
```

Changing any of these after the container is already running requires `docker compose up -d
--build` again — editing `.env` alone does not restart the container, and (importantly)
running `dotnet user-secrets set` on your own machine has **no effect on a deployed
container at all**: user secrets are a local-development-only mechanism, read from a file
on your machine that never ships with the image. Every credential the deployed app needs
must go through `.env` here (or the container/App Service's own environment settings).

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
   - `Ollama__ApiKey` = your Ollama gateway token, only if you switch the provider to
     Ollama in the dashboard (same caveat as above: `dotnet user-secrets` never reaches
     this app — set it here)
   - `WEBSITES_PORT` = `8080`
   - optionally `DataFolderPath` = `/data` with an Azure Files mount at `/data` so you can
     update data files without rebuilding the image (the baked-in seed data is used otherwise).
4. Open `https://<app>.azurewebsites.net`. HTTPS is automatic.

Note: on Azure SQL the app cannot create the database itself (no `master` access) — create
it in step 1; the app still creates the `staging` schema and tables on its own.

## Option C: Kubernetes on the company server (multiapps) — CI/CD via Azure Pipelines

This is the current production path. Every push to `main` builds the image on the
server, pushes it to the internal registry and rolls it out to the cluster. Nothing
needs to be installed on anyone's laptop.

### What's in the repo

| File | Purpose |
|---|---|
| `Dockerfile` | Multi-stage build: .NET 8 SDK → aspnet runtime, runs as non-root, port 8080 |
| `azure-pipelines.yml` | Build & push (podman) → deploy (kubectl) on the self-hosted pool `multiapps` |
| `k8s/00-namespace.yaml` | Namespace `ai-dashboard` |
| `k8s/10-configmap.yaml` | Non-secret settings (LLM provider, models, Ollama URL, data path) |
| `k8s/20-mssql.yaml` | SQL Server 2022 StatefulSet with a 20 GB persistent volume |
| `k8s/30-app.yaml` | App Deployment, data volume (10 GB), Service, NodePort 30080, Ingress |
| `k8s/secret.example.yaml` | Shape of the Secret — real values come from the pipeline, never from git |

### One-time setup in Azure DevOps

1. **Variable group**: Pipelines → Library → *+ Variable group* named `ai-dashboard` with
   `SQL_PASSWORD`, `OPENAI_API_KEY`, `ANTHROPIC_API_KEY`, `OLLAMA_API_KEY`,
   `SEED_ADMIN_PASSWORD` (click the lock icon on each to make it secret). Under
   *Pipeline permissions* allow this pipeline.
2. **Pipeline**: Pipelines → *New pipeline* → Azure Repos Git → `AI_Dashboard` →
   *Existing Azure Pipelines YAML file* → `/azure-pipelines.yml` → Run.
   First run may ask you to *Permit* the `multiapps` agent pool and the variable group.

### Where the app is

- By IP, no DNS needed: **http://172.17.131.40:30080**
- By name through ingress-nginx: **http://ai-dashboard.multiapps.local** — ask IT for a
  DNS record pointing at `172.17.131.40`, or add that line to your hosts file.

First login: `admin` / the `SEED_ADMIN_PASSWORD` you set. Change it from the UI.

### Day-2 operations (on the server, as root)

```bash
kubectl -n ai-dashboard get pods                       # health at a glance
kubectl -n ai-dashboard logs deploy/ai-dashboard -f    # app logs
kubectl -n ai-dashboard logs statefulset/mssql         # SQL Server logs
kubectl -n ai-dashboard rollout restart deploy/ai-dashboard   # restart the app
kubectl -n ai-dashboard rollout undo deploy/ai-dashboard      # roll back to previous image
```

**Data files** live on the volume behind PVC `ai-dashboard-data`. To add files:

```bash
POD=$(kubectl -n ai-dashboard get pod -l app=ai-dashboard -o jsonpath='{.items[0].metadata.name}')
kubectl -n ai-dashboard cp ./my_report.xlsx $POD:/data/
curl -X POST -b cookie.txt http://172.17.131.40:30080/api/data/refresh   # after signing in
```

**Change a setting** (model, provider, Ollama URL): edit `k8s/10-configmap.yaml`, push to
`main`. **Rotate a secret**: update the variable group and re-run the pipeline.

**Backups**: the SQL data is on the `data-mssql-0` PVC under
`/opt/local-path-provisioner/` on the server. Take a SQL backup with
`kubectl -n ai-dashboard exec mssql-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SQL_PASSWORD" -C -Q "BACKUP DATABASE ChatToDashboard TO DISK='/var/opt/mssql/ChatToDashboard.bak'"`
and copy the `.bak` out with `kubectl cp`.

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
