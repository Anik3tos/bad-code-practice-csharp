namespace BadCodePractice.Features.DiLifetimeChallenge;

// This will be registered as a SINGLETON
public class BadDiLifetimeService : IDiLifetimeService
{
    public string Name => "Bad DI Lifetime (Singleton with captives)";
    
    // Captive dependency! A scoped object injected into a singleton lives forever.
    private readonly IScopedState _scopedState;
    
    // We inject IServiceProvider to act as a factory, but we hold the resolved services forever in a list
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ITransientOperation> _operationsCache = new();

    public BadDiLifetimeService(IScopedState scopedState, IServiceProvider serviceProvider)
    {
        _scopedState = scopedState;
        _serviceProvider = serviceProvider;
    }

    public async Task<DiScenarioResult> ExecuteOperationAsync(string requestId)
    {
        await Task.Delay(10); // Simulate work
        
        // This state is now shared across ALL requests in the app
        _scopedState.Increment();

        // Memory leak! 
        // Resolving a transient/disposable from the root provider and holding it forever
        var operation = _serviceProvider.GetRequiredService<ITransientOperation>();
        operation.Execute();
        _operationsCache.Add(operation);

        return new DiScenarioResult(
            Name,
            requestId,
            _scopedState.ScopeId,
            _scopedState.RequestCount,
            10 // mock elapsed
        );
    }
}
