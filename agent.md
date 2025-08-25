# agent.md — Villainous Web App (Codex Guide)
_Date: 2025-08-25_

This is the **primary guide** for Codex agents and human developers to stand up and maintain the Villainous web app.

## Linked Docs
- [villainous-architecture.md](./villainous-architecture.md)
- [GAME_KNOWLEDGE.md](./GAME_KNOWLEDGE.md)
- [API_CONTRACT.md](./API_CONTRACT.md)
- [CODING_STANDARDS.md](./CODING_STANDARDS.md)
- [TEST_PLAN.md](./TEST_PLAN.md)
- [SETUP_GUIDE.md](./SETUP_GUIDE.md)

---

## Global Goals
- Deterministic game engine with replays  
- Clear separation: **Engine (C#)**, **API (ASP.NET Core)**, **Web (React/TS)**  
- Proper observability with logs, traces, metrics  
- Comprehensive test coverage  
- Simple, repeatable setup for developers and Codex agent automation  

---

## Task Checklist

### 0. Bootstrap Repo
- [ ] Initialize Git repo  
   ```bash
   git init
   dotnet new gitignore
   ```
- [ ] Add `.gitignore`, `README.md`, and `LICENSE` (MIT recommended)  
- [ ] Create solution:  
   ```bash
   dotnet new sln -n Villainous
   ```
- [ ] Add projects:  
   ```bash
   dotnet new classlib -n Engine -o packages/engine
   dotnet new classlib -n Model -o packages/model
   dotnet new web -n Api -o apps/api
   dotnet sln add packages/engine packages/model apps/api
   ```
- [ ] Scaffold Vite React + TS under `apps/web`:  
   ```bash
   pnpm create vite@latest apps/web -- --template react-ts
   ```

### 1. Backend (API & Engine)
- [ ] Implement `GameState` and `PlayerState` as immutable records  
- [ ] Define `ICommand` interface; reducers return `DomainEvents`  
- [ ] Implement core actions:  
  - [ ] Vanquish (one Hero, Allies ≥ Hero strength)  
  - [ ] Fate pipeline (targeting, reveals)  
  - [ ] Objective checks per villain  
- [ ] Configure REST endpoints:  
  - POST `/api/matches` — create match  
  - GET `/api/matches/{id}/state` — current view  
  - GET `/api/matches/{id}/replay` — replay log  
  - POST `/api/matches/{id}/commands` — submit commands  
- [ ] Configure SignalR hub with: `JoinMatch`, `SendCommand`, state broadcast events  
- [ ] Error handling:  
  - Use RFC 9457 ProblemDetails for REST  
  - `CommandRejected` for SignalR  
- [ ] Configure Serilog (Console, File, Seq), OpenTelemetry, and health endpoints (`/healthz/live`, `/ready`)  

### 2. Frontend
- [ ] Install dependencies:  
   ```bash
   pnpm add react-router-dom @tanstack/react-query zustand @microsoft/signalr zod react-hook-form @hookform/resolvers
   ```
- [ ] Set up routing, state management (Zustand), data fetching (React Query), SignalR  
- [ ] Build UI: villain realms, action spots, hand view, prompt modals  
- [ ] Error handling with ProblemDetails + traceId + accessibility  

### 3. Testing
- [ ] Engine: xUnit + property tests + golden fixtures  
- [ ] API: integration + SignalR + ProblemDetails mapping tests  
- [ ] Frontend: Vitest + React Testing Library + MSW  
- [ ] Coverage goals: Backend ≥85%, Frontend ≥80%  

### 4. CI/CD
- [ ] GitHub Actions: build, test, coverage, lint/format checks  

---

## Acceptance Criteria
- Playable 2P hot‐seat game with deterministic replays  
- ProblemDetails on illegal actions  
- Logs/traces visible via Seq + OTEL  
- Coverage thresholds met  

