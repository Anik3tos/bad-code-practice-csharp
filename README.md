# Bad Code Practice

Hands-on labs for learning to identify and fix .NET performance anti-patterns. Each challenge presents intentionally flawed code with measurable metrics—your task is to refactor it and see real improvements.

## Features

- **12 challenge labs** covering common .NET performance pitfalls
- **Side-by-side comparison** of Bad, Practice, and AI Refactored implementations
- **Real-time metrics** to measure your improvements
- **Interactive Blazor UI** for running challenges in browser

## Quick Start

```bash
# Start database (Postgres)
docker compose up -d

# Run the app
cd BadCodePractice
dotnet run
```

Open the displayed localhost URL and navigate to any challenge.

### Alternative: SQLite (no Docker)

```bash
cd BadCodePractice
$env:Database__Provider="sqlite"
dotnet run
```

### Port already in use?

```bash
dotnet run --urls "http://localhost:5101"
```

## Challenge Labs

| Challenge | Focus Areas | Route |
|-----------|-------------|-------|
| **EF Core** | N+1 queries, over-fetching, in-memory filtering | `/ef-core-challenge` |
| **Caching** | Unbounded growth, missing TTL, cache stampede | `/caching-challenge` |
| **Memory Leak** | Static refs, event subscriptions, timers, closures | `/memory-leak-challenge` |
| **Async Misuse** | Sync-over-async, fire-and-forget, missing cancellation | `/async-misuse-challenge` |
| **Concurrency** | Race conditions, shared mutable state | `/concurrency-challenge` |
| **DI Lifetime** | Captive dependencies, scope leakage, disposable mistakes | `/di-lifetime-challenge` |
| **Allocation** | Boxing, string churn, LINQ overhead, LOH pressure | `/allocation-challenge` |
| **Resiliency/Retry** | Blind retries, missing backoff/jitter, no circuit breaker | `/resiliency-retry-challenge` |
| **Exception Handling** | Swallowed exceptions, `throw ex`, missing context/correlation | `/exception-handling-challenge` |
| **Serialization** | Reflection hot paths, sync JSON, missing source generators, over-serialization | `/serialization-challenge` |
| **Logging** | PII leakage, noisy strings, missing correlation IDs | `/logging-challenge` |
| **Regex** | Inline compilation, substring allocations, missing source generators | `/regex-challenge` |

## How Challenges Work

Each challenge follows a consistent pattern:

| File | Purpose |
|------|---------|
| `Bad*Service.cs` | Intentionally bad implementation—**don't modify** |
| `Practice*Service.cs` | Your workspace—implement fixes here |
| `Refactored*Service.cs` | Reference solution for comparison |

1. Open the practice file for your chosen challenge
2. Identify and fix the anti-patterns
3. Run the challenge in the UI to see metrics
4. Compare your results against Bad and AI Refactored baselines

## Project Structure

```
BadCodePractice/
├── Components/Pages/       # Blazor pages (one per challenge)
├── Features/               # Challenge implementations
│   └── <ChallengeName>/
│       ├── I<Name>Service.cs           # Interface
│       ├── Bad<Name>Service.cs         # Anti-patterns
│       ├── Practice<Name>Service.cs    # Your workspace
│       ├── Refactored<Name>Service.cs  # Solution
│       └── <Name>ChallengeRunner.cs    # Orchestrator
├── Data/                   # EF Core entities, DbContext, seeding
└── Infrastructure/         # Query metrics, interceptors
```

## Requirements

- .NET 10 SDK
- Docker (optional, for Postgres)
- PostgreSQL on port 5433 (or use SQLite)

## For AI Agents

See [AGENTS.md](./AGENTS.md) for guidelines on working with this codebase programmatically.

## License

MIT
