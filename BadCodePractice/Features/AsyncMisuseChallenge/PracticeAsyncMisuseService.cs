namespace BadCodePractice.Features.AsyncMisuseChallenge;

public class PracticeAsyncMisuseService : IAsyncMisuseService
{
    public string Name => "Practice async (Your Turn)";

    public Task<int> ProcessItemsAsync(int count, CancellationToken cancellationToken = default)
    {
        int processedCount = 0;

        for (int i = 0; i < count; i++)
        {
            Task.Run(() => DoBackgroundLoggingAsync(i));

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
        await Task.Delay(10); 
    }

    private async Task<bool> ProcessSingleItemAsync(int id)
    {
        await Task.Delay(50);
        return true;
    }
}
