using System.Collections.Concurrent;

namespace BadCodePractice.Features.ResiliencyRetryChallenge;

internal static class ResiliencyRetryWorkload
{
    internal static async Task<IReadOnlyList<RequestOutcome>> ExecuteAsync(
        int totalRequests,
        int maxConcurrency,
        Func<int, CancellationToken, Task<bool>> operation,
        CancellationToken cancellationToken)
    {
        var outcomes = new RequestOutcome[totalRequests];
        var tasks = new List<Task>(totalRequests);
        using var gate = new SemaphoreSlim(maxConcurrency);

        for (var requestId = 0; requestId < totalRequests; requestId++)
        {
            await gate.WaitAsync(cancellationToken);
            var capturedRequestId = requestId;

            tasks.Add(Task.Run(async () =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var success = false;

                try
                {
                    success = await operation(capturedRequestId, cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    success = false;
                }
                catch
                {
                    success = false;
                }
                finally
                {
                    stopwatch.Stop();
                    outcomes[capturedRequestId] = new RequestOutcome(success, stopwatch.Elapsed.TotalMilliseconds);
                    gate.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        return outcomes;
    }

    internal static double CalculateP95(IReadOnlyList<RequestOutcome> outcomes)
    {
        if (outcomes.Count == 0)
        {
            return 0;
        }

        var ordered = outcomes
            .Select(x => x.LatencyMilliseconds)
            .OrderBy(x => x)
            .ToArray();

        var index = (int)Math.Ceiling(0.95 * ordered.Length) - 1;
        index = Math.Clamp(index, 0, ordered.Length - 1);
        return ordered[index];
    }
}

internal sealed record RequestOutcome(bool Success, double LatencyMilliseconds);

public sealed class FaultInjectedDownstream(ResiliencyRetryOptions options)
{
    internal async Task<string> CallAsync(
        int requestId,
        int attempt,
        CancellationToken cancellationToken)
    {
        var faultScore = Score(requestId, attempt, 11);
        var slowScore = Score(requestId, attempt, 29);
        var jitterScore = Score(requestId, attempt, 53);

        var inFaultStorm = (requestId / 40) % 5 == 3;
        var shouldFail = faultScore < options.FaultPercent || (inFaultStorm && faultScore < 85);
        var shouldBeSlow = slowScore < options.SlowPercent || inFaultStorm;

        var normalDelay = 20 + jitterScore % 35;
        var slowDelay = options.SlowLatencyMilliseconds + jitterScore % 90;
        var delay = shouldBeSlow ? slowDelay : normalDelay;

        await Task.Delay(delay, cancellationToken);

        if (shouldFail)
        {
            throw new DownstreamTransientException("Downstream transient failure injected.");
        }

        return "ok";
    }

    private static int Score(int requestId, int attempt, int salt)
    {
        unchecked
        {
            var value = requestId * 73856093;
            value ^= attempt * 19349663;
            value ^= salt * 83492791;
            value ^= value >> 13;
            value *= 1274126177;
            value ^= value >> 16;
            value = Math.Abs(value);
            return value % 100;
        }
    }
}

internal sealed class DownstreamTransientException(string message) : Exception(message);

internal sealed class SimpleCircuitBreaker(int failureThreshold, TimeSpan openDuration)
{
    private int _consecutiveFailures;
    private long _openUntilEpochMs;

    internal bool CanExecute()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var openUntil = Volatile.Read(ref _openUntilEpochMs);
        return now >= openUntil;
    }

    internal void RecordSuccess()
    {
        Interlocked.Exchange(ref _consecutiveFailures, 0);
    }

    internal void RecordFailure()
    {
        var failures = Interlocked.Increment(ref _consecutiveFailures);
        if (failures < failureThreshold)
        {
            return;
        }

        var openUntil = DateTimeOffset.UtcNow.Add(openDuration).ToUnixTimeMilliseconds();
        Interlocked.Exchange(ref _openUntilEpochMs, openUntil);
        Interlocked.Exchange(ref _consecutiveFailures, 0);
    }
}
