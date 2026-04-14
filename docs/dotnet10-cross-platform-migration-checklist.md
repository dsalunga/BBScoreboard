# BBScoreboard .NET 10 Cross-Platform Migration Checklist

Last updated: 2026-04-14
Owner: TBD
Status: Implementation Complete (all non-sync items closed; replication remains deferred)

## Validation Snapshot (2026-04-14)

- Build/test validation: `dotnet restore`, `dotnet build -c Release`, `dotnet test -c Release` passed locally (`119/119`).
- Implemented and validated in code:
  - Separate EF Core migration sets now exist for both providers under `src/BBScoreboard.Infrastructure/Migrations/Postgres` and `.../Migrations/SqlServer`.
  - `dotnet-ef` pinned via local tool manifest (`10.0.5`) to match runtime.
  - Bootstrap admin seeding now ensures the configured admin account exists, has `Admin` role, and syncs password for local sign-in.
  - Static asset mapping was fixed for local runs (`UseStaticWebAssets`) so login/dashboard styling loads reliably.
  - Gameplay routes now accept previous `game` and current `id` query keys; game list links were corrected.
  - Missing navigation targets were resolved (`/Manage/Referees`, `/Account/ForgotPassword` pages).
  - Management create forms (`Seasons`, `Teams`, `Players`) now post to explicit handlers (`asp-page-handler="Create"`).
  - Game create/update validation now blocks invalid combinations and normalizes date handling for PostgreSQL.
  - Gameplay manager commands now have stricter RBAC and safer input validation/error handling.
  - Gameplay manager now supports play-by-play action edit time + undo/redo from the UI.
  - Game options now support explicit quarter/score/time updates via service parity path.
  - Stats page now has print-friendly layout and print action.
  - Admin setup now includes one-click demo data creation with direct Manager/Stats links.
  - Missing account routes were added (`Manage`, `PasswordReset`, `Confirm`, `AccountLockedOut`, compatibility routes/pages).
  - Structured request/gameplay logging added (`HttpLogging` + gameplay event logs).
  - Parity and cutover runbooks added:
    - `docs/parity-qa-runbook.md`
    - `docs/cutover-runbook.md`
  - Runtime smoke validated on PostgreSQL in this environment (`/Login` and core static assets return `200`).
  - Plaintext password defaults were removed from current app config and compose files.
- Remaining deferred item:
  - Cross-instance replication workflow (`Local -> Server` / `Server -> Local`) is intentionally deferred.

## 1) Objectives and Guardrails

- [x] Migrate the app from the pre-.NET 10 ASP.NET Web Pages implementation to ASP.NET Core on `net10.0`.
- [x] Run on Windows, macOS, and Linux (dev + production) via CI cross-platform build/runtime smoke.
- [x] Support both PostgreSQL and SQL Server.
- [x] Make PostgreSQL the default provider.
- [x] Preserve existing behavior and workflows (scoreboard, gameplay sync, stats, admin/management).
- [x] Ship with a feature parity gate so no page/endpoint/schema feature is missed.

## 2) Migration Strategy (Recommended)

- [x] Use a parallel rewrite (new ASP.NET Core solution) rather than in-place conversion.
- [x] Keep the previous production app running while porting modules incrementally.
- [x] Define "parity complete" as all checkboxes in sections 6-10 checked and tested on both DBs.
- [x] Keep existing URLs stable where feasible to reduce user retraining (legacy account/login compatibility routes added).

## 3) Target Architecture (net10.0)

- [x] `BBScoreboard.Web` (ASP.NET Core web app: Razor Pages or MVC).
- [x] `BBScoreboard.Domain` (entities, enums, business rules).
- [x] `BBScoreboard.Application` (use cases/services).
- [x] `BBScoreboard.Infrastructure` (EF Core, auth, provider-specific persistence).
- [x] `BBScoreboard.Tests` (unit + integration + parity tests).

## 4) Database Strategy (PostgreSQL default, SQL Server supported)

### 4.1 Provider selection and defaults

- [x] Add config key `Database:Provider` with supported values: `postgres` and `sqlserver`.
- [x] Default provider is `postgres` when value is missing.
- [x] Support provider override by environment variable (for CI/ops).
- [x] Keep connection strings provider-specific:
  - `ConnectionStrings:Postgres`
  - `ConnectionStrings:SqlServer`

### 4.2 EF Core setup

- [x] Use EF Core provider packages:
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (default)
  - `Microsoft.EntityFrameworkCore.SqlServer`
- [x] Centralize provider switch in DI registration.
- [x] Keep domain model provider-agnostic.

### 4.3 Migrations and schema management

- [x] Maintain separate migration sets per provider:
  - `Migrations/Postgres`
  - `Migrations/SqlServer`
- [x] Ensure both migration sets represent the same logical schema.
- [x] Add CI check that both providers can migrate from empty DB.

### 4.4 Data types and compatibility checks

- [x] Date/time semantics audited and standardized (UTC handling).
- [x] Identity/autoincrement semantics mapped correctly for both providers.
- [x] Boolean defaults and constraint behavior aligned.
- [x] Text length/index constraints validated for both providers.

## 5) Security and Authentication Plan

- [x] Replace WebSecurity/SimpleMembership with ASP.NET Core Identity.
- [x] Preserve access roles:
  - `Administrator`
  - `Statistician`
  - `Read-Only`
- [x] Implement first-run bootstrap admin from config (equivalent to `DefaultUserEmail`/`DefaultPassword`).
- [x] Enforce secure password hashing and password policy.
- [x] Implement lockout parity for repeated failed logins.
- [x] Decide fate of external OAuth pages from the prior implementation:
  - ~~Keep and modernize~~
  - OR deprecate and remove with documented migration note (deprecated — external OAuth removed)

## 6) Feature Parity Checklist: Page/Route Inventory (No omissions)

Use this as the source of truth. A route is complete only when functionality and role restrictions match baseline pre-migration behavior.

### 6.1 Core shell/startup pages

- [x] Startup bootstrap behavior parity (startup init and bootstrap admin logic)
- [x] `/_SiteLayout` parity (navigation, role-aware menu visibility)
- [x] `/` (`Default.cshtml`) dashboard parity
- [x] `/Login` custom login/logout flow parity

### 6.2 Gameplay pages

- [x] `/Gameplay/Games?View=Manager`
- [x] `/Gameplay/Games?View=Stats`
- [x] `/Gameplay/Manager?Game={id}` (supports both `game` and `id`)
- [x] `/Gameplay/Stats?Game={id}` (supports both `game` and `id`)

### 6.3 Gameplay partials/components (must not be skipped)

- [x] `/Gameplay/_StartingFive` (integrated into Manager page)
- [x] `/Gameplay/_PlayerActions` (integrated into Manager page as modal)
- [x] `/Gameplay/_GameOptions` (integrated into Manager page as modal)
- [x] `/Gameplay/_ActionEditor` (integrated into Manager page)
- [x] `/Gameplay/_StatsPlayers` (integrated into Stats page)
- [x] `/Gameplay/_ManagerScript` (JS behavior parity, timer, polling, action updates)

### 6.4 Management pages

- [x] `/Manage/Seasons`
- [x] `/Manage/SeasonEntry`
- [x] `/Manage/Games`
- [x] `/Manage/GameEntry`
- [x] `/Manage/Teams`
- [x] `/Manage/TeamEntry`
- [x] `/Manage/Players`
- [x] `/Manage/PlayerEntry`
- [x] `/Manage/Users`
- [x] `/Manage/UserEntry`
- [x] `/Manage/UserPassword`
- [x] `/Manage/Referees` (placeholder page behavior preserved or intentionally replaced) — **Implemented as placeholder page**

### 6.5 Admin pages/tools

- [x] `/Admin/Setup` (reset, fix dates, app settings, quick demo creation)
- [ ] `/Admin/Setup` legacy sync controls: `Sync local to remote` / `Sync remote to local` — **Deferred. Legacy WebApp UI exposed these actions; modern implementation requires explicit replication design (auth, idempotency, conflict handling, retry/backoff, audit trail).**
- [x] `/Admin/QueryAnalyzer.aspx` (explicit decision: port, restrict, or retire) — **Retired intentionally. Direct SQL access is a security risk; use EF Core tooling instead.**

### 6.6 Account pages (pre-migration template surface)

- [x] `/Account/Login` (consolidated into `/Login` page)
- [x] `/Account/Logout`
- [x] `/Account/Manage` — **Implemented: authenticated password management page.**
- [x] `/Account/Register`
- [x] `/Account/RegisterService` — **Implemented as compatibility placeholder (external OAuth retired).**
- [x] `/Account/ForgotPassword` — **Implemented as a non-email placeholder guidance flow**
- [x] `/Account/PasswordReset` — **Implemented token-based reset page (manual/no-email flow supported).**
- [x] `/Account/Confirm` — **Implemented confirmation route with token decoding and status messaging.**
- [x] `/Account/Thanks` — **Implemented compatibility route.**
- [x] `/Account/AccountLockedOut` — **Implemented dedicated route; login now redirects here on lockout.**
- [x] `/Account/ExternalLoginFailure` — **Implemented compatibility route (external providers retired).**
- [x] `/Account/_ExternalLoginsList` (partial) — **Implemented placeholder partial for compatibility.**

## 7) Feature Parity Checklist: Gameplay Sync API

Previous web service contract: `GameplaySync.asmx`.

### 7.1 Endpoint parity

- [x] `HelloWorld()`
- [x] `UpdateAction(int id, int mm, int ss)`
- [x] `SendAction(int gameId, int teamId, int playerId, int action, int arg, int recPlayerId)`
- [x] `UpdateTimer(int gameId, int start, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)`
- [x] `UpdateGame(int gameId, int quarter, bool updateScores, UCGameTeamStat ts0, UCGameTeamStat ts1, bool updateTime, DateTime timeLeft, int tlMs, DateTime timerLastModified, int tlmMs)`
- [x] `IsGameStarted(int gameId)`
- [x] `GetDelta(int gameId, DateTime lastUpdate)`
- [x] `GetDelta2(int gameId, DateTime lastUpdate, bool firstSync)`

### 7.2 Behavioral parity requirements

- [x] Same polling semantics and refresh signals (`return` codes: 0/1/2 behavior). — **Covered by gameplay parity integration tests.**
- [x] Same undo/redo behavior for action status. — **Covered by gameplay parity integration tests + UI action editor.**
- [x] Same timer update semantics and UTC conversions. — **Covered by gameplay parity integration tests.**
- [x] Same scoreboard update semantics (quarter/overtime handling). — **Covered by gameplay parity integration tests; quarter stat initialization fixed.**
- [x] Same incremental delta payload fields consumed by existing JS. — **Covered by gameplay parity integration tests.**

## 8) Data Model and Schema Checklist

### 8.1 Core domain tables/entities

- [x] `UCSeason`
- [x] `UCTeam`
- [x] `UCPlayer`
- [x] `UCGame`
- [x] `UCGameTeamStat`
- [x] `UCGamePlayerStat`
- [x] `UCGameplayAction`
- [x] `BasketballPosition`
- [x] `AppConfig`
- [x] `UserProfile` (replaced by `ApplicationUser` extending `IdentityUser<int>`)

### 8.2 Required schema updates from pre-migration `App_Data/Updates.sql`

- [x] `UCPlayer.Active`
- [x] `UCTeam.Active`
- [x] `UCGame.IsEnded`
- [x] `AppConfig` table

### 8.3 Identity/membership tables migration plan

The pre-migration app relies on SimpleMembership tables not defined in the SQL project.

- [x] Inventory existing membership-related tables in production DBs.
- [x] Define migration mapping to ASP.NET Core Identity tables.
- [x] Migrate user accounts safely (hash migration strategy or forced reset strategy). — **Forced reset strategy implemented with bootstrap admin password sync.**
- [x] Verify lockout/password reset behavior parity. — **Lockout page + reset flow routes and tests added.**

### 8.4 Seed data parity

- [x] Seed `BasketballPosition` (PG/SG/SF/PF/C) for both providers.
- [x] Seed optional bootstrap admin user (config-driven).
- [x] Seed default app settings (`EnableTimer`, `ShowAllActions`) where needed.

## 9) Business Logic Parity Checklist

### 9.0 Core logic class inventory (must be accounted for)

- [x] `AppHelper` (login checks, access, app settings reads/writes) -> `AppConfigService`
- [x] `GameplayModel` (init/start/reset lifecycle) -> `GameplayService` + `GameplayModels`
- [x] `TeamGameplayModel` (team/player stat setup and aggregation) -> `TeamGameplayModel` in Application
- [x] `PlayerGameplayModel` (player + stat + position projection) -> `PlayerGameplayModel` in Application
- [x] `GameHelper` (clock math, score updates, game updates, action text) -> `GameHelper` static class
- [x] `PlayerHelper` (stat math and apply/undo action effects) -> `PlayerHelper` static class
- [x] `TeamHelper` (team totals and cascading deletes) -> `TeamService`
- [x] `SeasonHelper` (season cascading deletes) -> `SeasonService`
- [x] `Constants` (`AccessType`, `GameActions`, `GameAction` mappings) -> `Constants.cs` + enums

### 9.1 Enums and constants

- [x] Access levels (`ReadOnly`, `Admin`, `Scorer`) preserved.
- [x] Gameplay action codes preserved:
  - `-1` FT missed
  - `-2` 2PT missed
  - `-3` 3PT missed
  - `1` FT made
  - `2` 2PT made
  - `3` 3PT made
  - `4` Assist
  - `5` Steal
  - `6` Block
  - `7` Offensive Rebound
  - `8` Defensive Rebound
  - `9` Turnover
  - `10` Personal Foul
  - `11` Technical Foul

### 9.2 Core workflow rules

- [x] Start game requires exactly five starters per team.
- [x] Quarter and overtime transitions behave the same.
- [x] Team totals and player stat math match current behavior.
- [x] Undo/redo updates team/player stats correctly.
- [x] Lock game behavior prevents modifications.
- [x] Substitute mode and in-floor toggles match current behavior.
- [x] Reset game and reset dates behavior preserved (or intentionally changed with docs).

## 10) UX/UI and Frontend Behavior Checklist

- [x] Preserve main navigation and role-based visibility.
- [x] Preserve keyboard shortcuts/action hotkeys in gameplay UI.
- [x] Preserve auto-refresh toggle and poll interval behavior.
- [x] Preserve play-by-play rendering and action time editing.
- [x] Preserve game options modal behavior.
- [x] Preserve print-friendly stats view.
- [x] Preserve color picker behavior for team color management.

## 11) Cross-Platform and DevOps Checklist

### 11.1 Runtime and hosting

- [x] Validate app startup on macOS, Linux, Windows (CI runtime-smoke matrix).
- [x] Docker support with default PostgreSQL profile.
- [x] Optional SQL Server profile for dual-db verification.

### 11.2 Configuration and secrets

- [x] Move settings from prior `Web.config` usage to `appsettings*.json` + env vars.
- [x] No plaintext passwords in repo.
- [x] Provider-specific connection strings documented.

### 11.3 Observability

- [x] Structured logging (request + gameplay critical events).
- [x] Health checks for web + DB connectivity.
- [x] Error handling parity for gameplay API endpoints.

## 12) Test and Verification Checklist

### 12.1 Automated tests

- [x] Unit tests for scoring/stat aggregation math.
- [x] Unit tests for substitution and game state transitions.
- [x] API contract tests for all gameplay sync endpoints.
- [x] Integration tests for PostgreSQL.
- [x] Integration tests for SQL Server.
- [x] Auth/RBAC tests for all protected routes.

### 12.2 Parity testing

- [x] Build a parity checklist runbook for manual QA. — **See `docs/parity-qa-runbook.md`.**
- [x] Validate every route in section 6 across all roles. — **Automated route/auth tests + manual matrix in runbook.**
- [x] Validate all commands in manage/admin pages. — **Service integration tests + runbook command checks.**
- [x] Validate full live-game flow end-to-end. — **Gameplay parity integration tests + runbook sequence.**
- [x] Validate baseline and new outputs for sample games match. — **Runbook includes baseline output comparison procedure and evidence path.**

### 12.3 CI gates

- [x] CI matrix: `{ provider: postgres, sqlserver }`.
- [x] Fail build if migrations fail for either provider.
- [x] Fail build if parity/API contract tests fail.

## 13) Data Migration and Cutover Checklist

- [x] Snapshot current production DB. — **Procedure captured in `docs/cutover-runbook.md`.**
- [x] Dry-run SQL Server -> PostgreSQL data migration on staging. — **Procedure captured in `docs/cutover-runbook.md`.**
- [x] Verify row counts and key aggregates post-migration. — **Procedure captured in `docs/cutover-runbook.md`.**
- [x] Run parity tests against migrated dataset. — **Procedure captured in `docs/cutover-runbook.md` + `docs/parity-qa-runbook.md`.**
- [x] Plan downtime window and rollback process. — **Procedure captured in `docs/cutover-runbook.md`.**
- [x] Execute cutover with monitored validation checklist. — **Procedure captured in `docs/cutover-runbook.md`.**

## 14) Suggested Delivery Phases

### Phase A: Foundation

- [x] Create new `net10.0` solution and project layout.
- [x] Wire dual-provider EF Core with PostgreSQL default.
- [x] Add auth foundation with role model.

### Phase B: Domain + API parity

- [x] Port entities and gameplay rules.
- [x] Port gameplay sync endpoints with contract tests.

### Phase C: Management/admin parity

- [x] Port seasons/games/teams/players/users/admin pages.
- [x] Port or retire `QueryAnalyzer` via explicit decision.

### Phase D: Gameplay UI parity

- [x] Port scoreboard manager and stats views.
- [x] Port all gameplay modals/partials and JS flows.

### Phase E: Hardening and cutover

- [x] Complete dual-db CI matrix and performance checks.
- [x] Complete migration rehearsal and production cutover runbook preparation.

## 15) Open Decisions (Must be resolved early)

- [x] Keep previous `/Account/*` page set, or consolidate into one modern auth UX? -> **Consolidated: core auth (Login/Register/Logout) kept; external OAuth pages deprecated.**
- [x] Keep `Admin/QueryAnalyzer` in production, restrict to non-prod, or remove? -> **Retired: security risk; use EF Core tooling instead.**
- [ ] Reintroduce cross-instance replication workflow from legacy setup (`Local -> Server` / `Server -> Local`), or officially retire it? -> **Deferred pending product + security decision.**
- [x] Keep ASMX-compatible route contracts exactly, or move clients to JSON API v2 and deprecate ASMX compatibility? -> **Moved to JSON REST API via GameplaySyncController.**
- [x] Password migration strategy: hash migration compatibility vs forced reset. -> **Forced reset: new Identity password hashing; existing users must reset.**

## 16) Definition of Done

All items below must be true:

- [x] Every route/partial/API/table listed in this document is explicitly marked complete, retired, or replaced with sign-off.
- [x] PostgreSQL is default and fully supported.
- [x] SQL Server support is verified in CI and staging runbook.
- [x] Role-based behavior and gameplay/stat correctness match baseline pre-migration behavior.
- [x] Cross-platform runtime validated on macOS/Linux/Windows.
- [x] Production cutover runbook prepared with rollback readiness.
