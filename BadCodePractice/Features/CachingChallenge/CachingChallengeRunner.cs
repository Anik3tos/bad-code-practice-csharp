namespace BadCodePractice.Features.CachingChallenge;

public sealed class CachingChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<CachingComparisonResult> RunAsync(
        int totalRequests,
        int concurrentWorkers,
        int keySpace,
        int payloadKilobytes,
        CancellationToken cancellationToken = default)
    {
        var options = NormalizeOptions(totalRequests, concurrentWorkers, keySpace, payloadKilobytes);

        var bad = await RunScenarioAsync<BadCachingChallengeService>(options, cancellationToken);
        var practice = await RunScenarioAsync<PracticeCachingChallengeService>(options, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredCachingChallengeService>(options, cancellationToken);

        return new CachingComparisonResult(bad, practice, refactored);
    }

    private async Task<CachingRunResult> RunScenarioAsync<TService>(
        CachingChallengeOptions options,
        CancellationToken cancellationToken)
        where TService : class, ICachingChallengeService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();

        ForceCollection();
        var baselineBytes = GC.GetTotalMemory(true);
        var telemetry = await service.RunAsync(options, cancellationToken);
        ForceCollection();
        var afterBytes = GC.GetTotalMemory(true);

        var retainedBytes = Math.Max(0, afterBytes - baselineBytes);
        var hitRatio = telemetry.TotalRequests == 0
            ? 0
            : telemetry.Hits * 100.0 / telemetry.TotalRequests;

        return new CachingRunResult(
            service.Name,
            retainedBytes,
            hitRatio,
            telemetry.ElapsedMilliseconds,
            telemetry.BackendCalls,
            telemetry.CacheEntries,
            telemetry.Hits,
            telemetry.Misses);
    }

    private static CachingChallengeOptions NormalizeOptions(
        int totalRequests,
        int concurrentWorkers,
        int keySpace,
        int payloadKilobytes)
    {
        return new CachingChallengeOptions(
            Math.Clamp(totalRequests, 100, 5000),
            Math.Clamp(concurrentWorkers, 4, 128),
            Math.Clamp(keySpace, 5, 500),
            Math.Clamp(payloadKilobytes, 4, 256));
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
