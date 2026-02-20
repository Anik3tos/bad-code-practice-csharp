namespace BadCodePractice.Features.ResiliencyRetryChallenge;

public sealed class RefactoredResiliencyRetryService : ResiliencyRetryServiceBase
{
    public override string Name => "Refactored retry strategy";

    private readonly SimpleCircuitBreaker _circuitBreaker = new(
        failureThreshold: 8,
        openDuration: TimeSpan.FromMilliseconds(450));

    protected override async Task<bool> ExecuteRequestAsync(
        FaultInjectedDownstream downstream,
        ResiliencyRetryOptions options,
        int requestId,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 4;

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            if (!_circuitBreaker.CanExecute())
            {
                return false;
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(220 + attempt * 60);

            try
            {
                await CallDownstreamAsync(downstream, requestId, attempt, timeoutCts.Token);
                _circuitBreaker.RecordSuccess();
                return true;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _circuitBreaker.RecordFailure();
            }
            catch (DownstreamTransientException)
            {
                _circuitBreaker.RecordFailure();
            }

            if (attempt < maxRetries)
            {
                var backoff = BuildJitteredBackoff(attempt, requestId);
                await Task.Delay(backoff, cancellationToken);
            }
        }

        return false;
    }

    private static TimeSpan BuildJitteredBackoff(int attempt, int requestId)
    {
        var exponentialMs = Math.Min(500, 40 * (1 << attempt));
        var jitterMs = Math.Abs((requestId * 31 + attempt * 17) % 30);
        return TimeSpan.FromMilliseconds(exponentialMs + jitterMs);
    }
}
