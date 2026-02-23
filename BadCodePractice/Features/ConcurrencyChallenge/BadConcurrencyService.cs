namespace BadCodePractice.Features.ConcurrencyChallenge;

public class BadConcurrencyService : IConcurrencyService
{
    public string Name => "Bad Concurrency (Multiple race patterns)";

    // Shared mutable state across all concurrent tasks.
    private int _sharedTotal;

    // Extra shared state that is also updated from multiple threads.
    private int _auditCount;

    // Shared flag accessed from different threads without synchronization.
    private bool _stopRequested;

    public async Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        _sharedTotal = 0;
        _auditCount = 0;
        _stopRequested = false;

        var tasks = new List<Task>(count);

        // Bad #1: fire-and-forget background task that is never awaited.
        _ = Task.Run(() =>
        {
            Thread.SpinWait(5_000);
            _stopRequested = true;
        });

        for (int i = 0; i < count; i++)
        {
            var itemId = i;

            // Bad #2: worker tasks touch shared state without any synchronization.
            tasks.Add(Task.Run(() => ProcessSingleItem(itemId), cancellationToken));
        }

        await Task.WhenAll(tasks);

        return _sharedTotal;
    }

    private void ProcessSingleItem(int itemId)
    {
        // Bad #3: unsynchronized shared flag read (no volatile, no lock).
        if (_stopRequested && (itemId & 1) == 0)
        {
            return;
        }

        // Bad #4: read-modify-write on shared state without synchronization.
        var current = _sharedTotal;

        // Simulate work to increase context-switch probability.
        Thread.SpinWait(100);

        _sharedTotal = current + 1;

        // Bad #5: lock on a new object each time, so it protects nothing.
        // Also fire-and-forget work with errors/completion never observed.
        _ = Task.Run(() =>
        {
            lock (new object())
            {
                var snapshot = _auditCount;
                Thread.SpinWait(50);
                _auditCount = snapshot + 1;
            }
        });
    }
}
