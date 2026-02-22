namespace BadCodePractice.Features.ConcurrencyChallenge;

public class PracticeConcurrencyService : IConcurrencyService
{
    public string Name => "Practice Concurrency (Your Turn - Multiple Issues)";

    // TODO: Fix shared mutable state so updates are always thread-safe.
    private int _sharedTotal;

    // TODO: This extra counter is intentionally updated unsafely.
    private int _auditCount;

    // TODO: Make this signal safe to read/write across threads.
    private bool _stopRequested;

    public async Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        _sharedTotal = 0;
        _auditCount = 0;
        _stopRequested = false;

        var tasks = new List<Task>(count);

        // TODO: Remove fire-and-forget work or make completion observable.
        _ = Task.Run(() =>
        {
            Thread.SpinWait(5_000);
            _stopRequested = true;
        });

        for (int i = 0; i < count; i++)
        {
            var itemId = i;
            tasks.Add(Task.Run(() => ProcessSingleItem(itemId), cancellationToken));
        }

        await Task.WhenAll(tasks);

        return _sharedTotal;
    }

    private void ProcessSingleItem(int itemId)
    {
        // TODO: Fix visibility and synchronization around this shared flag.
        if (_stopRequested && (itemId & 1) == 0)
        {
            return;
        }

        // TODO: Replace non-atomic read-modify-write with a thread-safe primitive.
        var current = _sharedTotal;
        Thread.SpinWait(100);
        _sharedTotal = current + 1;

        // TODO: Locking on a new object each call provides no protection.
        // TODO: Avoid unobserved fire-and-forget tasks.
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
