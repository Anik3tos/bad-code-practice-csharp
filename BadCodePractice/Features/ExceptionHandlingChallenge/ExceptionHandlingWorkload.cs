namespace BadCodePractice.Features.ExceptionHandlingChallenge;

internal static class ExceptionHandlingWorkload
{
    internal static async Task<IReadOnlyList<ExceptionRequestOutcome>> ExecuteAsync(
        int totalRequests,
        int maxConcurrency,
        Func<int, CancellationToken, Task<ExceptionRequestOutcome>> operation,
        CancellationToken cancellationToken)
    {
        var outcomes = new ExceptionRequestOutcome[totalRequests];
        var tasks = new List<Task>(totalRequests);
        using var gate = new SemaphoreSlim(maxConcurrency);

        for (var requestId = 0; requestId < totalRequests; requestId++)
        {
            await gate.WaitAsync(cancellationToken);
            var capturedRequestId = requestId;

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    outcomes[capturedRequestId] = await operation(capturedRequestId, cancellationToken);
                }
                finally
                {
                    gate.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        return outcomes;
    }
}

internal sealed class ExceptionHandlingDownstream(ExceptionHandlingOptions options)
{
    internal async Task CallAsync(
        int requestId,
        int attempt,
        CancellationToken cancellationToken)
    {
        var permanentScore = Score(requestId, attempt, 19);
        var transientScore = Score(requestId, attempt, 37);
        var latencyScore = Score(requestId, attempt, 73);
        var inFaultBurst = (requestId / 45) % 5 == 2;

        await Task.Delay(10 + latencyScore % 20, cancellationToken);

        if (permanentScore < options.PermanentFaultPercent)
        {
            throw new PermanentDependencyException("Permanent downstream failure.");
        }

        if (transientScore < options.TransientFaultPercent || (inFaultBurst && transientScore < 80))
        {
            throw new TransientDependencyException("Transient downstream failure.");
        }
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

internal sealed class TransientDependencyException(string message) : Exception(message);

internal sealed class PermanentDependencyException(string message) : Exception(message);
