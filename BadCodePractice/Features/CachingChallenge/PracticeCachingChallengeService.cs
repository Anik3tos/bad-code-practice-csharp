using System.Collections.Concurrent;
using System.Diagnostics;

namespace BadCodePractice.Features.CachingChallenge;

public sealed class PracticeCachingChallengeService : ICachingChallengeService
{
    public string Name => "Practice cache";

    private readonly ConcurrentDictionary<string, byte[]> _cache = new();

    private int _hits;
    private int _misses;
    private int _backendCalls;

    public async Task<CachingServiceTelemetry> RunAsync(
        CachingChallengeOptions options,
        CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        _hits = 0;
        _misses = 0;
        _backendCalls = 0;

        var requests = CachingChallengeWorkload.Build(options);
        var payloadBytes = options.PayloadKilobytes * 1024;

        var stopwatch = Stopwatch.StartNew();
        await CachingChallengeWorkload.ExecuteAsync(
            requests,
            options.ConcurrentWorkers,
            async (key, ct) => await GetValueAsync(key, payloadBytes, ct),
            cancellationToken);
        stopwatch.Stop();

        return new CachingServiceTelemetry(
            requests.Count,
            _hits,
            _misses,
            _backendCalls,
            stopwatch.Elapsed.TotalMilliseconds,
            _cache.Count);
    }

    private async Task<byte[]> GetValueAsync(string key, int payloadBytes, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(key, out var cached))
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        Interlocked.Increment(ref _misses);
        var fetched = await CachingChallengeWorkload.FetchFromSourceAsync(
            key,
            payloadBytes,
            () => Interlocked.Increment(ref _backendCalls),
            cancellationToken);

        // Starts from the same bad implementation on purpose.
        _cache[key] = fetched;
        return fetched;
    }
}
