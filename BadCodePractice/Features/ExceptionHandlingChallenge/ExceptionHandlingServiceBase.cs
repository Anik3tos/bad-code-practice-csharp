using System.Diagnostics;

namespace BadCodePractice.Features.ExceptionHandlingChallenge;

public abstract class ExceptionHandlingServiceBase : IExceptionHandlingService
{
    private int _downstreamCallCount;
    private ExceptionHandlingDownstream? _downstream;

    public abstract string Name { get; }

    public async Task<ExceptionHandlingTelemetry> RunAsync(
        ExceptionHandlingOptions options,
        CancellationToken cancellationToken = default)
    {
        _downstreamCallCount = 0;
        var normalizedOptions = NormalizeOptions(options);
        _downstream = new ExceptionHandlingDownstream(normalizedOptions);

        var stopwatch = Stopwatch.StartNew();
        var outcomes = await ExceptionHandlingWorkload.ExecuteAsync(
            normalizedOptions.TotalRequests,
            normalizedOptions.ConcurrentWorkers,
            (requestId, ct) => ExecuteRequestAsync(normalizedOptions, requestId, ct),
            cancellationToken);
        stopwatch.Stop();

        _downstream = null;
        var failedRequests = outcomes.Count(x => !x.Success);
        var retryAttemptedRequests = outcomes.Count(x => x.RetryAttempted);
        var retryRecoveredRequests = outcomes.Count(x => x.RecoveredByRetry);
        var diagnosability = CalculateDiagnosabilityScore(outcomes);

        return new ExceptionHandlingTelemetry(
            normalizedOptions.TotalRequests,
            failedRequests,
            retryAttemptedRequests,
            retryRecoveredRequests,
            _downstreamCallCount,
            diagnosability,
            stopwatch.Elapsed.TotalMilliseconds);
    }

    protected async Task CallDownstreamAsync(
        int requestId,
        int attempt,
        CancellationToken cancellationToken)
    {
        var downstream = _downstream ?? throw new InvalidOperationException("Downstream is not initialized.");
        Interlocked.Increment(ref _downstreamCallCount);
        await downstream.CallAsync(requestId, attempt, cancellationToken);
    }

    protected abstract Task<ExceptionRequestOutcome> ExecuteRequestAsync(
        ExceptionHandlingOptions options,
        int requestId,
        CancellationToken cancellationToken);

    private static ExceptionHandlingOptions NormalizeOptions(ExceptionHandlingOptions options)
    {
        var totalRequests = Math.Clamp(options.TotalRequests, 100, 5000);
        var concurrentWorkers = Math.Clamp(options.ConcurrentWorkers, 4, 128);
        var permanentFaultPercent = Math.Clamp(options.PermanentFaultPercent, 0, 40);
        var transientFaultCap = Math.Max(0, 95 - permanentFaultPercent);
        var transientFaultPercent = Math.Clamp(options.TransientFaultPercent, 0, transientFaultCap);

        return options with
        {
            TotalRequests = totalRequests,
            ConcurrentWorkers = concurrentWorkers,
            PermanentFaultPercent = permanentFaultPercent,
            TransientFaultPercent = transientFaultPercent
        };
    }

    private static double CalculateDiagnosabilityScore(IReadOnlyList<ExceptionRequestOutcome> outcomes)
    {
        var failures = outcomes.Where(x => !x.Success).ToArray();
        if (failures.Length == 0)
        {
            return 100;
        }

        const double requiredFieldsPerFailure = 6;
        var presentFieldCount = failures.Sum(failure => CountPresentFields(failure.ErrorContext));
        return presentFieldCount * 100.0 / (failures.Length * requiredFieldsPerFailure);
    }

    private static int CountPresentFields(ExceptionDiagnosticContext? context)
    {
        if (context is null)
        {
            return 0;
        }

        var count = 0;
        if (!string.IsNullOrWhiteSpace(context.CorrelationId))
        {
            count++;
        }

        if (!string.IsNullOrWhiteSpace(context.Operation))
        {
            count++;
        }

        if (!string.IsNullOrWhiteSpace(context.ErrorCode))
        {
            count++;
        }

        if (!string.IsNullOrWhiteSpace(context.ExceptionType))
        {
            count++;
        }

        if (context.Attempt.HasValue)
        {
            count++;
        }

        if (context.IsTransient.HasValue)
        {
            count++;
        }

        return count;
    }
}
