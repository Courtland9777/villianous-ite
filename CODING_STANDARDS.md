# CODING_STANDARDS.md
_Date: 2025-08-25_

This document defines **coding conventions** for the Villainous web app across backend (C#) and frontend (TypeScript/React).

---

## General Principles
- Consistency is more important than personal preference.  
- Favor readability and testability over micro-optimizations.  
- Keep functions/classes small and focused (SRP).  
- Avoid duplication; refactor into shared helpers.  
- Always add tests when fixing bugs or adding features.  

---

## C# Standards

### Language & Version
- Target **.NET 8** and **C# 12**.  
- Enable `nullable` reference types.  
- Use `file-scoped namespaces`.  

### Project Structure
- `packages/engine` contains pure domain logic (no ASP.NET deps).  
- `apps/api` contains API controllers/hubs and infrastructure.  
- Shared DTOs in `packages/model`.  

### Style
- **Naming**  
  - Classes/Interfaces: `PascalCase`  
  - Methods: `PascalCase`  
  - Properties: `PascalCase`  
  - Parameters & locals: `camelCase`  
  - Constants: `PascalCase`  
  - Async methods suffix with `Async`.  

- **Records & DTOs**  
  - Use `record` for immutable data structures.  
  - Use `init` for set-once properties.  

- **Control Flow**  
  - Prefer pattern matching over `is` + cast.  
  - Prefer `switch` expressions for branching.  
  - Avoid deeply nested `if` statements; refactor early returns.  

- **Error Handling**  
  - Throw domain-specific exceptions only in the engine.  
  - API layer maps exceptions to **ProblemDetails**.  

- **Logging**  
  - Use `ILogger<T>` or Serilog directly.  
  - Never log sensitive/hidden game state (hands, fate deck).  
  - Include `traceId`, `matchId`, and `playerId` in logs when available.  

---

## TypeScript Standards

### Language & Version
- TypeScript **5.6 strict mode**.  
- Enable `strictNullChecks`, `exactOptionalPropertyTypes`, `noImplicitAny`.  

### Project Structure
- `apps/web/src/api` — network layer (REST + SignalR).  
- `apps/web/src/stores` — Zustand stores.  
- `apps/web/src/features` — feature-specific components.  
- `apps/web/src/components` — reusable UI.  

### Style
- **Naming**  
  - Files: `kebab-case.ts[x]`  
  - React components: `PascalCase`  
  - Variables/functions: `camelCase`  
  - Zustand stores: `*.store.ts`  

- **Components**  
  - Always use function components + hooks.  
  - Use `React.FC<Props>` only when typing children.  
  - No default exports; always named exports.  

- **State Management**  
  - Keep Zustand stores minimal.  
  - Use React Query for server data.  
  - Never duplicate server state locally.  

- **Error Handling**  
  - Validate API responses with Zod.  
  - Use centralized error boundary + toast notifications.  

- **Styling**  
  - Use TailwindCSS for utility classes.  
  - Shared styles via `@apply` in `.css` when needed.  

---

## Git & Workflow

- **Commits**: Follow [Conventional Commits](https://www.conventionalcommits.org/). Examples:  
  - `feat(engine): add vanquish command`  
  - `fix(api): correct ProblemDetails mapping`  
  - `test(web): add prompt modal unit tests`  

- **Pull Requests**  
  - Must include tests and documentation updates.  
  - CI must pass before merge.  

---

## Testing Conventions

- Backend: xUnit tests use Arrange–Act–Assert.  
- Frontend: Vitest with RTL; prefer `screen.getByRole` queries.  
- Use Moq for backend dependencies.  
- Always test error paths (ProblemDetails, CommandRejected).  

---

## Linting & Formatting

- Backend:  
  - Enable analyzers (`EnableNETAnalyzers=true`, `AnalysisMode=AllEnabledByDefault`).  
  - Run `dotnet format` before commit.  

- Frontend:  
  - ESLint flat config + Prettier 3.x.  
  - Run `pnpm lint` and `pnpm format` in CI.  
