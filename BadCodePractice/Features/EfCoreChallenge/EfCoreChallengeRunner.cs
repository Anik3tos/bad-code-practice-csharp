using System.Diagnostics;
using BadCodePractice.Infrastructure;

namespace BadCodePractice.Features.EfCoreChallenge;

public sealed class EfCoreChallengeRunner(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<ChallengeComparisonResult> RunAsync(string city, CancellationToken cancellationToken = default)
    {
        var bad = await RunScenarioAsync<BadOrderReportService>(city, cancellationToken);
        var practice = await RunScenarioAsync<PracticeOrderReportService>(city, cancellationToken);
        var refactored = await RunScenarioAsync<RefactoredOrderReportService>(city, cancellationToken);

        return new ChallengeComparisonResult(city, bad, practice, refactored);
    }

    private async Task<ScenarioRunResult> RunScenarioAsync<TService>(
        string city,
        CancellationToken cancellationToken)
        where TService : class, IOrderReportService
    {
        using var scope = serviceScopeFactory.CreateScope();
        var metrics = scope.ServiceProvider.GetRequiredService<QueryMetrics>();
        metrics.Reset();

        var service = scope.ServiceProvider.GetRequiredService<TService>();
        var stopwatch = Stopwatch.StartNew();
        var report = await service.GetOrderReportAsync(city, cancellationToken);
        stopwatch.Stop();

        return new ScenarioRunResult(
            service.Name,
            report.Count,
            stopwatch.Elapsed.TotalMilliseconds,
            metrics.CommandCount,
            report.Take(5).ToList());
    }
}

public sealed record ChallengeComparisonResult(
    string City,
    ScenarioRunResult Bad,
    ScenarioRunResult Practice,
    ScenarioRunResult Refactored);

public sealed record ScenarioRunResult(
    string Label,
    int Rows,
    double ElapsedMilliseconds,
    int QueryCount,
    IReadOnlyList<OrderReportDto> TopRows);
