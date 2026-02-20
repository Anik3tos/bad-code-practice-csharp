namespace BadCodePractice.Features.AsyncMisuseChallenge;

public class BadAsyncMisuseService : IAsyncMisuseService
{
    public string Name => "Bad async (Original)";

    public Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        int processedCount = 0;

        for (int i = 0; i < count; i++)
        {
            // 1. Fire-and-forget: doing some independent work without awaiting it/tracking it
            // This could throw exceptions that go unobserved
            Task.Run(() => DoBackgroundLoggingAsync(i));

            // 2. Sync over Async: Using .Result or .Wait() instead of await
            // This blocks the calling thread, causing ThreadPool starvation
            var result = ProcessSingleItemAsync(i).Result;
            
            if (result)
            {
                processedCount++;
            }
        }

        return Task.FromResult(processedCount);
    }

    private async Task DoBackgroundLoggingAsync(int id)
    {
        // 3. Missing Cancellation Token
        await Task.Delay(10); 
    }

    private async Task<bool> ProcessSingleItemAsync(int id)
    {
        // Simulating some I/O bound work like a DB call or HTTP request
        // 3. Missing Cancellation Token
        await Task.Delay(50);
        return true;
    }
}
