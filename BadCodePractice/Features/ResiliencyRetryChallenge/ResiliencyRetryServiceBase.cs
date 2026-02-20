using System.Diagnostics;

namespace BadCodePractice.Features.ResiliencyRetryChallenge;

public abstract class ResiliencyRetryServiceBase : IResiliencyRetryService
{
    private int _downstreamCallCount;

    public abstract string Name { get; }

    public async Task<ResiliencyTelemetry> RunAsync(
        ResiliencyRetryOptions options,
        CancellationToken cancellationToken = default)
    {
        _downstreamCallCount = 0;

        var normalizedOptions = NormalizeOptions(options);
        var downstream = new FaultInjectedDownstream(normalizedOptions);

        var stopwatch = Stopwatch.StartNew();
        var outcomes = await ResiliencyRetryWorkload.ExecuteAsync(
            normalizedOptions.TotalRequests,
            normalizedOptions.ConcurrentWorkers,
            (requestId, ct) => ExecuteRequestAsync(downstream, normalizedOptions, requestId, ct),
            cancellationToken);
        stopwatch.Stop();

        var successfulRequests = outcomes.Count(x => x.Success);
        var p95 = ResiliencyRetryWorkload.CalculateP95(outcomes);

        return new ResiliencyTelemetry(
            normalizedOptions.TotalRequests,
            successfulRequests,
            _downstreamCallCount,
            stopwatch.Elapsed.TotalMilliseconds,
            p95);
    }

    protected async Task CallDownstreamAsync(
        FaultInjectedDownstream downstream,
        int requestId,
        int attempt,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _downstreamCallCount);
        await downstream.CallAsync(requestId, attempt, cancellationToken);
    }

    protected abstract Task<bool> ExecuteRequestAsync(
        FaultInjectedDownstream downstream,
        ResiliencyRetryOptions options,
        int requestId,
        CancellationToken cancellationToken);

    private static ResiliencyRetryOptions NormalizeOptions(ResiliencyRetryOptions options)
    {
        return options with
        {
            TotalRequests = Math.Clamp(options.TotalRequests, 100, 5000),
            ConcurrentWorkers = Math.Clamp(options.ConcurrentWorkers, 4, 128),
            FaultPercent = Math.Clamp(options.FaultPercent, 0, 95),
            SlowPercent = Math.Clamp(options.SlowPercent, 0, 95),
            SlowLatencyMilliseconds = Math.Clamp(options.SlowLatencyMilliseconds, 100, 1500)
        };
    }
}
