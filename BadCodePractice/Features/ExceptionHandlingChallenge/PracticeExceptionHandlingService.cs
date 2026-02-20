namespace BadCodePractice.Features.ExceptionHandlingChallenge;

public sealed class PracticeExceptionHandlingService : ExceptionHandlingServiceBase
{
    public override string Name => "Practice exception handling";

    protected override async Task<ExceptionRequestOutcome> ExecuteRequestAsync(
        ExceptionHandlingOptions options,
        int requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            await CallDownstreamAsync(requestId, attempt: 0, cancellationToken);
            return new ExceptionRequestOutcome(
                Success: true,
                Attempts: 1,
                RetryAttempted: false,
                RecoveredByRetry: false,
                ErrorContext: null);
        }
        catch (TransientDependencyException)
        {
            try
            {
                await CallDownstreamAsync(requestId, attempt: 1, cancellationToken);
                return new ExceptionRequestOutcome(
                    Success: true,
                    Attempts: 2,
                    RetryAttempted: true,
                    RecoveredByRetry: true,
                    ErrorContext: null);
            }
            catch (Exception retryException)
            {
                try
                {
                    throw retryException; // Starts with the same anti-pattern.
                }
                catch
                {
                    // Starts with swallowed exceptions and weak context.
                }

                var minimalContext = requestId % 2 == 0
                    ? null
                    : new ExceptionDiagnosticContext(
                        CorrelationId: null,
                        Operation: "Process",
                        ErrorCode: null,
                        ExceptionType: retryException.GetType().Name,
                        Attempt: null,
                        IsTransient: null);

                return new ExceptionRequestOutcome(
                    Success: false,
                    Attempts: 2,
                    RetryAttempted: true,
                    RecoveredByRetry: false,
                    ErrorContext: minimalContext);
            }
        }
        catch
        {
            return new ExceptionRequestOutcome(
                Success: false,
                Attempts: 1,
                RetryAttempted: false,
                RecoveredByRetry: false,
                ErrorContext: null);
        }
    }
}
