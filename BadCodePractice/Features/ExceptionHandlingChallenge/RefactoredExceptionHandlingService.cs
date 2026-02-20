namespace BadCodePractice.Features.ExceptionHandlingChallenge;

public sealed class RefactoredExceptionHandlingService : ExceptionHandlingServiceBase
{
    public override string Name => "AI Refactored exception handling";

    protected override async Task<ExceptionRequestOutcome> ExecuteRequestAsync(
        ExceptionHandlingOptions options,
        int requestId,
        CancellationToken cancellationToken)
    {
        var correlationId = BuildCorrelationId(requestId);
        const int maxRetries = 3;
        var retryAttempted = false;
        var firstAttemptFailed = false;

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await CallDownstreamAsync(requestId, attempt, cancellationToken);
                return new ExceptionRequestOutcome(
                    Success: true,
                    Attempts: attempt + 1,
                    RetryAttempted: retryAttempted,
                    RecoveredByRetry: firstAttemptFailed,
                    ErrorContext: null);
            }
            catch (TransientDependencyException exception)
            {
                if (attempt == 0)
                {
                    firstAttemptFailed = true;
                }

                if (attempt >= maxRetries)
                {
                    return new ExceptionRequestOutcome(
                        Success: false,
                        Attempts: attempt + 1,
                        RetryAttempted: retryAttempted,
                        RecoveredByRetry: false,
                        ErrorContext: BuildContext(
                            correlationId,
                            "Challenge.DownstreamCall",
                            "DOWNSTREAM_TRANSIENT_EXHAUSTED",
                            exception,
                            attempt + 1,
                            isTransient: true));
                }

                retryAttempted = true;
                await Task.Delay(BuildJitteredBackoff(attempt, requestId), cancellationToken);
            }
            catch (PermanentDependencyException exception)
            {
                if (attempt == 0)
                {
                    firstAttemptFailed = true;
                }

                return new ExceptionRequestOutcome(
                    Success: false,
                    Attempts: attempt + 1,
                    RetryAttempted: retryAttempted,
                    RecoveredByRetry: false,
                    ErrorContext: BuildContext(
                        correlationId,
                        "Challenge.DownstreamCall",
                        "DOWNSTREAM_PERMANENT",
                        exception,
                        attempt + 1,
                        isTransient: false));
            }
            catch (Exception exception)
            {
                if (attempt == 0)
                {
                    firstAttemptFailed = true;
                }

                return new ExceptionRequestOutcome(
                    Success: false,
                    Attempts: attempt + 1,
                    RetryAttempted: retryAttempted,
                    RecoveredByRetry: false,
                    ErrorContext: BuildContext(
                        correlationId,
                        "Challenge.DownstreamCall",
                        "UNEXPECTED_EXCEPTION",
                        exception,
                        attempt + 1,
                        isTransient: false));
            }
        }

        return new ExceptionRequestOutcome(
            Success: false,
            Attempts: maxRetries + 1,
            RetryAttempted: retryAttempted,
            RecoveredByRetry: false,
            ErrorContext: new ExceptionDiagnosticContext(
                CorrelationId: correlationId,
                Operation: "Challenge.DownstreamCall",
                ErrorCode: "UNREACHABLE_STATE",
                ExceptionType: "InvalidOperationException",
                Attempt: maxRetries + 1,
                IsTransient: false));
    }

    private static string BuildCorrelationId(int requestId)
    {
        return $"req-{requestId:D5}-{Math.Abs(requestId * 97 % 1000):D3}";
    }

    private static ExceptionDiagnosticContext BuildContext(
        string correlationId,
        string operation,
        string errorCode,
        Exception exception,
        int attempt,
        bool isTransient)
    {
        return new ExceptionDiagnosticContext(
            CorrelationId: correlationId,
            Operation: operation,
            ErrorCode: errorCode,
            ExceptionType: exception.GetType().Name,
            Attempt: attempt,
            IsTransient: isTransient);
    }

    private static TimeSpan BuildJitteredBackoff(int attempt, int requestId)
    {
        var exponentialMs = Math.Min(220, 30 * (1 << attempt));
        var jitterMs = Math.Abs((requestId * 23 + attempt * 11) % 25);
        return TimeSpan.FromMilliseconds(exponentialMs + jitterMs);
    }
}
