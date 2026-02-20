# bad-code-practice

Practice project for finding bad code and refactoring it into cleaner, faster code.

## Included challenge labs

### EF Core challenge
- Practice file: `BadCodePractice/Features/EfCoreChallenge/PracticeOrderReportService.cs`
- Route: `/ef-core-challenge`
- Focus: over-fetching, in-memory filtering, N+1 queries
- Metrics: SQL command count, elapsed time, output rows

### Caching challenge
- Practice file: `BadCodePractice/Features/CachingChallenge/PracticeCachingChallengeService.cs`
- Route: `/caching-challenge`
- Focus: unbounded cache growth, missing TTL/eviction, cache stampede
- Metrics: memory retained, hit ratio, response time

### Memory leak challenge
- Practice file: `BadCodePractice/Features/MemoryLeakChallenge/PracticeMemoryLeakService.cs`
- Route: `/memory-leak-challenge`
- Focus: static references, event subscriptions, timers, unbounded collections, closure capture
- Metrics: retained memory by cause and total retained memory

### Async misuse challenge
- Practice file: `BadCodePractice/Features/AsyncMisuseChallenge/PracticeAsyncMisuseService.cs`
- Route: `/async-misuse-challenge`
- Focus: sync-over-async, fire-and-forget, missing cancellation
- Metrics: elapsed time, processed count, approximate worker thread usage

### Concurrency challenge
- Practice file: `BadCodePractice/Features/ConcurrencyChallenge/PracticeConcurrencyService.cs`
- Route: `/concurrency-challenge`
- Focus: race conditions and shared mutable state
- Metrics: expected vs actual count, missed updates, elapsed time

### DI lifetime challenge
- Practice file: `BadCodePractice/Features/DiLifetimeChallenge/PracticeDiLifetimeService.cs`
- Route: `/di-lifetime-challenge`
- Focus: captive dependencies, scope/state leakage, disposable lifetime mistakes
- Metrics: unique scopes seen, leak indicator, memory growth, elapsed time

### Allocation challenge
- Practice file: `BadCodePractice/Features/AllocationChallenge/PracticeAllocationService.cs`
- Route: `/allocation-challenge`
- Focus: heavy allocations in hot paths, boxing, string churn, LINQ overhead
- Metrics: memory allocated (MB), Gen 0/1/2 collection counts, elapsed time

### Resiliency/Retry challenge
- Practice file: `BadCodePractice/Features/ResiliencyRetryChallenge/PracticeResiliencyRetryService.cs`
- Route: `/resiliency-retry-challenge`
- Focus: blind retries, missing timeout/jitter/backoff, no circuit breaker
- Metrics: success under fault injection, p95 tail latency, downstream call count

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

Open any challenge route from the list above using the local URL printed by `dotnet run`.

## Optional: run with SQLite instead of Docker

```bash
cd BadCodePractice
$env:Database__Provider="sqlite"
dotnet run
```

## If app port is already in use

```bash
dotnet run --urls "http://localhost:5101"
```
