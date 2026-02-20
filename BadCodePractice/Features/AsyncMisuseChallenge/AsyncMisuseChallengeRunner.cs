using System.Diagnostics;

namespace BadCodePractice.Features.AsyncMisuseChallenge;

public sealed class AsyncMisuseChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<AsyncChallengeComparisonResult> RunAsync(int count, CancellationToken cancellationToken = default)
    {
        var bad = await RunScenarioAsync<BadAsyncMisuseService>(count, cancellationToken);
        var practice = await RunScenarioAsync<PracticeAsyncMisuseService>(count, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredAsyncMisuseService>(count, cancellationToken);

        return new AsyncChallengeComparisonResult(count, bad, practice, refactored);
    }

    private async Task<AsyncScenarioRunResult> RunScenarioAsync<TService>(
        int count,
        CancellationToken cancellationToken)
        where TService : class, IAsyncMisuseService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();

        // Measure before
        ThreadPool.GetAvailableThreads(out int workerThreadsBefore, out int completionPortThreadsBefore);
        var stopwatch = Stopwatch.StartNew();

        // Wrap in Task.Run to prevent deadlocking the Blazor UI SynchronizationContext
        var processedCount =
            await Task.Run(() => service.ProcessItemsAsync(count, cancellationToken), cancellationToken);

        stopwatch.Stop();

        // Measure after
        ThreadPool.GetAvailableThreads(out int workerThreadsAfter, out int completionPortThreadsAfter);

        return new AsyncScenarioRunResult(
            service.Name,
            processedCount,
            stopwatch.Elapsed.TotalMilliseconds,
            workerThreadsBefore,
            workerThreadsAfter);
    }
}

public sealed record AsyncChallengeComparisonResult(
    int ItemCount,
    AsyncScenarioRunResult Bad,
    AsyncScenarioRunResult Practice,
    AsyncScenarioRunResult Refactored);

public sealed record AsyncScenarioRunResult(
    string Label,
    int ProcessedCount,
    double ElapsedMilliseconds,
    int WorkerThreadsBefore,
    int WorkerThreadsAfter)
{
    public int ThreadsUsed => WorkerThreadsBefore - WorkerThreadsAfter;
}
