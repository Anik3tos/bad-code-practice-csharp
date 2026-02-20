namespace BadCodePractice.Features.AsyncMisuseChallenge;

public interface IAsyncMisuseService
{
    string Name { get; }
    Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default);
}
