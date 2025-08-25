# Villainous Web App — Architecture & Decision Record
_Date: 2025-08-25_

## 1. Overview
This document defines the **architecture, libraries, and design decisions** for the Villainous web app.

- **Backend**: .NET 8, C# 12, ASP.NET Core Minimal APIs + SignalR  
- **Frontend**: React 19.1.0, TypeScript 5.6.2, Vite 7  
- **Engine**: Pure C# rules engine (deterministic, seeded RNG)  
- **Logging/Tracing**: Serilog 9.0, OpenTelemetry 1.12, Seq sink 8.0  
- **Testing**: xUnit 2.9.3, Coverlet 6.0.4, Vitest 2.1.0  
- **Observability**: structured logs, metrics, traces  

---

## 2. Engine
- Immutable `GameState` and `PlayerState` records  
- `ICommand` → reducers → `DomainEvents`  
- Deterministic RNG (`IRandom`) injected per match  
- Invariants enforced (deck counts, power ≥0, no dangling refs)  
- Replay system applies command log with same seed  

---

## 3. API
- **REST Endpoints**  
  - POST `/api/matches` — create match  
  - GET `/api/matches/{id}/state` — current snapshot (redacted for caller)  
  - GET `/api/matches/{id}/replay` — full event log  
  - POST `/api/matches/{id}/commands` — submit commands  
- **SignalR Hub**  
  - `JoinMatch` — snapshot  
  - `SendCommand` — events or rejection  
  - `LeaveMatch` — disconnect  
- **Error Handling**  
  - RFC 9457 ProblemDetails for REST  
  - `CommandRejected` event for SignalR  
- **Idempotency**  
  - Commands deduped via `{matchId, playerId, clientSeq}`  

---

## 4. Frontend
- Built with Vite + React + TS strict mode  
- State: Zustand slices (`matchView`, `prompts`, `connection`)  
- Data fetching: React Query for REST, SignalR for realtime  
- UI:  
  - Render villain boards and locations  
  - Action spots enabled/disabled by state  
  - Prompt modal for target/fate choices  
- Error boundary displays ProblemDetails with `traceId`  

---

## 5. Observability
- **Serilog** sinks: Console (JSON), File (rolling), Seq  
- **OTEL**: traces + metrics, with `traceId` propagated to ProblemDetails  
- **Enrichment**: MatchId, PlayerId, CorrelationId  

---

## 6. Testing
- **Engine**: xUnit unit tests, property tests, golden YAML fixtures  
- **API**: Integration tests with WebApplicationFactory, in-memory SignalR clients  
- **Frontend**: Vitest component tests, MSW for API mocking  
- **Coverage thresholds**: Backend ≥85%, Frontend ≥80%  

---

## 7. Package Pins (excerpt)

**Directory.Packages.props**
```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <PackageVersion Include="Serilog.Sinks.Seq" Version="8.0.0" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="9.0.3" />
    <PackageVersion Include="Asp.Versioning.Http" Version="8.1.0" />
    <PackageVersion Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.8" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>
</Project>
```

**apps/web/package.json (excerpt)**
```json
{
  "dependencies": {
    "react": "19.1.0",
    "react-dom": "19.1.0",
    "react-router-dom": "7.8.2",
    "@tanstack/react-query": "5.85.5",
    "@microsoft/signalr": "9.0.6",
    "zustand": "5.0.8",
    "zod": "3.23.8"
  },
  "devDependencies": {
    "typescript": "5.6.2",
    "vite": "7.0.3",
    "vitest": "2.1.0",
    "eslint": "9.34.0",
    "@typescript-eslint/eslint-plugin": "8.40.0",
    "@typescript-eslint/parser": "8.40.0",
    "prettier": "3.6.2"
  }
}
```

---

## 8. Acceptance Criteria
- Two-player hot-seat matches playable end-to-end  
- Deterministic replays with fixed seed  
- Errors returned as ProblemDetails with `code` and `traceId`  
- Logs/traces visible via Seq + OTEL  
- Test coverage thresholds enforced  
