namespace BadCodePractice.Features.ConcurrencyChallenge;

public class RefactoredConcurrencyService : IConcurrencyService
{
    public string Name => "AI Refactored Concurrency (Thread-safe)";

    private int _sharedTotal;

    public async Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        _sharedTotal = 0;
        var tasks = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            tasks.Add(Task.Run(() => ProcessSingleItem(), cancellationToken));
        }

        await Task.WhenAll(tasks);

        return _sharedTotal;
    }

    private void ProcessSingleItem()
    {
        // Good: Using Interlocked to safely increment the shared integer
        // Alternatively, a lock statement could be used around the increment logic.
        Thread.SpinWait(100);
        Interlocked.Increment(ref _sharedTotal);
    }
}
