namespace BadCodePractice.Features.CachingChallenge;

public interface ICachingChallengeService
{
    string Name { get; }
    Task<CachingServiceTelemetry> RunAsync(CachingChallengeOptions options, CancellationToken cancellationToken = default);
}

public sealed record CachingChallengeOptions(
    int TotalRequests,
    int ConcurrentWorkers,
    int KeySpace,
    int PayloadKilobytes);

public sealed record CachingServiceTelemetry(
    int TotalRequests,
    int Hits,
    int Misses,
    int BackendCalls,
    double ElapsedMilliseconds,
    int CacheEntries);

public sealed record CachingRunResult(
    string Label,
    long RetainedBytes,
    double HitRatioPercent,
    double ElapsedMilliseconds,
    int BackendCalls,
    int CacheEntries,
    int Hits,
    int Misses);

public sealed record CachingComparisonResult(
    CachingRunResult Bad,
    CachingRunResult Practice,
    CachingRunResult Refactored);
