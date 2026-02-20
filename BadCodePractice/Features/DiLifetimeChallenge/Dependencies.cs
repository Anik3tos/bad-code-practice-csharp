namespace BadCodePractice.Features.DiLifetimeChallenge;

public interface IScopedState
{
    Guid ScopeId { get; }
    int RequestCount { get; }
    void Increment();
}

public class ScopedState : IScopedState
{
    public Guid ScopeId { get; } = Guid.NewGuid();
    public int RequestCount { get; private set; }

    public void Increment()
    {
        RequestCount++;
    }
}

public interface ITransientOperation : IDisposable
{
    Guid OperationId { get; }
    void Execute();
}

public class TransientOperation : ITransientOperation
{
    public Guid OperationId { get; } = Guid.NewGuid();
    
    // Simulate some memory allocation
    private readonly byte[] _buffer = new byte[1024 * 1024]; // 1MB

    public void Execute()
    {
        // Do nothing, just alive
    }

    public void Dispose()
    {
        // Suppress warning
        GC.SuppressFinalize(this);
    }
}
