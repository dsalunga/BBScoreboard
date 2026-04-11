# BBScoreboard

![BBScoreboard banner](BBScoreboard.WebApp/assets/img/bbscoreboardbanner.png)

BBScoreboard is an ASP.NET Web Pages app for running basketball tournament scoreboards, logging in-game actions, and generating team/player stats.

## Features

- Live scoreboard with quarter tracking and optional game timer
- Play-by-play logging (FG/3PT/FT, rebounds, fouls, assists, steals, blocks, turnovers)
- Team and player stats views (overall and per quarter)
- Admin screens for seasons, games, teams, players, referees, users, and app settings
- Role-based access (`Administrator`, `Statistician`, `Read-Only`)
- AJAX sync endpoint via `GameplaySync.asmx`

## Tech Stack

- C# / .NET Framework 4.7.1
- ASP.NET Web Pages (Razor)
- Entity Framework 6 (database-first EDMX)
- SQL Server
- Bootstrap 3 + jQuery

## Repository Structure

- `BBScoreboard.WebApp/` - website project (pages, UI, web service, startup)
- `BBScoreboard/` - core library (entities, helpers, gameplay logic)
- `BBScoreboard.Database/` - SQL Server database project (base schema)
- `BBScoreboard.WebApp/App_Data/Updates.sql` - required schema updates for newer fields
- `IISExpress/` - IIS Express config used by included run script

## Prerequisites

- Windows (recommended)
- Visual Studio 2017+ with:
  - ASP.NET and web development workload
  - .NET Framework 4.7.1 targeting pack
  - SQL Server Data Tools (for `.sqlproj` publish)
- SQL Server instance (LocalDB or full SQL Server)
- NuGet package restore tooling

## Local Setup

1. Clone the repository.
2. Open `BBScoreboard.sln` in Visual Studio.
3. Restore NuGet packages for the solution.
4. Create database `BBScoreboard` using one of these options:
   - Preferred: publish `BBScoreboard.Database/BBScoreboard.Database.sqlproj`.
   - Alternative: execute SQL scripts in `BBScoreboard.Database/dbo/Tables/` on an empty DB.
5. Run `BBScoreboard.WebApp/App_Data/Updates.sql` against the same DB.
   - This adds required fields/tables such as `UCPlayer.Active`, `UCTeam.Active`, `UCGame.IsEnded`, and `AppConfig`.
6. Verify/update `BBScoreboard.WebApp/Web.config`:
   - `ConnectionString`
   - `BBScoreboardEntities`
   - `DefaultUserEmail`
   - `DefaultPassword`
   - `AppName`
7. Run the app:
   - Visual Studio IIS Express profile: default URL in solution is `http://localhost:52054`
   - Repo script: run `IISExpress - BBScoreboard x86.cmd`, which serves `http://localhost:7777`

## First Login

On startup, `_AppStart.cshtml` checks `UserProfile`. If empty, it creates a bootstrap admin account from `Web.config`:

- `DefaultUserEmail` (default: `admin@someorg.org`)
- `DefaultPassword` (default: `L3tmein`)

Change the bootstrap password immediately after first login.

## Typical Usage Flow

1. Configure teams and players (`Manage > Teams`).
2. Configure seasons and games (`Manage > Seasons`).
3. Run live scoring (`Gameplay > Scoreboard`).
4. Review statistics (`Gameplay > Game Stats`).
5. Adjust timer/action display behavior (`Manage > Settings`).

## Troubleshooting

- SQL errors like `Invalid column name 'Active'` or `'IsEnded'`: run `BBScoreboard.WebApp/App_Data/Updates.sql`.
- Login or membership table issues in fresh DBs: confirm the app can connect using `ConnectionString`, then restart app so `_AppStart.cshtml` can initialize membership tables.
- Blank pages or startup issues in IIS Express: verify target framework packs are installed and NuGet restore completed successfully.

## License

MIT
