# Book Store — Semantic Search API

**Semantic Search in .NET 10 using PostgreSQL + pgvector**

A .NET 10 Web API that searches books by **meaning**, not just keywords. It stores vector embeddings in PostgreSQL using the **pgvector** extension and generates embeddings locally with **Ollama** (free, no API key required).

---

## What this project does

1. Stores books with `title`, `description`, `author`, and a **384-dimension vector** embedding.
2. Generates embeddings via Ollama (`all-minilm` model).
3. Runs **semantic search** using pgvector cosine distance through EF Core LINQ.
4. Exposes two REST endpoints with **Swagger** UI.

---

## Architecture

```
Client (Swagger / Postman)
        │
        ▼
  BookStore API (.NET 10)
        │
        ├──► Ollama (localhost:11434)     → generate embeddings
        │
        └──► PostgreSQL + pgvector (localhost:5433) → store & search vectors
```

---

## Prerequisites

| Tool | Purpose | Download |
|------|---------|----------|
| **Docker Desktop** | Run PostgreSQL + pgvector and Ollama in containers | [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop/) |
| **.NET 10 SDK** | Build and run the API | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) |
| **Git** (optional) | Clone the repository | [git-scm.com](https://git-scm.com/) |

**Windows requirements for Docker Desktop:**
- Windows 10/11 64-bit
- WSL 2 (Docker Desktop installs/configures this during setup)
- Virtualization enabled in BIOS (if prompted)

---

## Step 1 — Install Docker Desktop (Windows)

1. Download **Docker Desktop for Windows** from [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/).
2. Run the installer (`Docker Desktop Installer.exe`).
3. When prompted, enable **Use WSL 2 instead of Hyper-V** (recommended).
4. Restart your computer if the installer asks you to.
5. Open **Docker Desktop** from the Start menu.
6. Wait until the tray icon shows **Docker Desktop is running** (not "Starting…").

### Verify Docker is working

Open **PowerShell** and run:

```powershell
docker version
```

You should see both **Client** and **Server** sections. If **Server** is missing, Docker Desktop is not running yet.

```powershell
docker compose version
```

---

## Step 2 — Install .NET 10 SDK

1. Download the **.NET 10 SDK** from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).
2. Run the installer.
3. Verify in PowerShell:

```powershell
dotnet --version
```

Expected output example: `10.0.x`

---

## Step 3 — Open the project

```powershell
cd Book-store
```

Restore NuGet packages:

```powershell
dotnet restore
```

---

## Step 4 — Start Docker services

From the project root (where `docker-compose.yml` is located):

```powershell
docker compose up -d
```

This starts two containers:

| Service | Container name | Host port | Description |
|---------|----------------|-----------|-------------|
| PostgreSQL + pgvector | `bookstore-postgres` | **5433** | Database (`bookstore`) — port 5433 avoids conflict with local Postgres on 5432 |
| Ollama | `bookstore-ollama` | **11434** | Local embedding API |

### Verify containers are running

```powershell
docker compose ps
```

Expected: both services show **running** or **Up**.

```powershell
docker ps
```

You should see ports like `0.0.0.0:5433->5432/tcp` and `0.0.0.0:11434->11434/tcp`.

### Check PostgreSQL is ready

```powershell
docker exec bookstore-postgres pg_isready -U postgres
```

Expected: `accepting connections`

### Check port 5433 is reachable

```powershell
Test-NetConnection -ComputerName localhost -Port 5433
```

`TcpTestSucceeded` should be **True**.

---

## Step 5 — Pull the Ollama embedding model

The API uses the **`all-minilm`** model (384-dimension vectors). Pull it once:

```powershell
docker exec bookstore-ollama ollama pull all-minilm
```

Verify the model is available:

```powershell
docker exec bookstore-ollama ollama list
```

You should see `all-minilm` in the list.

### Test Ollama embeddings directly (optional)

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:11434/api/embed" `
  -ContentType "application/json" `
  -Body '{"model":"all-minilm","input":"books about wolf behavior"}'
```

---

## Step 6 — Run the API

```powershell
dotnet run --project src/BookStore.Api
```

On **first run**, the application will:

1. Apply EF Core migrations (creates `Books` table + pgvector extension + HNSW index).
2. Seed **30 books** (animals, business, coding topics).
3. Call Ollama once per book to generate embeddings.

> **Note:** First startup can take several minutes while seed data is embedded. Later starts are much faster.

### Default URLs

| URL | Purpose |
|-----|---------|
| `http://localhost:5031/swagger` | Swagger UI (HTTP) |
| `https://localhost:7170/swagger` | Swagger UI (HTTPS) |

Check the console output for the exact URLs when the app starts.

---

## Step 7 — Test the API

### Swagger UI

Open in your browser:

```
http://localhost:5031/swagger
```

### Semantic search

**GET** `/api/books/search?q={query}&limit=5`

Example queries to try:

- `wolf packs and wildlife behavior`
- `startup leadership and business strategy`
- `clean code and software design patterns`
- `ocean animals and marine conservation`

**cURL (PowerShell):**

```powershell
curl "http://localhost:5031/api/books/search?q=wolf%20packs%20and%20wildlife%20behavior&limit=5"
```

**Postman:**

| Field | Value |
|-------|-------|
| Method | `GET` |
| URL | `http://localhost:5031/api/books/search` |
| Query params | `q` = `wolf packs and wildlife behavior`, `limit` = `5` |

### Create a new book

**POST** `/api/books`

The API generates an embedding and stores the book automatically.

**cURL (PowerShell):**

```powershell
curl -X POST http://localhost:5031/api/books `
  -H "Content-Type: application/json" `
  -d '{"title":"Deep Learning for Wildlife","description":"Using neural networks to track endangered species from camera trap images.","author":"Jane Rivera"}'
```

**Postman:**

| Field | Value |
|-------|-------|
| Method | `POST` |
| URL | `http://localhost:5031/api/books` |
| Headers | `Content-Type: application/json` |
| Body (raw JSON) | see below |

```json
{
  "title": "Deep Learning for Wildlife",
  "description": "Using neural networks to track endangered species from camera trap images.",
  "author": "Jane Rivera"
}
```

---

## Configuration

Connection and Ollama settings are in [`src/BookStore.Api/appsettings.json`](src/BookStore.Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5433;Database=bookstore;Username=postgres;Password=postgres"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "EmbeddingModel": "all-minilm"
  }
}
```

| Setting | Value | Notes |
|---------|-------|-------|
| Postgres host | `localhost` | Docker maps container port 5432 → host **5433** |
| Postgres port | `5433` | Change only if you remap ports in `docker-compose.yml` |
| Database | `bookstore` | Created automatically by Docker Postgres image |
| Ollama endpoint | `http://localhost:11434` | Ollama REST API |
| Embedding model | `all-minilm` | 384-dimension vectors |

---

## Project structure

```
Book-store/
├── BookStore.sln
├── docker-compose.yml
├── README.md
└── src/
    └── BookStore.Api/
        ├── Program.cs
        ├── appsettings.json
        ├── Data/
        │   ├── BookStoreDbContext.cs
        │   └── BookSeeder.cs
        ├── Models/
        │   ├── Book.cs
        │   └── Dtos/
        ├── Services/
        │   ├── EmbeddingService.cs
        │   ├── BookSearchService.cs
        │   └── BookService.cs
        ├── Endpoints/
        │   └── BookEndpoints.cs
        └── Migrations/
```

---

## API reference

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/books/search?q={query}&limit=5` | Semantic search — returns books ranked by vector similarity |
| `POST` | `/api/books` | Create a book with auto-generated embedding |

**Search response** includes `similarity` (0–1, higher = more relevant).

**Create book request body:**

```json
{
  "title": "string (required)",
  "description": "string (required)",
  "author": "string (required)"
}
```

---

## Tech stack

- **ASP.NET Core 10** — Web API + Swagger
- **EF Core** + **Npgsql** — PostgreSQL data access
- **[Pgvector.EntityFrameworkCore](https://github.com/pgvector/pgvector-dotnet)** — `vector(384)` column + `CosineDistance` LINQ
- **[OllamaSharp](https://www.nuget.org/packages/OllamaSharp)** + **Microsoft.Extensions.AI** — embedding generation
- **Docker** — `pgvector/pgvector:pg16` + `ollama/ollama`

---

## Troubleshooting

### `failed to connect to the docker API at npipe:////./pipe/docker_engine`

**Cause:** Docker Desktop is not running.

**Fix:**
1. Open **Docker Desktop** from the Start menu.
2. Wait until it shows **running**.
3. Run `docker version` and confirm **Server** appears.
4. Retry `docker compose up -d`.

---

### `NpgsqlException: The operation has timed out`

**Cause:** The API cannot reach PostgreSQL on `localhost:5433`.

**Fix:**
1. Confirm Docker is running: `docker version`
2. Confirm containers are up: `docker compose ps`
3. Confirm port is open: `Test-NetConnection localhost -Port 5433`
4. Check Postgres logs: `docker compose logs postgres`
5. Restart containers: `docker compose down` then `docker compose up -d`
6. Wait 10–20 seconds for Postgres to finish starting, then run the API again.

---

### Port 5433 already in use

Edit `docker-compose.yml` and change the host port, for example:

```yaml
ports:
  - "5434:5432"
```

Then update `appsettings.json` to use `Port=5434`.

---

### Ollama model not found / embedding errors

Pull the model again:

```powershell
docker exec bookstore-ollama ollama pull all-minilm
docker exec bookstore-ollama ollama list
```

Test Ollama directly:

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:11434/api/embed" `
  -ContentType "application/json" `
  -Body '{"model":"all-minilm","input":"test"}'
```

---

### First startup is very slow

Expected behavior. The seeder embeds **30 books** via Ollama (one API call per book). Subsequent runs skip seeding if data already exists.

---

### WSL 2 issues on Windows

In PowerShell (as Administrator):

```powershell
wsl --status
wsl --update
```

In **Docker Desktop → Settings → General**, ensure **Use the WSL 2 based engine** is enabled.

---

## Stop and clean up

### Stop containers (keep data)

```powershell
docker compose stop
```

### Stop and remove containers (keep volumes/data)

```powershell
docker compose down
```

### Stop and remove everything including database data

```powershell
docker compose down -v
```

> **Warning:** `-v` deletes Postgres and Ollama model volumes. You will need to re-pull `all-minilm` and re-seed on next run.

---

## Useful Docker commands

```powershell
# View running containers
docker compose ps

# View logs (all services)
docker compose logs

# View Postgres logs only
docker compose logs postgres

# View Ollama logs only
docker compose logs ollama

# Restart a single service
docker compose restart postgres

# Open psql inside the Postgres container
docker exec -it bookstore-postgres psql -U postgres -d bookstore
```

Inside `psql`, useful commands:

```sql
-- List tables
\dt

-- Check pgvector extension
\dx

-- Count books
SELECT COUNT(*) FROM "Books";
```

---

## EF Core migrations (optional)

Migrations run automatically on startup. To run manually:

```powershell
cd src/BookStore.Api
dotnet ef database update
```

Add a new migration after model changes:

```powershell
dotnet ef migrations add MigrationName
```

---

## License

This project is provided as a demo for learning semantic search with .NET 10, PostgreSQL, and pgvector.
