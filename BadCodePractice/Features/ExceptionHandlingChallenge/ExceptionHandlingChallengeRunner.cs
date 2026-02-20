namespace BadCodePractice.Features.ExceptionHandlingChallenge;

public sealed class ExceptionHandlingChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<ExceptionHandlingComparisonResult> RunAsync(
        int totalRequests,
        int concurrentWorkers,
        int transientFaultPercent,
        int permanentFaultPercent,
        CancellationToken cancellationToken = default)
    {
        var options = new ExceptionHandlingOptions(
            TotalRequests: totalRequests,
            ConcurrentWorkers: concurrentWorkers,
            TransientFaultPercent: transientFaultPercent,
            PermanentFaultPercent: permanentFaultPercent);

        var bad = await RunScenarioAsync<BadExceptionHandlingService>(options, cancellationToken);
        var practice = await RunScenarioAsync<PracticeExceptionHandlingService>(options, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredExceptionHandlingService>(options, cancellationToken);

        return new ExceptionHandlingComparisonResult(options, bad, practice, refactored);
    }

    private async Task<ExceptionHandlingScenarioResult> RunScenarioAsync<TService>(
        ExceptionHandlingOptions options,
        CancellationToken cancellationToken)
        where TService : class, IExceptionHandlingService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        var telemetry = await service.RunAsync(options, cancellationToken);

        var failureRate = telemetry.TotalRequests == 0
            ? 0
            : telemetry.FailedRequests * 100.0 / telemetry.TotalRequests;

        var meanRetrySuccess = telemetry.RetryAttemptedRequests == 0
            ? 0
            : telemetry.RetryRecoveredRequests * 100.0 / telemetry.RetryAttemptedRequests;

        return new ExceptionHandlingScenarioResult(
            service.Name,
            telemetry.TotalRequests,
            telemetry.FailedRequests,
            failureRate,
            telemetry.DiagnosabilityScorePercent,
            meanRetrySuccess,
            telemetry.DownstreamCallCount,
            telemetry.ElapsedMilliseconds);
    }
}
