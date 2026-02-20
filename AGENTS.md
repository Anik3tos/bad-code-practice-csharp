# AGENTS.md

Guidelines for AI coding agents working on this codebase.

## Project Overview

Bad Code Practice is a Blazor Server app for learning to identify and fix performance anti-patterns in .NET. Each challenge presents intentionally bad code that users refactor into clean, performant implementations.

## Architecture

```
BadCodePractice/
├── Components/Pages/       # Blazor pages (one per challenge)
├── Features/               # Challenge implementations by feature
│   └── <ChallengeName>/
│       ├── I<Name>Service.cs           # Interface
│       ├── Bad<Name>Service.cs         # Intentionally bad implementation
│       ├── Practice<Name>Service.cs    # User practice file (editable)
│       ├── Refactored<Name>Service.cs  # Reference solution
│       └── <Name>ChallengeRunner.cs    # Orchestrates challenge execution
├── Data/                   # EF Core entities, DbContext, seeding
└── Infrastructure/         # Cross-cutting concerns (metrics, interceptors)
```

## Challenge Pattern

Every challenge follows the same triad pattern:

| File | Purpose |
|------|---------|
| `Bad*Service.cs` | Demonstrates anti-patterns - **DO NOT FIX** |
| `Practice*Service.cs` | User workspace - implement fixes here |
| `Refactored*Service.cs` | Reference solution for comparison |

The `*ChallengeRunner.cs` orchestrates execution and collects metrics.

## Code Conventions

- **Framework**: .NET 10, Blazor Server
- **DI**: All services registered in `Program.cs`
- **Interfaces**: Prefix with `I`, placed alongside implementations
- **Namespaces**: `BadCodePractice.Features.<ChallengeName>`
- **Async**: Suffix async methods with `Async`
- **Cancellation**: Accept `CancellationToken` in long-running methods

## Adding a New Challenge

1. Create `Features/<Name>Challenge/` folder
2. Implement:
   - `I<Name>Service.cs` - interface with `ExecuteAsync()` method
   - `Bad<Name>Service.cs` - contains anti-patterns
   - `Practice<Name>Service.cs` - TODO stubs for user
   - `Refactored<Name>Service.cs` - clean solution
   - `<Name>ChallengeRunner.cs` - runs all three, collects metrics
3. Create Blazor page in `Components/Pages/<Name>Challenge.razor`
4. Register services in `Program.cs`
5. Add navigation link in `NavMenu.razor`
6. Update this file and `README.md`

## Current Challenges

| Challenge | Focus Area | Route |
|-----------|------------|-------|
| EF Core | N+1 queries, over-fetching | `/ef-core-challenge` |
| Caching | Unbounded growth, stampede | `/caching-challenge` |
| Memory Leak | Static refs, events, timers | `/memory-leak-challenge` |
| Async Misuse | Sync-over-async, fire-forget | `/async-misuse-challenge` |
| Concurrency | Race conditions, shared state | `/concurrency-challenge` |
| DI Lifetime | Captive deps, scope leakage | `/di-lifetime-challenge` |
| Resiliency/Retry | Retry storms, no backoff | `/resiliency-retry-challenge` |
| Allocation | Boxing, string concat, LOH | `/allocation-challenge` |

## Important Rules

1. **Never "fix" Bad* files** - they exist to demonstrate problems
2. **Practice files are user-editable** - keep them as TODO stubs
3. **AI Refactoredfiles show best practices** - maintain high quality
4. **Metrics matter** - each challenge measures specific performance characteristics
5. **Database**: Uses Postgres (Docker) by default, SQLite as fallback

## Testing Changes

```bash
docker compose up -d          # Start Postgres
cd BadCodePractice
dotnet run                    # Launch app
# Navigate to challenge route to verify
```

## File Naming

- Services: `<Type><Challenge>Service.cs` (e.g., `BadCachingChallengeService.cs`)
- Runner: `<Challenge>ChallengeRunner.cs`
- Contracts: `<Challenge>Contracts.cs` or `<Challenge>Workload.cs`
- Page: `<Challenge>.razor` (e.g., `CachingChallenge.razor`)
