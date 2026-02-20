namespace BadCodePractice.Features.MemoryLeakChallenge;

internal static class MemoryLeakRoots
{
    private static readonly List<byte[]> StaticBuffers = new();
    private static readonly List<Timer> Timers = new();
    private static readonly Dictionary<string, byte[]> UnboundedCache = new();
    private static readonly List<Func<int>> CapturedClosures = new();
    private static event EventHandler? Pulse;

    internal static void AddStaticBuffer(byte[] buffer) => StaticBuffers.Add(buffer);
    internal static void AddTimer(Timer timer) => Timers.Add(timer);
    internal static void AddCacheEntry(string key, byte[] value) => UnboundedCache[key] = value;
    internal static void AddClosure(Func<int> closure) => CapturedClosures.Add(closure);
    internal static void Subscribe(EventHandler handler) => Pulse += handler;
    internal static void Unsubscribe(EventHandler handler) => Pulse -= handler;
    internal static void RaisePulse() => Pulse?.Invoke(null, EventArgs.Empty);

    internal static void Reset()
    {
        foreach (var timer in Timers)
        {
            timer.Dispose();
        }

        Timers.Clear();
        StaticBuffers.Clear();
        UnboundedCache.Clear();
        CapturedClosures.Clear();
        Pulse = null;
    }
}

internal sealed class EventSubscriber
{
    private readonly byte[] _payload;

    internal EventSubscriber(int payloadBytes)
    {
        _payload = new byte[payloadBytes];
    }

    internal void OnPulse(object? sender, EventArgs args)
    {
        GC.KeepAlive(_payload);
    }
}
