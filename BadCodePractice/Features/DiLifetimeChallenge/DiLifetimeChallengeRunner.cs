using System.Diagnostics;

namespace BadCodePractice.Features.DiLifetimeChallenge;

public sealed class DiLifetimeChallengeRunner(
    BadDiLifetimeService badService,
    PracticeDiLifetimeService practiceService,
    RefactoredDiLifetimeService refactoredService)
{
    public async Task<DiChallengeComparisonResult> RunAsync(int requestCount)
    {
        // 1. Force a GC to get a clean baseline of memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long initialMemory = GC.GetTotalMemory(true);

        // 2. Run scenarios
        var bad = await RunScenarioAsync(badService, requestCount);
        var practice = await RunScenarioAsync(practiceService, requestCount);
        var refactored = await RunScenarioAsync(refactoredService, requestCount);

        // 3. Final memory check
        GC.Collect();
        GC.WaitForPendingFinalizers();
        long finalMemory = GC.GetTotalMemory(true);
        double memoryGrowthMb = (finalMemory - initialMemory) / (1024.0 * 1024.0);

        return new DiChallengeComparisonResult(
            requestCount, 
            bad, 
            practice, 
            refactored, 
            memoryGrowthMb);
    }

    private async Task<DiScenarioRunSummary> RunScenarioAsync(IDiLifetimeService service, int totalRequests)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var seenScopeIds = new HashSet<Guid>();
        int highestCountSeen = 0;
        
        // Simulate concurrent requests
        var tasks = new List<Task<DiScenarioResult>>();
        for (int i = 0; i < totalRequests; i++)
        {
            var reqId = $"REQ-{i}";
            tasks.Add(Task.Run(() => service.ExecuteOperationAsync(reqId)));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        foreach (var result in results)
        {
            seenScopeIds.Add(result.ScopeIdSeen);
            if (result.ScopeCountSeen > highestCountSeen)
            {
                highestCountSeen = result.ScopeCountSeen;
            }
        }

        // Did state leak?
        // If it's a singleton captive, we will only see ONE Scope ID across all requests
        // If it's correctly scoped, we will see `totalRequests` different Scope IDs
        bool stateLeaked = seenScopeIds.Count < totalRequests;
        
        return new DiScenarioRunSummary(
            service.Name,
            totalRequests,
            seenScopeIds.Count,
            highestCountSeen,
            stateLeaked,
            stopwatch.Elapsed.TotalMilliseconds);
    }
}

public sealed record DiChallengeComparisonResult(
    int TotalRequests,
    DiScenarioRunSummary Bad,
    DiScenarioRunSummary Practice,
    DiScenarioRunSummary Refactored,
    double MemoryGrowthMb);

public sealed record DiScenarioRunSummary(
    string Label,
    int TotalRequests,
    int UniqueScopesSeen,
    int MaxCountSeen,
    bool StateLeaked,
    double ElapsedMilliseconds);
