using System.Diagnostics;

namespace BadCodePractice.Features.ConcurrencyChallenge;

public sealed class ConcurrencyChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<ConcurrencyChallengeComparisonResult> RunAsync(int count, CancellationToken cancellationToken = default)
    {
        var bad = await RunScenarioAsync<BadConcurrencyService>(count, cancellationToken);
        var practice = await RunScenarioAsync<PracticeConcurrencyService>(count, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredConcurrencyService>(count, cancellationToken);

        return new ConcurrencyChallengeComparisonResult(count, bad, practice, refactored);
    }

    private async Task<ConcurrencyScenarioRunResult> RunScenarioAsync<TService>(
        int count,
        CancellationToken cancellationToken)
        where TService : class, IConcurrencyService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        
        var stopwatch = Stopwatch.StartNew();
        
        // Wrap in Task.Run to prevent deadlocking the Blazor UI SynchronizationContext if .Result or .Wait() was used
        var actualCount = await Task.Run(() => service.ProcessItemsAsync(count, cancellationToken), cancellationToken);
        
        stopwatch.Stop();
        
        return new ConcurrencyScenarioRunResult(
            service.Name,
            count,
            actualCount,
            stopwatch.Elapsed.TotalMilliseconds);
    }
}

public sealed record ConcurrencyChallengeComparisonResult(
    int ExpectedCount,
    ConcurrencyScenarioRunResult Bad,
    ConcurrencyScenarioRunResult Practice,
    ConcurrencyScenarioRunResult Refactored);

public sealed record ConcurrencyScenarioRunResult(
    string Label,
    int ExpectedCount,
    int ActualCount,
    double ElapsedMilliseconds)
{
    public bool IsCorrect => ExpectedCount == ActualCount;
    public int Difference => ExpectedCount - ActualCount;
}
