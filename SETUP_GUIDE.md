# SETUP_GUIDE.md
_Date: 2025-08-25_

This guide describes how to set up the Villainous web app locally for development and testing.

---

## 1) Prerequisites

- **.NET 8 SDK** (https://dotnet.microsoft.com/download/dotnet/8.0)  
- **Node.js** v20.19+ or v22 LTS (https://nodejs.org)  
- **pnpm** package manager (recommended) → `npm install -g pnpm`  
- **Docker Desktop** (for running Seq, OpenTelemetry Collector, optional DB)  
- **Git** client  
- Editor: Visual Studio 2022 or JetBrains Rider (for backend), VS Code (frontend)  

---

## 2) Repository Structure

```
/apps/api       # ASP.NET Core 8 minimal API + SignalR
/apps/web       # React 19 + TS + Vite frontend
/packages/engine # Pure C# rules engine
/packages/model # Shared DTOs & schemas
/tests/         # Backend + frontend tests
/infra/compose  # docker-compose for observability
```

---

## 3) First Run

### Backend API
```bash
dotnet restore
dotnet build
dotnet run --project apps/api
```
- API starts at `http://localhost:5165`
- Swagger UI at `http://localhost:5165/swagger`

### Frontend Web
```bash
pnpm -C apps/web install
pnpm -C apps/web dev
```
- Web app runs at `http://localhost:5173` (default Vite port)

---

## 4) Observability Setup

### Run Seq (logs)
```bash
docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq
```
Access at `http://localhost:5341`.

### Run OpenTelemetry Collector (optional)
```bash
docker run -d --name otelcol -p 4317:4317 otel/opentelemetry-collector:latest
```

Configure exporters in `appsettings.json`.

---

## 5) Configuration Files

- `appsettings.json`  
  - Serilog config (console, file, seq sinks)  
  - OpenTelemetry exporter endpoints  
- `appsettings.Development.json`  
  - Lower log level (Debug)  
  - Dev-only overrides  

---

## 6) Developer Workflow

1. Pull latest changes → `git pull`  
2. Run tests → `dotnet test` and `pnpm -C apps/web test`  
3. Add/modify code (engine, api, or web)  
4. Format before commit:  
   ```bash
   dotnet format
   pnpm -C apps/web lint --fix
   pnpm -C apps/web format
   ```
5. Commit with [Conventional Commits](https://www.conventionalcommits.org/)  
6. Push and verify CI passes on GitHub  

---

## 7) Running Tests Locally

### Backend
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend
```bash
pnpm -C apps/web test -- --coverage
```

Coverage reports stored in `/coverage`.

---

## 8) CI Integration

- GitHub Actions runs backend & frontend tests on each PR.  
- Coverage thresholds enforced: Backend ≥85%, Frontend ≥80%.  
- Lint and formatting checks block merges.  

---

## 9) Troubleshooting

- **Port conflicts**: Change ports in `launchSettings.json` or Vite config.  
- **Docker not running**: Ensure Docker Desktop is started.  
- **Seq not reachable**: Verify `docker ps` shows container; restart if needed.  
- **SignalR reconnect issues**: Check browser console logs; confirm `/hub` endpoint available.  

---

## 10) Next Steps

- Review [agent.md](./agent.md) for the task board.  
- Review [villainous-architecture.md](./villainous-architecture.md) for design details.  
- Confirm test coverage and observability before extending gameplay features.  
