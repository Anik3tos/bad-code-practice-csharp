namespace BadCodePractice.Features.ConcurrencyChallenge;

public class PracticeConcurrencyService : IConcurrencyService
{
    public string Name => "Practice Concurrency (Your Turn)";
    
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
        var current = _sharedTotal;
        Thread.SpinWait(100); 
        _sharedTotal = current + 1;
    }
}
