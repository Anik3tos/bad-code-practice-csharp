namespace BadCodePractice.Features.ResiliencyRetryChallenge;

public sealed class BadResiliencyRetryService : ResiliencyRetryServiceBase
{
    public override string Name => "Bad retry strategy";

    protected override async Task<bool> ExecuteRequestAsync(
        FaultInjectedDownstream downstream,
        ResiliencyRetryOptions options,
        int requestId,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await CallDownstreamAsync(downstream, requestId, attempt, cancellationToken);
                return true;
            }
            catch (DownstreamTransientException) when (attempt < maxRetries)
            {
                // Blind retry: no timeout, no jitter/backoff, no circuit breaker.
            }
        }

        return false;
    }
}
