namespace BadCodePractice.Features.ResiliencyRetryChallenge;

public sealed class ResiliencyRetryChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<ResiliencyRetryComparisonResult> RunAsync(
        int totalRequests,
        int concurrentWorkers,
        int faultPercent,
        int slowPercent,
        int slowLatencyMilliseconds,
        CancellationToken cancellationToken = default)
    {
        var options = new ResiliencyRetryOptions(
            TotalRequests: totalRequests,
            ConcurrentWorkers: concurrentWorkers,
            FaultPercent: faultPercent,
            SlowPercent: slowPercent,
            SlowLatencyMilliseconds: slowLatencyMilliseconds);

        var bad = await RunScenarioAsync<BadResiliencyRetryService>(options, cancellationToken);
        var practice = await RunScenarioAsync<PracticeResiliencyRetryService>(options, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredResiliencyRetryService>(options, cancellationToken);

        return new ResiliencyRetryComparisonResult(options, bad, practice, refactored);
    }

    private async Task<ResiliencyScenarioResult> RunScenarioAsync<TService>(
        ResiliencyRetryOptions options,
        CancellationToken cancellationToken)
        where TService : class, IResiliencyRetryService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        var telemetry = await service.RunAsync(options, cancellationToken);

        var successRate = telemetry.TotalRequests == 0
            ? 0
            : telemetry.SuccessfulRequests * 100.0 / telemetry.TotalRequests;

        return new ResiliencyScenarioResult(
            service.Name,
            telemetry.TotalRequests,
            telemetry.SuccessfulRequests,
            successRate,
            telemetry.DownstreamCallCount,
            telemetry.ElapsedMilliseconds,
            telemetry.P95LatencyMilliseconds);
    }
}
