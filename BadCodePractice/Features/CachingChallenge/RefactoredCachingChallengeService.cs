using System.Collections.Concurrent;
using System.Diagnostics;

namespace BadCodePractice.Features.CachingChallenge;

public sealed class RefactoredCachingChallengeService : ICachingChallengeService
{
    private const int MaxEntries = 100;
    private static readonly TimeSpan TimeToLive = TimeSpan.FromSeconds(8);

    public string Name => "AI Refactored cache";

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ConcurrentDictionary<string, Task<byte[]>> _inflight = new();

    private int _hits;
    private int _misses;
    private int _backendCalls;

    public async Task<CachingServiceTelemetry> RunAsync(
        CachingChallengeOptions options,
        CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        _inflight.Clear();
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
        var now = DateTimeOffset.UtcNow;
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > now)
        {
            entry.Touch();
            Interlocked.Increment(ref _hits);
            return entry.Payload;
        }

        Interlocked.Increment(ref _misses);
        var inflightTask = _inflight.GetOrAdd(key, _ => LoadAndStoreAsync(key, payloadBytes, cancellationToken));

        try
        {
            return await inflightTask;
        }
        finally
        {
            _inflight.TryRemove(key, out _);
        }
    }

    private async Task<byte[]> LoadAndStoreAsync(string key, int payloadBytes, CancellationToken cancellationToken)
    {
        var payload = await CachingChallengeWorkload.FetchFromSourceAsync(
            key,
            payloadBytes,
            () => Interlocked.Increment(ref _backendCalls),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        _cache[key] = new CacheEntry(payload, now + TimeToLive);
        EvictExpiredAndOverflow(now);
        return payload;
    }

    private void EvictExpiredAndOverflow(DateTimeOffset now)
    {
        foreach (var pair in _cache)
        {
            if (pair.Value.ExpiresAt <= now)
            {
                _cache.TryRemove(pair.Key, out _);
            }
        }

        var overflow = _cache.Count - MaxEntries;
        if (overflow <= 0)
        {
            return;
        }

        var keysToEvict = _cache
            .OrderBy(pair => pair.Value.LastAccessTicks)
            .Take(overflow)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var key in keysToEvict)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private sealed class CacheEntry(byte[] payload, DateTimeOffset expiresAt)
    {
        private long _lastAccessTicks = Stopwatch.GetTimestamp();

        public byte[] Payload { get; } = payload;
        public DateTimeOffset ExpiresAt { get; } = expiresAt;
        public long LastAccessTicks => Volatile.Read(ref _lastAccessTicks);

        public void Touch()
        {
            Interlocked.Exchange(ref _lastAccessTicks, Stopwatch.GetTimestamp());
        }
    }
}
