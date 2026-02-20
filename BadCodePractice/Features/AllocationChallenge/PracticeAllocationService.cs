namespace BadCodePractice.Features.AllocationChallenge;

public class PracticeAllocationService : IAllocationService
{
    public string Name => "Practice Allocations (Your Turn)";

    public int ProcessData(int itemCount)
    {
        var data = Enumerable.Range(0, itemCount).ToArray();
        
        var mixedData = new List<object>();
        foreach (var i in data)
        {
            mixedData.Add(i); 
        }

        int totalLength = 0;
        
        var processedItems = mixedData
            .Where(x => (int)x % 2 == 0)
            .Select(x => ExtractValue(x));

        foreach (var item in processedItems)
        {
            totalLength += item.Length;
        }

        return totalLength;
    }

    private string ExtractValue(object val)
    {
        string result = "";
        
        for (int i = 0; i < 5; i++)
        {
            result += "Value: " + val.ToString() + ", ";
        }

        return result;
    }
}
