namespace BadCodePractice.Features.MemoryLeakChallenge;

public sealed class MemoryLeakChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<MemoryLeakComparisonResult> RunAsync(
        int iterations,
        int payloadKilobytes,
        CancellationToken cancellationToken = default)
    {
        var bad = await RunScenarioAsync<BadMemoryLeakService>(iterations, payloadKilobytes, cancellationToken);
        var practice = await RunScenarioAsync<PracticeMemoryLeakService>(iterations, payloadKilobytes, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredMemoryLeakService>(
            iterations,
            payloadKilobytes,
            cancellationToken);

        return new MemoryLeakComparisonResult(bad, practice, refactored);
    }

    public void ResetLeakedState()
    {
        MemoryLeakRoots.Reset();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private async Task<MemoryLeakRunResult> RunScenarioAsync<TService>(
        int iterations,
        int payloadKilobytes,
        CancellationToken cancellationToken)
        where TService : class, IMemoryLeakService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await service.RunAsync(iterations, payloadKilobytes, cancellationToken);
    }
}
