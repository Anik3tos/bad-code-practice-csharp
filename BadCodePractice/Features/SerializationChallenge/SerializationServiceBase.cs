using System.Diagnostics;

namespace BadCodePractice.Features.SerializationChallenge;

public abstract class SerializationServiceBase : ISerializationChallengeService
{
    public abstract string Name { get; }

    public async Task<SerializationTelemetry> RunAsync(
        SerializationChallengeOptions options,
        CancellationToken cancellationToken = default)
    {
        var normalizedOptions = Normalize(options);
        var payloads = SerializationPayloadFactory.Create(
            normalizedOptions.TotalRequests,
            normalizedOptions.ExtraFieldLength);

        ForceCollection();
        var allocatedBefore = GC.GetTotalAllocatedBytes(true);
        var stopwatch = Stopwatch.StartNew();
        var reflectionCacheMisses = await ExecuteCoreAsync(payloads, normalizedOptions, cancellationToken);
        stopwatch.Stop();
        var allocatedAfter = GC.GetTotalAllocatedBytes(true);

        return new SerializationTelemetry(
            normalizedOptions.TotalRequests,
            Math.Max(0, allocatedAfter - allocatedBefore),
            stopwatch.Elapsed.TotalMilliseconds,
            reflectionCacheMisses);
    }

    protected abstract Task<int> ExecuteCoreAsync(
        IReadOnlyList<SerializationRichPayload> payloads,
        SerializationChallengeOptions options,
        CancellationToken cancellationToken);

    private static SerializationChallengeOptions Normalize(SerializationChallengeOptions options)
    {
        return options with
        {
            TotalRequests = Math.Clamp(options.TotalRequests, 100, 5000),
            ConcurrentWorkers = Math.Clamp(options.ConcurrentWorkers, 4, 128),
            ExtraFieldLength = Math.Clamp(options.ExtraFieldLength, 64, 4096)
        };
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
