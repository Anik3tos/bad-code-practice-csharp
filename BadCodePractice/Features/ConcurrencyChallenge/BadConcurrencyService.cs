namespace BadCodePractice.Features.ConcurrencyChallenge;

public class BadConcurrencyService : IConcurrencyService
{
    public string Name => "Bad Concurrency (Shared state, no lock)";
    
    // Shared mutable state across all concurrent tasks
    private int _sharedTotal;

    public async Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        _sharedTotal = 0;
        var tasks = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            // Firing off tasks concurrently without synchronizing access to _sharedTotal
            tasks.Add(Task.Run(() => ProcessSingleItem(), cancellationToken));
        }

        await Task.WhenAll(tasks);
        
        return _sharedTotal;
    }

    private void ProcessSingleItem()
    {
        // Bad: Read, Modify, Write on shared state without synchronization
        // Leads to lost updates when multiple threads do this simultaneously
        var current = _sharedTotal;
        
        // Simulating a tiny bit of work to increase the chance of context switch
        // between read and write, making the race condition more obvious
        Thread.SpinWait(100); 
        
        _sharedTotal = current + 1;
    }
}
