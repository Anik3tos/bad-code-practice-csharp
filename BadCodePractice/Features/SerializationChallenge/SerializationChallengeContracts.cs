namespace BadCodePractice.Features.SerializationChallenge;

public interface ISerializationChallengeService
{
    string Name { get; }
    Task<SerializationTelemetry> RunAsync(
        SerializationChallengeOptions options,
        CancellationToken cancellationToken = default);
}

public sealed record SerializationChallengeOptions(
    int TotalRequests,
    int ConcurrentWorkers,
    int ExtraFieldLength);

public sealed record SerializationTelemetry(
    int TotalRequests,
    long AllocatedBytes,
    double ElapsedMilliseconds,
    int ReflectionCacheMisses);

public sealed record SerializationScenarioResult(
    string Label,
    int TotalRequests,
    long AllocatedBytes,
    double ElapsedMilliseconds,
    int ReflectionCacheMisses);

public sealed record SerializationComparisonResult(
    SerializationChallengeOptions Options,
    SerializationScenarioResult Bad,
    SerializationScenarioResult Practice,
    SerializationScenarioResult Refactored);
