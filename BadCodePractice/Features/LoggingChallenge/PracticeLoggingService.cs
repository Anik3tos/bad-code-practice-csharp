using Microsoft.Extensions.Logging;

namespace BadCodePractice.Features.LoggingChallenge;

public class PracticeLoggingService(ILogger<PracticeLoggingService> logger) : ILoggingService
{
    public string Name => "Practice Logging (Your Turn)";

    public async Task ProcessTransactionsAsync(IEnumerable<UserTransaction> transactions)
    {
        foreach (var tx in transactions)
        {
            logger.LogInformation($"Starting transaction {tx.RequestId} for Amount: {tx.Amount}");

            logger.LogInformation($"Charging card {tx.CreditCardNumber} for user {tx.UserId}");

            await Task.Delay(1);

            logger.LogInformation($"Finished processing transaction: {tx.RequestId}");
        }
    }
}
