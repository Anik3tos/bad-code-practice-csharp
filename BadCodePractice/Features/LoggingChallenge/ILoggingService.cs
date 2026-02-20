namespace BadCodePractice.Features.LoggingChallenge;

public sealed record UserTransaction(
    string RequestId,
    string UserId,
    string CreditCardNumber,
    decimal Amount);

public interface ILoggingService
{
    string Name { get; }
    Task ProcessTransactionsAsync(IEnumerable<UserTransaction> transactions);
}
