namespace BadCodePractice.Features.MemoryLeakChallenge;

internal static class MemoryLeakScenarioExecutor
{
    internal static async Task<MemoryLeakRunResult> RunAsync(
        string label,
        IEnumerable<LeakScenario> scenarios,
        CancellationToken cancellationToken)
    {
        var results = new List<LeakCauseResult>();

        foreach (var scenario in scenarios)
        {
            MemoryLeakRoots.Reset();
            var retainedBytes = await MeasureRetainedBytesAsync(scenario.Execute, cancellationToken);
            results.Add(new LeakCauseResult(scenario.Cause, retainedBytes));
            MemoryLeakRoots.Reset();
        }

        var totalRetainedBytes = results.Sum(x => x.RetainedBytes);
        return new MemoryLeakRunResult(label, totalRetainedBytes, results);
    }

    private static async Task<long> MeasureRetainedBytesAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        ForceCollection();
        var baselineBytes = GC.GetTotalMemory(true);
        await operation(cancellationToken);
        ForceCollection();
        var afterBytes = GC.GetTotalMemory(true);
        return Math.Max(0, afterBytes - baselineBytes);
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

internal sealed record LeakScenario(
    string Cause,
    Func<CancellationToken, Task> Execute);
