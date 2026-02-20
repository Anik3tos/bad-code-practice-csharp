namespace BadCodePractice.Features.SerializationChallenge;

public sealed class SerializationChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<SerializationComparisonResult> RunAsync(
        int totalRequests,
        int concurrentWorkers,
        int extraFieldLength,
        CancellationToken cancellationToken = default)
    {
        var options = new SerializationChallengeOptions(
            TotalRequests: totalRequests,
            ConcurrentWorkers: concurrentWorkers,
            ExtraFieldLength: extraFieldLength);

        var bad = await RunScenarioAsync<BadSerializationService>(options, cancellationToken);
        var practice = await RunScenarioAsync<PracticeSerializationService>(options, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredSerializationService>(options, cancellationToken);

        return new SerializationComparisonResult(options, bad, practice, refactored);
    }

    private async Task<SerializationScenarioResult> RunScenarioAsync<TService>(
        SerializationChallengeOptions options,
        CancellationToken cancellationToken)
        where TService : class, ISerializationChallengeService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        var telemetry = await service.RunAsync(options, cancellationToken);

        return new SerializationScenarioResult(
            service.Name,
            telemetry.TotalRequests,
            telemetry.AllocatedBytes,
            telemetry.ElapsedMilliseconds,
            telemetry.ReflectionCacheMisses);
    }
}
