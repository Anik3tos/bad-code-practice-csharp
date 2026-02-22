namespace BadCodePractice.Features.ConcurrencyChallenge;

public class RefactoredConcurrencyService : IConcurrencyService
{
    public string Name => "AI Refactored Concurrency (Thread-safe patterns)";

    private int _sharedTotal;
    private int _auditCount;
    private int _stopRequested;

    public async Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        _sharedTotal = 0;
        _auditCount = 0;
        Volatile.Write(ref _stopRequested, 0);

        var tasks = new List<Task>(count + 1);

        // Good: keep all background work observable and await it.
        tasks.Add(Task.Run(() =>
        {
            Thread.SpinWait(5_000);
            Volatile.Write(ref _stopRequested, 1);
        }, cancellationToken));

        for (int i = 0; i < count; i++)
        {
            tasks.Add(Task.Run(() => ProcessSingleItem(cancellationToken), cancellationToken));
        }

        await Task.WhenAll(tasks);

        return Volatile.Read(ref _sharedTotal);
    }

    private void ProcessSingleItem(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Good: atomic increment prevents lost updates.
        Thread.SpinWait(100);
        Interlocked.Increment(ref _sharedTotal);

        // Good: reads are synchronized via Volatile; behavior does not affect correctness.
        if (Volatile.Read(ref _stopRequested) == 0)
        {
            Thread.SpinWait(50);
        }

        // Good: no fake locking and no fire-and-forget updates.
        Interlocked.Increment(ref _auditCount);
    }
}
