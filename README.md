# bad-code-practice

Practice project for finding bad code and refactoring it into cleaner, faster code.

## EF Core challenge included

The app ships with one complete scenario based on common EF Core mistakes.

1. Open `BadCodePractice/Features/EfCoreChallenge/BadOrderReportService.cs`.
2. Identify the anti-patterns (over-fetching, in-memory filtering, N+1 queries).
3. Refactor the method.
4. Run the app and compare metrics on `/ef-core-challenge`.

The challenge page shows:

- SQL command count
- elapsed time
- output rows

## Run with Docker DB (Postgres)

From repo root:

```bash
docker compose up -d
```

This starts Postgres on `localhost:5433`.

Then run the app:

```bash
cd BadCodePractice
dotnet restore
dotnet run
```

Then open `https://localhost:xxxx/ef-core-challenge` (port shown in console).

## Optional: run with SQLite instead of Docker

```bash
cd BadCodePractice
$env:Database__Provider="sqlite"
dotnet run
```

## If app port is already in use

If you see an error like `Failed to bind to address ... address already in use`, run with another URL:

```bash
dotnet run --urls "http://localhost:5101"
```
