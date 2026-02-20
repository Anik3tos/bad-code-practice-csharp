namespace BadCodePractice.Features.MemoryLeakChallenge;

public interface IMemoryLeakService
{
    string Name { get; }
    Task<MemoryLeakRunResult> RunAsync(int iterations, int payloadKilobytes, CancellationToken cancellationToken = default);
}

public sealed record LeakCauseResult(
    string Cause,
    long RetainedBytes);

public sealed record MemoryLeakRunResult(
    string Label,
    long TotalRetainedBytes,
    IReadOnlyList<LeakCauseResult> Causes);

public sealed record MemoryLeakComparisonResult(
    MemoryLeakRunResult Bad,
    MemoryLeakRunResult Practice,
    MemoryLeakRunResult Refactored);
