using System.Collections.Concurrent;

namespace BadCodePractice.Features.CachingChallenge;

internal static class CachingChallengeWorkload
{
    internal static IReadOnlyList<string> Build(CachingChallengeOptions options)
    {
        var requests = new List<string>(options.TotalRequests + options.ConcurrentWorkers * 8);
        var random = new Random(1337);

        // Stampede burst before cache is warm.
        for (var i = 0; i < options.ConcurrentWorkers * 4; i++)
        {
            requests.Add("hot-product-001");
        }

        for (var i = 0; i < options.TotalRequests; i++)
        {
            requests.Add($"product-{random.Next(options.KeySpace):000}");
        }

        // Another burst to validate hit behavior after warm-up.
        for (var i = 0; i < options.ConcurrentWorkers * 4; i++)
        {
            requests.Add("hot-product-001");
        }

        return requests;
    }

    internal static async Task ExecuteAsync(
        IReadOnlyList<string> keys,
        int maxConcurrency,
        Func<string, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        using var gate = new SemaphoreSlim(maxConcurrency);
        var tasks = new ConcurrentBag<Task>();

        foreach (var key in keys)
        {
            await gate.WaitAsync(cancellationToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    await operation(key, cancellationToken);
                }
                finally
                {
                    gate.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    internal static async Task<byte[]> FetchFromSourceAsync(
        string key,
        int payloadBytes,
        Action onBackendCall,
        CancellationToken cancellationToken)
    {
        onBackendCall();
        await Task.Delay(20, cancellationToken);
        var payload = new byte[payloadBytes];
        payload[0] = (byte)(Math.Abs(key.GetHashCode()) % 255);
        return payload;
    }
}
