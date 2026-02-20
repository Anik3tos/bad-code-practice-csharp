namespace BadCodePractice.Features.ResiliencyRetryChallenge;

public sealed class PracticeResiliencyRetryService : ResiliencyRetryServiceBase
{
    public override string Name => "Practice retry strategy";

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
                // Starts with the same bad behavior for refactoring practice.
            }
        }

        return false;
    }
}
