namespace BadCodePractice.Features.ConcurrencyChallenge;

public interface IConcurrencyService
{
    string Name { get; }
    Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default);
}
