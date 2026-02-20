namespace BadCodePractice.Infrastructure;

public sealed class QueryMetrics
{
    private int _commandCount;

    public int CommandCount => Volatile.Read(ref _commandCount);

    public void Reset()
    {
        Interlocked.Exchange(ref _commandCount, 0);
    }

    public void Increment()
    {
        Interlocked.Increment(ref _commandCount);
    }
}
