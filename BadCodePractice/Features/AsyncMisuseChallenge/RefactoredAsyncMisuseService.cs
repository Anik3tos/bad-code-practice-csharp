namespace BadCodePractice.Features.AsyncMisuseChallenge;

public class RefactoredAsyncMisuseService : IAsyncMisuseService
{
    public string Name => "Refactored async (Good)";

    public async Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        int processedCount = 0;

        // Create tasks for concurrent execution instead of blocking sequentially
        var itemTasks = new List<Task<bool>>();
        var logTasks = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            // 1. Await background tasks or track them properly if done concurrently
            logTasks.Add(DoBackgroundLoggingAsync(i, cancellationToken));

            // 2. Properly await instead of .Result, and initiate concurrently
            itemTasks.Add(ProcessSingleItemAsync(i, cancellationToken));
        }

        // Wait for all log tasks to complete instead of fire-and-forget
        await Task.WhenAll(logTasks);

        // Wait for all processing tasks to complete
        var results = await Task.WhenAll(itemTasks);

        foreach (var result in results)
        {
            if (result)
            {
                processedCount++;
            }
        }

        return processedCount;
    }

    private async Task DoBackgroundLoggingAsync(int id, CancellationToken cancellationToken)
    {
        // 3. Pass through the CancellationToken
        await Task.Delay(10, cancellationToken);
    }

    private async Task<bool> ProcessSingleItemAsync(int id, CancellationToken cancellationToken)
    {
        // 3. Pass through the CancellationToken
        await Task.Delay(50, cancellationToken);
        return true;
    }
}
