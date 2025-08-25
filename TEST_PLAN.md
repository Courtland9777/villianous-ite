# TEST_PLAN.md
_Date: 2025-08-25_

This plan covers testing for **engine**, **API**, and **web client**, including determinism, legality enforcement, and observability hooks.

---

## 1) Scope & Goals

- Verify **rules correctness** and **deterministic replays** (same seed + commands ⇒ same end state).
- Ensure **ProblemDetails** errors are returned for illegal actions with stable `code` and `traceId`.
- Validate **SignalR** real-time behavior (join, command, broadcast, reconnect).
- Maintain **defense-in-depth** via property tests, golden fixtures, and integration tests.
- Enforce **coverage thresholds**: Backend ≥85%, Frontend ≥80%.

---

## 2) Test Matrix

| Area | Category | Examples |
|---|---|---|
| Engine | Unit | vanquish legality, fate draw/choice, objective checks |
| Engine | Property | shuffle determinism, deck accounting, command idempotency |
| Engine | Golden | YAML-driven sequences → expected events & final hash |
| API | Integration | REST ProblemDetails mapping, idempotent commands, view redaction |
| API | Realtime | SignalR Join/SendCommand/StateUpdated, reconnect/queueing |
| Web | Component | prompts modal, action availability, error boundary |
| Web | Contract | client ↔ DTO shape conformance (mock server) |
| Web | E2E (optional) | basic flows with Playwright or Cypress |

---

## 3) Engine Tests (xUnit)

### 3.1 Unit Tests
- **Vanquish**: allies sum ≥ hero strength; discard semantics; modifiers respected.
- **Fate flow**: draw N, choose 1, apply effects; targeting constraints enforced.
- **Objectives**:
  - **Ursula**: Trident + Crown in Ursula’s Lair ⇒ win.
  - **Hook**: Peter Pan defeated at Jolly Roger ⇒ win (requires Never Land unlock).
  - **Maleficent**: Curse at each location ⇒ win; test breaking/removal.
  - **Prince John**: ≥20 Power **and** Robin Hood in The Jail ⇒ win (Intro to Evil variant).
- **State invariants**: deck sizes, non-negative power, no dangling card refs.

### 3.2 Property-Based Tests
- **Shuffle determinism**: given a `seed`, shuffles are stable; different seeds vary.
- **Replay determinism**: given `(seed, commands)`, final hash stable.
- **Idempotent command handling**: duplicate `{matchId, playerId, clientSeq}` ⇒ single effect.

### 3.3 Golden Tests
- **Fixtures**: `tests/engine/golden/*.yaml`
  - `seed: 12345`
  - `commands: [...]` (typed)
  - `expect: { finalHash: "…" , events: [...] }`
- **Harness**: loader executes sequences, asserts event stream & `finalHash`.

---

## 4) API Tests (Integration)

### 4.1 REST
- **Create match**: `POST /api/matches` returns `matchId`, `seed`, snapshot.
- **Get state**: redaction of hidden info (opponent hand/fate deck).
- **Get replay**: event list includes seq numbers and correlation.
- **Illegal actions**: 400/409/500 with **ProblemDetails** `{ type, title, status, detail, instance, code, traceId }`.

### 4.2 Idempotency
- Duplicate command by same `{matchId, playerId, clientSeq}` processed once; second call returns 200 no-op or 409, depending on contract.

### 4.3 ProblemDetails Mapping
- Domain validation ⇒ `rules.invalid_target` (400).
- Illegal action ⇒ `rules.illegal_action` (400).
- Revision conflict ⇒ `rules.conflict` (409).
- Invariant breach ⇒ `engine.invariant_violation` (500).
- Assert `traceId` present; log correlation matches.

### 4.4 Health & Observability
- `/healthz/live` and `/ready` return 200.
- Logs contain `matchId`, `playerId`, `traceId` when available.

---

## 5) Realtime (SignalR) Tests

- **JoinMatch** emits `MatchJoined` with snapshot; connection added to group.
- **SendCommand** yields `StateUpdated` events in order.
- **CommandRejected** fires with `{ code, message, traceId }` for illegal commands.
- **Reconnect**: connection drop → queued commands sent after reconnect; no duplication.
- **Backpressure**: slow client does not miss state-critical events (deltas or full snapshot resent as needed).

> Implementation: use `TestServer` + in-memory SignalR clients or `WebApplicationFactory` with `HubConnection` to run headless.

---

## 6) Web Tests (Vitest + React Testing Library)

### 6.1 Components
- **Prompts modal**: respects `min/max` selections; disables continue until valid.
- **Action sites**: enabled/disabled by legality; tooltips show reason when disabled.
- **Error boundary**: renders ProblemDetails; copy-to-clipboard includes `traceId`.
- **Board**: renders locations, heroes, allies; updates on `StateUpdated`.

### 6.2 Contract Tests
- Decode DTOs with Zod; fail on shape drift.
- Ensure redaction rules enforced (no hidden-info leakage into UI).

### 6.3 Accessibility
- Modal focus trap, escape to close (when allowed), ARIA labels/roles.
- Keyboard navigation for selecting targets in prompts.

---

## 7) E2E (Optional but Recommended)

- Tool: **Playwright** or **Cypress** in CI (containers).
- Scenarios:
  - Create match → play basic turn → fate opponent → vanquish hero → win condition fires.
  - Network glitch simulation: disconnect/reconnect while prompts active.

---

## 8) Fixtures & Utilities

- **Seeds**: maintain a small corpus of known seeds that exercise edge cases.
- **Card catalogs**: JSON fixtures for villain decks; validate loading against schema.
- **Hashing**: final state hash via stable JSON canonicalization to compare replays.

---

## 9) Coverage & Quality Gates

- **Backend**: Coverlet; fail under **85%** line coverage.  
  - Enforce per-project thresholds (`engine` higher, e.g., 90%).  
- **Frontend**: Vitest coverage **≥80%**; components/features with business logic ≥85%.  
- **Mutation testing** (optional): Stryker.NET / StrykerJS for critical reducers/effects.

---

## 10) CI Integration

- **GitHub Actions**:  
  - Job `backend`: `dotnet restore/build/test`, publish coverage (`coverlet.collector`).  
  - Job `frontend`: `pnpm i`, `pnpm lint`, `pnpm test -- --coverage`.  
  - Artifacts: coverage reports, junit XML (if configured).  
- **Fail fast** on coverage or lint errors.  
- **Matrix** (optional): Node 20/22; Windows/Linux runners for .NET.

---

## 11) Non-Functional Tests

- **Performance smoke**: simple loop of commands over a match; assert P95 latency bound.  
- **Memory**: long-running match with many events; assert no unbounded growth.  
- **Logging**: structured fields present; no sensitive hidden-info logged.  

---

## 12) Test Data Management

- Keep golden fixtures small and human-readable.  
- Tag fixtures by villain and mechanic (e.g., `ursula-contracts.yaml`).  
- Store seeds and hashes in a manifest for quick health checks.

---

## 13) Exit Criteria

- All suites green (engine, API, web).  
- Coverage thresholds met or exceeded.  
- Determinism verified on corpus seeds.  
- No P0/P1 open bugs related to rules legality, determinism, or hidden-info leakage.

---
