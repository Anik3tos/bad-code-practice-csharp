using System.Diagnostics;

namespace BadCodePractice.Features.AllocationChallenge;

public sealed class AllocationChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<AllocationChallengeComparisonResult> RunAsync(int itemCount)
    {
        // To be safe and ensure the UI thread is not blocked by heavy sync workloads, we wrap the execution in Task.Run.
        // Since memory allocation measurements happen entirely inside RunScenario, it's thread-safe enough for this lab.
        var bad = await Task.Run(() => RunScenario<BadAllocationService>(itemCount));
        var practice = await Task.Run(() => RunScenario<PracticeAllocationService>(itemCount));
        var refactored = await Task.Run(() => RunScenario<RefactoredAllocationService>(itemCount));

        return new AllocationChallengeComparisonResult(itemCount, bad, practice, refactored);
    }

    private AllocationScenarioRunResult RunScenario<TService>(int itemCount)
        where TService : class, IAllocationService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        
        // Force GC before we start to get a clean baseline of Gen collections
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();

        int gen0Start = GC.CollectionCount(0);
        int gen1Start = GC.CollectionCount(1);
        int gen2Start = GC.CollectionCount(2);
        
        long allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();

        // RUN HOT PATH
        var resultLength = service.ProcessData(itemCount);
        
        stopwatch.Stop();
        long allocatedBytesAfter = GC.GetAllocatedBytesForCurrentThread();
        
        int gen0End = GC.CollectionCount(0);
        int gen1End = GC.CollectionCount(1);
        int gen2End = GC.CollectionCount(2);

        // Calculate differences
        long allocatedBytes = allocatedBytesAfter - allocatedBytesBefore;
        double allocatedMb = allocatedBytes / (1024.0 * 1024.0);

        return new AllocationScenarioRunResult(
            service.Name,
            resultLength,
            allocatedMb,
            gen0End - gen0Start,
            gen1End - gen1Start,
            gen2End - gen2Start,
            stopwatch.Elapsed.TotalMilliseconds);
    }
}

public sealed record AllocationChallengeComparisonResult(
    int ItemCount,
    AllocationScenarioRunResult Bad,
    AllocationScenarioRunResult Practice,
    AllocationScenarioRunResult Refactored);

public sealed record AllocationScenarioRunResult(
    string Label,
    int OutputLength,
    double AllocatedMegabytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    double ElapsedMilliseconds);
