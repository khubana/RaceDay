# Verification status and assignment checklist

## Included in this solution

- One Visual Studio solution containing `RaceDay.Api`, `RaceDay.Mvc`, and `RaceDay.Tests`.
- MVC registration/login/logout with role stored in MVC session and API session cookie forwarded for protected API calls. The API now registers a real session authentication scheme so protected routes can challenge with HTTP 401 and reject wrong roles with HTTP 403.
- Public event listing and details; event name/location, type and date filters.
- Organiser dashboard, event create/edit/delete, category add/delete, enrolment listing, and result entry.
- Participant event details/category enrolment, My Events/cancellation, My Results with finish position and total finishers, and profile update/picture upload.
- Azure Blob upload endpoints in the API; MVC sends uploads to the API only.
- MVC Dockerfile, README, SQL upgrade script, and GitHub Actions workflow that restores/builds the solution, explicitly builds MVC, and runs API smoke tests.

## Not automatically completed by source code

- Azure account/storage connection string must be supplied by the student in API user-secrets/environment variables.
- SQL upgrade must be executed against the correct existing RaceDay database.
- The student must push at least 20 meaningful commits to their own GitHub repository. A ZIP cannot create those repository commits.
- The student must record and publish an unlisted YouTube demonstration and put its real URL in the submission README/repository.
- The student must run the solution locally and verify it against their actual Part 2 database.

## Build verification

The preparation environment does not contain the .NET SDK, so `dotnet restore`, `dotnet build`, `dotnet test`, Swagger runtime, Azure uploads, and Docker runtime could not be executed here. The ZIP archive integrity was checked, but that is not the same as a successful application build.

Run in Visual Studio 2022 / .NET 8:

```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.sln --configuration Release
```

Start `RaceDay.Api` and `RaceDay.Mvc` together. Verify `https://localhost:7184/swagger` first, then register/login with both roles and test the full workflow.
