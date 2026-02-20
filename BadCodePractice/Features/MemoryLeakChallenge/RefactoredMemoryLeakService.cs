namespace BadCodePractice.Features.MemoryLeakChallenge;

public sealed class RefactoredMemoryLeakService : IMemoryLeakService
{
    public string Name => "AI Refactored implementation";

    public Task<MemoryLeakRunResult> RunAsync(
        int iterations,
        int payloadKilobytes,
        CancellationToken cancellationToken = default)
    {
        var normalizedIterations = NormalizeIterations(iterations);
        var payloadBytes = NormalizePayloadBytes(payloadKilobytes);

        var scenarios = new[]
        {
            new LeakScenario(
                "Static references kept forever",
                ct => StaticReferenceSafeAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Event handlers never unsubscribed",
                ct => EventSubscriptionSafeAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Timers created and never disposed",
                ct => TimerSafeAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Unbounded in-memory cache growth",
                ct => BoundedCacheSafeAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Closures capturing large objects",
                ct => ClosureCaptureSafeAsync(normalizedIterations, payloadBytes, ct))
        };

        return MemoryLeakScenarioExecutor.RunAsync(Name, scenarios, cancellationToken);
    }

    private static async Task StaticReferenceSafeAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var buffer = new byte[payloadBytes];
            GC.KeepAlive(buffer);
            await Task.Yield();
        }
    }

    private static async Task EventSubscriptionSafeAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subscriber = new EventSubscriber(payloadBytes);
            MemoryLeakRoots.Subscribe(subscriber.OnPulse);
            MemoryLeakRoots.RaisePulse();
            MemoryLeakRoots.Unsubscribe(subscriber.OnPulse);
            await Task.Yield();
        }
    }

    private static async Task TimerSafeAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = new byte[payloadBytes];
            using var timer = new Timer(static state => GC.KeepAlive(state), payload, Timeout.Infinite,
                Timeout.Infinite);
            await Task.Yield();
        }
    }

    private static async Task BoundedCacheSafeAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        const int maxEntries = 32;
        var cache = new Dictionary<int, byte[]>();
        var insertionOrder = new Queue<int>();

        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            cache[i] = new byte[payloadBytes];
            insertionOrder.Enqueue(i);

            if (cache.Count > maxEntries)
            {
                var oldestKey = insertionOrder.Dequeue();
                cache.Remove(oldestKey);
            }

            await Task.Yield();
        }
    }

    private static async Task ClosureCaptureSafeAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        var compactClosures = new List<Func<int>>();

        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var size = payloadBytes;
            compactClosures.Add(() => size);
            await Task.Yield();
        }
    }

    private static int NormalizeIterations(int iterations) => Math.Clamp(iterations, 10, 400);
    private static int NormalizePayloadBytes(int payloadKilobytes) => Math.Clamp(payloadKilobytes, 16, 512) * 1024;
}
