# RaceDay – Combined Part 2 and Part 3 Solution

this repository contains one Visual Studio solution for the RaceDay platform. The API remains a separate ASP.NET Core project and the MVC frontend consumes it over HTTP; the MVC project has no direct database access.

## Solution structure

- `RaceDay.Api/` – Part 2 REST API, EF Core SQL Server access, session authentication, and optional Azure Blob Storage upload endpoint examples.
- `RaceDay.Api/RaceDay.Tests/` – Part 2 API test project.
- `RaceDay.Mvc/` – Part 3 ASP.NET Core MVC frontend, API client, role-specific pages, Bootstrap styling, and Dockerfile.
- `.github/workflows/ci.yml` – restores and builds the complete solution and runs API tests.

## Open in Visual Studio

Open **`RaceDay.sln`** from this folder. It contains the API, API tests, and MVC projects.

## Configure and run locally

1. Use your existing Part 2 `RaceDay` database. Do not run a schema upgrade or migration. You can run `RaceDay.Api/Database/VERIFY_EXISTING_SCHEMA.sql` to inspect the schema; it is read-only.
2. Set the SQL Server connection string in `RaceDay.Api/appsettings.json` under `ConnectionStrings:RaceDayConnection`.
3. Set `AzureBlobStorage:ConnectionString` in `RaceDay.Api/appsettings.json` (or use user secrets/environment variables). Do not commit real credentials.
4. Start the API first: `dotnet run --project RaceDay.Api/RaceDay.Api.csproj`. The supplied launch profile listens on `http://localhost:5184` and `https://localhost:7184`.
5. Start MVC in another terminal: `dotnet run --project RaceDay.Mvc/RaceDay.Mvc.csproj`. MVC's `ApiSettings:BaseUrl` is set to `http://localhost:5184/` in `appsettings.json`.

## Build and test

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
dotnet test RaceDay.Api/RaceDay.Tests/RaceDay.Tests.csproj
```

## Docker – MVC frontend

Build from the MVC project directory:

```bash
docker build -t raceday-mvc .
```

Run it while the API is reachable from the container. On Docker Desktop, use `host.docker.internal` to reach an API running on the host:

```bash
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5184/ raceday-mvc
```

Open `http://localhost:8080`. The API and SQL Server must be running separately and reachable. The MVC container does not connect to SQL Server or Azure directly; all data and file operations go through the API.

## GitHub and video submission

Push the combined source to your Part 3 branch. Make at least 20 meaningful commits as you implement, test, and document the work. Add your unlisted YouTube demonstration URL to this README before submitting on ARC.

### Visual Studio startup

In Solution Explorer, right-click the solution → **Configure Startup Projects** → **Multiple startup projects**. Set `RaceDay.Api` and `RaceDay.Mvc` to **Start** (leave `RaceDay.Tests` as None). Start the solution. If Visual Studio prompts about the HTTPS development certificate, trust the ASP.NET Core development certificate.

## Repair and setup guide

See [REPAIR_AND_RUN.md](REPAIR_AND_RUN.md) for the Swagger fix, database role seeding, build commands, and run instructions.


## Submission tasks still required

Configure your own SQL Server and Azure Blob Storage credentials, run the solution and verify it locally, create at least 20 meaningful commits in your GitHub repository, and record an unlisted YouTube demonstration. See `VERIFICATION_STATUS.md` and `REPAIR_AND_RUN.md`.


## Important: existing database only

This version is mapped to the six tables and columns in the supplied Part 2 `Database/RaceDay.sql`: `Roles`, `Users`, `Events`, `Categories`, `EventEnrolment`, and `Results`. It does not run migrations or add database columns. Run `RaceDay.Api/Database/VERIFY_EXISTING_SCHEMA.sql` only to inspect the current database; it is read-only.

The supplied schema has no `ProfilePictureUrl` or `BannerImageUrl` columns. Therefore the MVC application does not claim to save image URLs in SQL Server. Azure upload URL persistence/display requires a database column, and has deliberately been disabled rather than altering your database.

For login, enter the original plain-text password used at registration. Never enter the `PasswordHash` value from SQL Server into the password box. If a row contains a placeholder such as `HASHED_PASSWORD_003`, it is not a valid ASP.NET Core password hash and cannot be verified; register a fresh account using a new email address.
