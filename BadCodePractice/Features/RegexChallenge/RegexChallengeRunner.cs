using System.Diagnostics;
using System.Text;

namespace BadCodePractice.Features.RegexChallenge;

public sealed class RegexChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<RegexChallengeComparisonResult> RunAsync(int lineCount)
    {
        var logLines = GenerateLogLines(lineCount);

        var bad = await Task.Run(() => RunScenario<BadRegexService>(logLines));
        var practice = await Task.Run(() => RunScenario<PracticeRegexService>(logLines));
        var refactored = await Task.Run(() => RunScenario<RefactoredRegexService>(logLines));

        return new RegexChallengeComparisonResult(lineCount, bad, practice, refactored);
    }

    private static List<string> GenerateLogLines(int count)
    {
        var lines = new List<string>(count);
        var levels = new[] { "DEBUG", "INFO", "WARN", "ERROR" };
        var random = new Random(42); // Fixed seed for reproducibility

        var sb = new StringBuilder(256);

        for (int i = 0; i < count; i++)
        {
            var timestamp = DateTime.UtcNow.AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss");
            var level = levels[random.Next(levels.Length)];
            var hasCorrelation = random.Next(3) == 0; // ~33% have correlation IDs

            sb.Clear();
            sb.Append('[');
            sb.Append(timestamp);
            sb.Append("] [");
            sb.Append(level);
            sb.Append("] ");

            if (hasCorrelation)
            {
                sb.Append("[corr:");
                sb.Append(Guid.NewGuid().ToString("N")[..16]);
                sb.Append("] ");
            }

            sb.Append("Sample log message for testing regex parsing performance entry ");
            sb.Append(i);

            lines.Add(sb.ToString());
        }

        return lines;
    }

    private RegexScenarioRunResult RunScenario<TService>(List<string> logLines)
        where TService : class, IRegexService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();

        // Force GC before measurement
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();

        long allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Start = GC.CollectionCount(0);
        int gen1Start = GC.CollectionCount(1);
        int gen2Start = GC.CollectionCount(2);

        var stopwatch = Stopwatch.StartNew();

        // RUN HOT PATH
        var results = service.ParseLogEntries(logLines);

        stopwatch.Stop();

        long allocatedBytesAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0End = GC.CollectionCount(0);
        int gen1End = GC.CollectionCount(1);
        int gen2End = GC.CollectionCount(2);

        long allocatedBytes = allocatedBytesAfter - allocatedBytesBefore;
        double allocatedMb = allocatedBytes / (1024.0 * 1024.0);

        // Verify correctness - all implementations should produce same count
        int parsedCount = results.Count;

        return new RegexScenarioRunResult(
            service.Name,
            parsedCount,
            allocatedMb,
            gen0End - gen0Start,
            gen1End - gen1Start,
            gen2End - gen2Start,
            stopwatch.Elapsed.TotalMilliseconds);
    }
}

public sealed record RegexChallengeComparisonResult(
    int LineCount,
    RegexScenarioRunResult Bad,
    RegexScenarioRunResult Practice,
    RegexScenarioRunResult Refactored);

public sealed record RegexScenarioRunResult(
    string Label,
    int ParsedCount,
    double AllocatedMegabytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    double ElapsedMilliseconds);
