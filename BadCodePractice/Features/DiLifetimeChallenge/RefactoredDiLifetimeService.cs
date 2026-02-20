namespace BadCodePractice.Features.DiLifetimeChallenge;

// This will be registered as a SINGLETON
public class RefactoredDiLifetimeService : IDiLifetimeService
{
    public string Name => "AI RefactoredDI Lifetime (Scope Factory)";
    
    private readonly IServiceScopeFactory _scopeFactory;

    // Inject IServiceScopeFactory into Singletons if they need to do scope-level work
    public RefactoredDiLifetimeService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<DiScenarioResult> ExecuteOperationAsync(string requestId)
    {
        await Task.Delay(10); // Simulate work

        // Create a dedicated scope for this unit of work
        using var scope = _scopeFactory.CreateScope();
        
        // Resolve exactly what we need within the scope
        var scopedState = scope.ServiceProvider.GetRequiredService<IScopedState>();
        var operation = scope.ServiceProvider.GetRequiredService<ITransientOperation>();

        scopedState.Increment();
        operation.Execute();
        
        // Operation and ScopedState are correctly disposed of at the end of the using block
        // No lists or shared states holding onto them

        return new DiScenarioResult(
            Name,
            requestId,
            scopedState.ScopeId,
            scopedState.RequestCount,
            10 
        );
    }
}
