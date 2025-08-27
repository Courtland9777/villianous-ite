# TASKS

## Test Status (2025-08-25)
- Backend line coverage: 72% (`dotnet test --collect:"XPlat Code Coverage"`)
- Frontend tests: failed – missing @vitest/coverage-v8
- Lint: passed (`pnpm -C apps/web lint`)
- Type check: passed (`pnpm -C apps/web exec tsc --noEmit`)

## Repo
- [x] ✅ Keep PolyForm Noncommercial license file
  _Rationale_: honor licensing terms
  _Acceptance Criteria_: `LICENSE` present.
- [x] ✅ Add Directory.Packages.props for NuGet version pinning
  _Rationale_: ensure reproducible builds
  _Acceptance Criteria_: centralized props file pins all package versions.
- [x] ✅ Pin web `package.json` dependency versions
  _Rationale_: avoid unintended upgrades
  _Acceptance Criteria_: remove range specifiers (`^`, `~`) in `apps/web/package.json`.

## Engine
- [x] ✅ Model GameState & PlayerState as immutable records
  _Rationale_: safe state transitions
  _Acceptance Criteria_: records with init-only properties in `packages/engine`.
- [x] ✅ Provide ICommand interface and domain events
  _Rationale_: enable reducer pattern
  _Acceptance Criteria_: commands produce `DomainEvent` instances.
- [x] ✅ Implement core commands (Vanquish, Fate, CheckObjective)
  _Rationale_: support basic gameplay
  _Acceptance Criteria_: command handlers and unit tests cover these actions.
- [x] ✅ Enforce deterministic engine with seeded RNG and replay parity tests
  _Rationale_: allow reproducible matches
  _Acceptance Criteria_: RNG injectable; replay tests confirm identical state for same seed+commands.
- [x] ✅ Guard state invariants (deck counts, non‑negative power, no dangling refs)
  _Rationale_: prevent illegal game states
  _Acceptance Criteria_: invariant checks with failing tests on violation.

## API
- [x] ✅ Expose REST endpoints for matches (create, state, replay, commands)
  _Rationale_: serve game state over HTTP
  _Acceptance Criteria_: endpoints respond per `API_CONTRACT.md`.
- [x] ✅ Expand SignalR hub with reconnect handling and group broadcasts
  _Rationale_: keep clients in sync during disconnects
  _Acceptance Criteria_: tests cover reconnect and broadcast semantics.
- [x] ✅ Return ProblemDetails with `code` and `traceId` for all REST errors
  _Rationale_: standardized error diagnostics
  _Acceptance Criteria_: RFC 9457 responses include `code` & `traceId`.
- [x] ✅ Emit `CommandRejected` with `code` and `traceId` on SignalR errors
  _Rationale_: mirror REST error shape
  _Acceptance Criteria_: hub sends structured rejection messages.
- [x] ✅ Redact hidden information from GameState DTOs
  _Rationale_: prevent opponent info leaks
  _Acceptance Criteria_: opponent hand and fate deck counts only.
- [x] ✅ Support idempotent commands via `{matchId, playerId, clientSeq}`
  _Rationale_: avoid duplicate effects
  _Acceptance Criteria_: duplicate submissions are ignored or rejected.
- [x] ✅ Provide `/healthz/live` and `/ready` endpoints
  _Rationale_: enable k8s probes
  _Acceptance Criteria_: endpoints return 200 with health checks.

## Web
- [x] ✅ Set up routing, stores, data fetching, and SignalR client
  _Rationale_: enable navigation and realtime play
  _Acceptance Criteria_: router, Zustand stores, React Query, SignalR connection exist.
- [x] ✅ Build core UI (realms, action spots, hand view, prompt modals)
  _Rationale_: allow players to take actions
  _Acceptance Criteria_: components render and respond to state.
- [x] ✅ Handle REST/SignalR errors with ProblemDetails and traceId display
  _Rationale_: help users report issues
  _Acceptance Criteria_: error boundary shows title, code, and traceId.
- [ ] ⛔ Add accessibility basics (focus traps, ARIA roles, keyboard nav)
  _Rationale_: usable by keyboard‑only players
  _Acceptance Criteria_: prompts trap focus and provide ARIA labels.
- [ ] ⛔ Implement SignalR reconnect logic on transient network loss
  _Rationale_: keep sessions alive
  _Acceptance Criteria_: client retries and rejoins matches automatically.

## Observability
- [x] ✅ Configure Serilog with console, file, and Seq sinks
  _Rationale_: collect structured logs
  _Acceptance Criteria_: appsettings configure all sinks.
- [x] ✅ Enable OpenTelemetry tracing and metrics exporters
  _Rationale_: support tracing backends
  _Acceptance Criteria_: OTEL configured with Otlp exporter.
- [x] ✅ Propagate `traceId` into logs and ProblemDetails
  _Rationale_: correlate errors with traces
  _Acceptance Criteria_: `traceId` present in log context and error payloads.
- [ ] ⛔ Enrich logs with `matchId` and `playerId`, omitting hidden info
  _Rationale_: maintain observability without leaks
  _Acceptance Criteria_: structured logging tested for redaction.

## Testing
- [ ] ⛔ Achieve backend coverage ≥85% (current 72%)
  _Rationale_: catch regressions early
  _Acceptance Criteria_: coverage reports show ≥85% line coverage.
- [ ] ⛔ Fix frontend tests and reach coverage ≥80% (missing @vitest/coverage-v8)
  _Rationale_: ensure UI reliability
  _Acceptance Criteria_: vitest run succeeds with ≥80% line coverage.
- [ ] ⛔ Add property and golden fixture tests for engine determinism
  _Rationale_: verify replay parity
  _Acceptance Criteria_: fixture corpus with hash checks committed.
- [ ] ⛔ Test ProblemDetails mapping and SignalR `CommandRejected` events
  _Rationale_: guarantee error contracts
  _Acceptance Criteria_: integration tests cover error paths.

## CI/CD
- [x] ✅ Run dotnet build/test and web lint/test in GitHub Actions
  _Rationale_: basic CI automation
  _Acceptance Criteria_: `.github/workflows/ci.yml` executes build and tests.
- [ ] ⛔ Enforce coverage thresholds and formatting in CI
  _Rationale_: block low quality changes
  _Acceptance Criteria_: workflow fails under set thresholds or formatting issues.
- [ ] ⛔ Add Prettier and `dotnet format` checks to CI
  _Rationale_: maintain consistent style
  _Acceptance Criteria_: workflow steps verify formatting.

## Docs
- [ ] ⛔ Update SETUP_GUIDE with verified local ports and observability URLs
  _Rationale_: keep onboarding accurate
  _Acceptance Criteria_: guide reflects working defaults.

## Security
- [ ] ⛔ Restrict CORS to allowlisted origins in development
  _Rationale_: prevent unsolicited web access
  _Acceptance Criteria_: CORS policy configured with explicit origins.
- [ ] ⛔ Add basic rate limiting to API and SignalR
  _Rationale_: mitigate abusive clients
  _Acceptance Criteria_: exceeding limits returns 429.
- [ ] ⛔ Validate and sanitize all input DTOs
  _Rationale_: defend against malformed data
  _Acceptance Criteria_: model binding rejects invalid payloads.

## Open Risks / Follow-ups
- Engine currently lacks deterministic RNG and invariants enforcement.
- Frontend tests cannot run until coverage plugin is installed.
