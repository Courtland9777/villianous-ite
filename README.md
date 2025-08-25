# Villainous Web App

This repository hosts a deterministic Disney Villainous engine, ASP.NET Core API, and React frontend.

## Getting Started
- Follow [SETUP_GUIDE.md](./SETUP_GUIDE.md) to install prerequisites and run the services.

## Documentation
- [villainous-architecture.md](./villainous-architecture.md) — architecture & decisions
- [API_CONTRACT.md](./API_CONTRACT.md) — REST and SignalR endpoints
- [GAME_KNOWLEDGE.md](./GAME_KNOWLEDGE.md) — rules and card references
- [TEST_PLAN.md](./TEST_PLAN.md) — testing strategy and coverage goals

## Acceptance Criteria
- Two-player hot-seat matches playable end-to-end
- Deterministic replays with fixed seed
- Illegal actions return ProblemDetails with `traceId`
- Logs and traces available via Seq and OpenTelemetry
- Backend coverage ≥85%, frontend ≥80%

---
This project is licensed under the PolyForm Noncommercial License 1.0.0. See LICENSE for details.
