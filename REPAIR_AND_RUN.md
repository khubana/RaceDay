# RaceDay setup and repair guide — existing database only

## Database rule

This solution uses only the six tables in the supplied Part 2 SQL file: `Roles`, `Users`, `Events`, `Categories`, `EventEnrolment`, and `Results`. It does not add, rename, or delete tables or columns. Do not run any previous `Part3_Upgrade.sql` file.

The EF Core model is mapped to the original SQL column lengths and nullability, including `Users.PasswordHash` as `NVARCHAR(MAX)`, `Users.Phone` as nullable `NVARCHAR(30)`, `Events.EventDescription` as nullable `NVARCHAR(MAX)`, and optional `DistanceKm`/`EventType`.

Run `RaceDay.Api/Database/VERIFY_EXISTING_SCHEMA.sql` if you want to inspect the current database. It is read-only.

## Connection

The default connection string is `(localdb)\api_guide`, database `RaceDay`. Check `RaceDay.Api/appsettings.json` and change only the server/instance if your database is hosted elsewhere.

## Register and login

1. Start `RaceDay.Api` and confirm Swagger loads at `https://localhost:7184/swagger`.
2. Start `RaceDay.Mvc`.
3. Register a new user with a new email address and choose Participant or Organiser.
4. Log in using the exact plain-text password entered at registration.
5. The API stores a password hash in `Users.PasswordHash`; the MVC password field must never be populated with that hash.

A database value such as `HASHED_PASSWORD_003` is a placeholder, not a valid ASP.NET Core PasswordHasher hash. It cannot be used as the login password or verified. If the old account's password hash is a placeholder or its original password is unknown, register a new account using another email.

## Build

```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln
dotnet test RaceDay.sln
```

## Known database limitation

The original six-table schema has no `Users.ProfilePictureUrl` and no `Events.BannerImageUrl`. This version therefore does not offer persistent profile/banner image uploads in MVC. It does not silently add those columns. Azure URL persistence and displaying those images require a schema change, which is intentionally outside this database-preserving package.
