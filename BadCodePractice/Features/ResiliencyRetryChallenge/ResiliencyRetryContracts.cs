namespace BadCodePractice.Features.ResiliencyRetryChallenge;

public interface IResiliencyRetryService
{
    string Name { get; }
    Task<ResiliencyTelemetry> RunAsync(ResiliencyRetryOptions options, CancellationToken cancellationToken = default);
}

public sealed record ResiliencyRetryOptions(
    int TotalRequests,
    int ConcurrentWorkers,
    int FaultPercent,
    int SlowPercent,
    int SlowLatencyMilliseconds);

public sealed record ResiliencyTelemetry(
    int TotalRequests,
    int SuccessfulRequests,
    int DownstreamCallCount,
    double ElapsedMilliseconds,
    double P95LatencyMilliseconds);

public sealed record ResiliencyScenarioResult(
    string Label,
    int TotalRequests,
    int SuccessfulRequests,
    double SuccessRatePercent,
    int DownstreamCallCount,
    double ElapsedMilliseconds,
    double P95LatencyMilliseconds);

public sealed record ResiliencyRetryComparisonResult(
    ResiliencyRetryOptions Options,
    ResiliencyScenarioResult Bad,
    ResiliencyScenarioResult Practice,
    ResiliencyScenarioResult Refactored);
