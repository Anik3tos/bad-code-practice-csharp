namespace BadCodePractice.Features.ExceptionHandlingChallenge;

public interface IExceptionHandlingService
{
    string Name { get; }
    Task<ExceptionHandlingTelemetry> RunAsync(
        ExceptionHandlingOptions options,
        CancellationToken cancellationToken = default);
}

public sealed record ExceptionHandlingOptions(
    int TotalRequests,
    int ConcurrentWorkers,
    int TransientFaultPercent,
    int PermanentFaultPercent);

public sealed record ExceptionDiagnosticContext(
    string? CorrelationId,
    string? Operation,
    string? ErrorCode,
    string? ExceptionType,
    int? Attempt,
    bool? IsTransient);

public sealed record ExceptionRequestOutcome(
    bool Success,
    int Attempts,
    bool RetryAttempted,
    bool RecoveredByRetry,
    ExceptionDiagnosticContext? ErrorContext);

public sealed record ExceptionHandlingTelemetry(
    int TotalRequests,
    int FailedRequests,
    int RetryAttemptedRequests,
    int RetryRecoveredRequests,
    int DownstreamCallCount,
    double DiagnosabilityScorePercent,
    double ElapsedMilliseconds);

public sealed record ExceptionHandlingScenarioResult(
    string Label,
    int TotalRequests,
    int FailedRequests,
    double FailureRatePercent,
    double DiagnosabilityScorePercent,
    double MeanRetrySuccessPercent,
    int DownstreamCallCount,
    double ElapsedMilliseconds);

public sealed record ExceptionHandlingComparisonResult(
    ExceptionHandlingOptions Options,
    ExceptionHandlingScenarioResult Bad,
    ExceptionHandlingScenarioResult Practice,
    ExceptionHandlingScenarioResult Refactored);
