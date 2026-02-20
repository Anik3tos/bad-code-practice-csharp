namespace BadCodePractice.Features.DiLifetimeChallenge;

public interface IDiLifetimeService
{
    string Name { get; }
    Task<DiScenarioResult> ExecuteOperationAsync(string requestId);
}

public sealed record DiScenarioResult(
    string Name,
    string RequestId,
    Guid ScopeIdSeen,
    int ScopeCountSeen,
    double ElapsedMilliseconds);
