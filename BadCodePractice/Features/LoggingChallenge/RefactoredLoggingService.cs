using Microsoft.Extensions.Logging;

namespace BadCodePractice.Features.LoggingChallenge;

public class RefactoredLoggingService(ILogger<RefactoredLoggingService> logger) : ILoggingService
{
    public string Name => "Refactored Logging (Structured, Masked)";

    public async Task ProcessTransactionsAsync(IEnumerable<UserTransaction> transactions)
    {
        foreach (var tx in transactions)
        {
            // 1. Use BeginScope to attach CorrelationId to ALL logs inside this using block automatically.
            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = tx.RequestId }))
            {
                // 2. Structured Logging: Use message templates, NOT string interpolation.
                // The logging framework only formats the string if Information is enabled.
                logger.LogInformation("Starting transaction for Amount: {Amount}", tx.Amount);

                // 3. Mask PII data. Never log raw sensitive numbers.
                var maskedCard = "****-****-****-" + tx.CreditCardNumber[^4..];
                logger.LogInformation("Charging card {MaskedCreditCard} for user {UserId}", maskedCard, tx.UserId);

                await Task.Delay(1);

                // 4. Shift overly noisy operational logs to Debug or Trace level
                logger.LogDebug("Finished processing transaction");
            }
        }
    }
}
