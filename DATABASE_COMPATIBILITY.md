# Database compatibility

This solution targets the six tables in the supplied Part 1 RaceDay SQL script:
`Roles`, `Users`, `Events`, `Categories`, `EventEnrolment`, and `Results`.

The EF Core mappings use the actual Part 1 column names and SQL types. Important details:

- `Users.Email` is `varchar(100)`, `Users.PasswordHash` is `varchar(100)`, and `Users.Phone` is required `varchar(20)`.
- `Events.EventName`, `EventDescription`, and `EventLocation` are `varchar(100)`; `EventDate` is SQL `date`.
- `Categories.EntryFee` is `decimal(10,2)`.
- The Part 1 script spells the enrolment foreign-key column `partcipantsID`. The C# property remains `ParticipantsID`, and EF maps it to that exact existing SQL column.
- No database tables or columns are added. No migrations or schema upgrade scripts are required for these changes.

Use the original Part 1 `RaceDay` database and its six existing tables. The read-only
`RaceDay.Api/Database/VERIFY_EXISTING_SCHEMA.sql` script can be used to inspect the
live database without changing it.

Existing sample users whose `PasswordHash` values are literal placeholders such as
`HASHED_PASSWORD_003` do not have a verifiable password. Register a new account through
the MVC Register page, then log in using the plain-text password entered during registration.
