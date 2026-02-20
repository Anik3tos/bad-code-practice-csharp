namespace BadCodePractice.Features.DiLifetimeChallenge;

// This will be registered as a SINGLETON
public class PracticeDiLifetimeService : IDiLifetimeService
{
    public string Name => "Practice DI Lifetime (Your Turn)";
    
    private readonly IScopedState _scopedState;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ITransientOperation> _operationsCache = new();

    public PracticeDiLifetimeService(IScopedState scopedState, IServiceProvider serviceProvider)
    {
        _scopedState = scopedState;
        _serviceProvider = serviceProvider;
    }

    public async Task<DiScenarioResult> ExecuteOperationAsync(string requestId)
    {
        await Task.Delay(10); 
        
        _scopedState.Increment();

        var operation = _serviceProvider.GetRequiredService<ITransientOperation>();
        operation.Execute();
        _operationsCache.Add(operation);

        return new DiScenarioResult(
            Name,
            requestId,
            _scopedState.ScopeId,
            _scopedState.RequestCount,
            10
        );
    }
}
