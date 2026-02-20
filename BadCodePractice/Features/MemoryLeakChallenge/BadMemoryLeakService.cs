namespace BadCodePractice.Features.MemoryLeakChallenge;

public sealed class BadMemoryLeakService : IMemoryLeakService
{
    public string Name => "Bad implementation";

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
                ct => StaticReferenceLeakAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Event handlers never unsubscribed",
                ct => EventSubscriptionLeakAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Timers created and never disposed",
                ct => TimerLeakAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Unbounded in-memory cache growth",
                ct => UnboundedCacheLeakAsync(normalizedIterations, payloadBytes, ct)),
            new LeakScenario(
                "Closures capturing large objects",
                ct => ClosureCaptureLeakAsync(normalizedIterations, payloadBytes, ct))
        };

        return MemoryLeakScenarioExecutor.RunAsync(Name, scenarios, cancellationToken);
    }

    private static async Task StaticReferenceLeakAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MemoryLeakRoots.AddStaticBuffer(new byte[payloadBytes]);
            await Task.Yield();
        }
    }

    private static async Task EventSubscriptionLeakAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subscriber = new EventSubscriber(payloadBytes);
            MemoryLeakRoots.Subscribe(subscriber.OnPulse);
            await Task.Yield();
        }

        MemoryLeakRoots.RaisePulse();
    }

    private static async Task TimerLeakAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = new byte[payloadBytes];
            var timer = new Timer(static state => GC.KeepAlive(state), payload, Timeout.Infinite, Timeout.Infinite);
            MemoryLeakRoots.AddTimer(timer);
            await Task.Yield();
        }
    }

    private static async Task UnboundedCacheLeakAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MemoryLeakRoots.AddCacheEntry($"cache-{i}-{Guid.NewGuid():N}", new byte[payloadBytes]);
            await Task.Yield();
        }
    }

    private static async Task ClosureCaptureLeakAsync(
        int iterations,
        int payloadBytes,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = new byte[payloadBytes];
            MemoryLeakRoots.AddClosure(() => payload.Length);
            await Task.Yield();
        }
    }

    private static int NormalizeIterations(int iterations) => Math.Clamp(iterations, 10, 400);
    private static int NormalizePayloadBytes(int payloadKilobytes) => Math.Clamp(payloadKilobytes, 16, 512) * 1024;
}
