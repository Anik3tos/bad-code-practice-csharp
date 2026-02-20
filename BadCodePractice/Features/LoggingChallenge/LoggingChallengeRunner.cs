using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BadCodePractice.Features.LoggingChallenge;

public sealed class LoggingChallengeRunner(IServiceProvider serviceProvider)
{
    public async Task<LoggingChallengeComparisonResult> RunAsync(int requestCount)
    {
        // Generate transactions
        var transactions = new List<UserTransaction>(requestCount);
        for (int i = 0; i < requestCount; i++)
        {
            transactions.Add(new UserTransaction(
                $"req-{Guid.NewGuid().ToString("N")[..8]}",
                $"user-{i}",
                "453211239987" + (i % 1000).ToString("D4"), // Fake CC
                (decimal)(i * 1.5)
            ));
        }

        var store = serviceProvider.GetRequiredService<InMemoryLoggerStore>();

        var bad = await RunScenarioAsync<BadLoggingService>(transactions, store, serviceProvider);
        var practice = await RunScenarioAsync<PracticeLoggingService>(transactions, store, serviceProvider);
        var refactored = await RunScenarioAsync<RefactoredLoggingService>(transactions, store, serviceProvider);

        return new LoggingChallengeComparisonResult(requestCount, bad, practice, refactored);
    }

    private async Task<LoggingScenarioRunResult> RunScenarioAsync<TService>(
        List<UserTransaction> transactions,
        InMemoryLoggerStore store,
        IServiceProvider provider)
        where TService : class, ILoggingService
    {
        var service = provider.GetRequiredService<TService>();

        // Reset the memory logger store before each run
        store.Clear();

        var stopwatch = Stopwatch.StartNew();

        await service.ProcessTransactionsAsync(transactions);

        stopwatch.Stop();

        var logs = store.Entries.ToList();

        // Calculate Metrics
        int totalLogs = logs.Count;
        int totalCharsLogged = logs.Sum(l => l.MessageLength);
        int charsPerRequest = totalLogs > 0 ? totalCharsLogged / transactions.Count : 0;

        int piiExposures = logs.Count(l => l.Message.Contains("45321123")); // Finding the raw prefix

        int logsWithCorrelation = logs.Count(l => l.HasCorrelationId);
        double traceCoveragePercentage = totalLogs > 0 ? (double)logsWithCorrelation / totalLogs * 100 : 0;

        return new LoggingScenarioRunResult(
            service.Name,
            totalLogs,
            charsPerRequest,
            piiExposures,
            traceCoveragePercentage,
            stopwatch.ElapsedMilliseconds
        );
    }
}

public sealed record LoggingChallengeComparisonResult(
    int TotalRequests,
    LoggingScenarioRunResult Bad,
    LoggingScenarioRunResult Practice,
    LoggingScenarioRunResult Refactored);

public sealed record LoggingScenarioRunResult(
    string Label,
    int TotalLogs,
    int CharsPerRequest,
    int PiiExposures,
    double TraceCoveragePercentage,
    long ElapsedMilliseconds);
