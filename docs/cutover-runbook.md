# BBScoreboard Cutover Runbook

Last updated: 2026-04-14

## Purpose

Execution guide for production migration/cutover tasks in checklist section `13`.

## 1) Snapshot Current Production DB

SQL Server:

```bash
sqlcmd -S <server> -U <user> -P '<password>' -Q "BACKUP DATABASE [BBScoreboard] TO DISK = N'<backup-path>/BBScoreboard-full.bak' WITH INIT, COMPRESSION"
```

PostgreSQL:

```bash
pg_dump -h <host> -U <user> -d <db> -Fc -f <backup-path>/bbscoreboard-$(date +%Y%m%d-%H%M%S).dump
```

## 2) Dry-Run SQL Server -> PostgreSQL Migration (Staging)

1. Restore latest SQL Server snapshot to staging source.
2. Prepare empty staging PostgreSQL target with latest migrations.
3. Execute ETL script or migration utility in dry-run mode.
4. Capture migration logs and row counts.

## 3) Verify Row Counts and Key Aggregates

Check at minimum:

- `UCSeason`
- `UCTeam`
- `UCPlayer`
- `UCGame`
- `UCGameTeamStat`
- `UCGamePlayerStat`
- `UCGameplayAction`
- `AspNetUsers` / `AspNetRoles` / `AspNetUserRoles`

Record totals and aggregate deltas to `docs/validation-evidence/<date>/cutover-counts.md`.

## 4) Parity Tests on Migrated Dataset

Run:

```bash
dotnet test src/BBScoreboard.slnx -c Release
```

Then execute manual parity flow from:

- `docs/parity-qa-runbook.md`

## 5) Downtime and Rollback Plan

Downtime checklist:

1. Announce maintenance window.
2. Disable score-entry mutations.
3. Capture final source snapshot.
4. Run migration.
5. Smoke-test health + login + manager + stats.
6. Enable traffic.

Rollback checklist:

1. Re-point app to previous database.
2. Restore latest pre-cutover backup if needed.
3. Re-enable traffic.
4. Publish incident summary.

## 6) Cutover Validation Checklist

- `/health` returns `200`.
- Admin login succeeds.
- Demo setup succeeds.
- Live game start/action flow succeeds.
- Stats page and print preview render.
- Mutation API endpoints return expected codes.
- Logs show no critical errors in first 30 minutes.
