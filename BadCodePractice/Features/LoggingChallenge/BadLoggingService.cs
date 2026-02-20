using Microsoft.Extensions.Logging;

namespace BadCodePractice.Features.LoggingChallenge;

public class BadLoggingService(ILogger<BadLoggingService> logger) : ILoggingService
{
    public string Name => "Bad Logging (PII, Noise, No Scope)";

    public async Task ProcessTransactionsAsync(IEnumerable<UserTransaction> transactions)
    {
        // 1. No correlation ID scope created for the batch or individual items.
        // It's impossible to trace these logs back to a specific web request.

        foreach (var tx in transactions)
        {
            // 2. String Interpolation ($""): This ALWAYS allocates a new string in memory
            // even if LogLevel.Information is disabled in appsettings.json.
            // It also defeats Structured Logging destinations (like Elastic/Datadog).
            logger.LogInformation($"Starting transaction {tx.RequestId} for Amount: {tx.Amount}");

            // 3. PII Leakage: Logging sensitive data like credit cards in plaintext.
            logger.LogInformation($"Charging card {tx.CreditCardNumber} for user {tx.UserId}");

            await Task.Delay(1); // Simulate work

            // 4. Noisy logs at the wrong level. This should be Debug or Trace.
            logger.LogInformation($"Finished processing transaction: {tx.RequestId}");
        }
    }
}
