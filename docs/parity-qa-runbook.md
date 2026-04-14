# BBScoreboard Parity QA Runbook

Last updated: 2026-04-14

## Purpose

Manual and semi-automated parity validation checklist for:

- route coverage by role
- manage/admin command behavior
- full live-game flow
- baseline output comparisons

This runbook backs checklist section `12.2 Parity testing`.

## 1) Preconditions

- App is running on PostgreSQL (`Database__Provider=postgres`).
- Bootstrap admin is configured and can sign in.
- At least one scorer and one read-only test user exist.
- DB starts from a clean test dataset.

## 2) Route Matrix by Role

Validate each route with roles: `Anonymous`, `ReadOnly`, `Scorer`, `Admin`.

Expected outcomes:

- `Anonymous` can access: `/Login`, `/Account/Register`, `/Account/ForgotPassword`, `/Account/PasswordReset`, `/Account/Confirm`.
- `ReadOnly` can access: `/`, `/Gameplay/Games`, `/Gameplay/Stats`.
- `ReadOnly` is blocked from: `/Gameplay/Manager`, `/Manage/*`, `/Admin/Setup`, mutation APIs.
- `Scorer` can access gameplay manager/stat pages and mutation APIs.
- `Scorer` is blocked from admin/manage pages.
- `Admin` can access all pages.

Core routes:

- `/`
- `/Login`
- `/Account/Register`
- `/Account/ForgotPassword`
- `/Account/PasswordReset`
- `/Account/Confirm`
- `/Account/Manage`
- `/Gameplay/Games?view=Manager`
- `/Gameplay/Games?view=Stats`
- `/Gameplay/Manager?id=<gameId>`
- `/Gameplay/Stats?id=<gameId>`
- `/Manage/Seasons`
- `/Manage/SeasonEntry`
- `/Manage/Games?seasonId=<seasonId>`
- `/Manage/GameEntry?seasonId=<seasonId>`
- `/Manage/Teams`
- `/Manage/TeamEntry`
- `/Manage/Players?teamId=<teamId>`
- `/Manage/PlayerEntry?teamId=<teamId>`
- `/Manage/Users`
- `/Manage/UserEntry`
- `/Manage/UserPassword`
- `/Manage/Referees`
- `/Admin/Setup`

## 3) Manage/Admin Command Validation

Run each command and verify DB state + UI response:

- Seasons: create/update/delete season.
- Teams: create/update/delete team.
- Players: create/update/delete player.
- Games: create/update/delete game (validation should block same-team game and duplicate game number per season).
- Admin setup:
  - toggle `EnableTimer`
  - toggle `ShowAllActions`
  - run `Create Demo Game Setup`.

## 4) Full Live-Game Flow

1. Sign in as Admin.
2. Open `/Admin/Setup`, create demo setup.
3. Open Manager.
4. Select exactly five starters for each team and start game.
5. Record actions: make/miss shots, rebounds, assists, fouls, turnovers.
6. Verify:
   - score updates per quarter and total
   - player stat rows update
   - play-by-play records appear.
7. Edit a play-by-play action:
   - click action row
   - update timestamp
   - undo/redo action
   - confirm reflected score/stat changes.
8. Open game options:
   - change quarter
   - update team scores
   - update game clock time.
9. Open Stats page:
   - validate totals
   - run print preview.

## 5) Baseline Output Comparison

For one representative game:

- export manager summaries and stats table from legacy baseline archive
- export current app outputs
- compare:
  - final score
  - team per-quarter points
  - player PTS/REB/AST/STL/BLK/TO/PF/TF totals
  - play-by-play count and ordering by action time
  - locked/unlocked behavior.

## 6) Evidence Capture

Store validation evidence under:

- `docs/validation-evidence/<YYYY-MM-DD>/`

Include:

- screenshots
- API response captures for mutation/delta endpoints
- SQL row-count checks for core tables
- test command outputs.
