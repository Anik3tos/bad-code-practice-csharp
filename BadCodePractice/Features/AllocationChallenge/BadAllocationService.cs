namespace BadCodePractice.Features.AllocationChallenge;

public class BadAllocationService : IAllocationService
{
    public string Name => "Bad Allocations (String +, Boxing, LINQ)";

    public int ProcessData(int itemCount)
    {
        // 1. LINQ array sizing: Enumerable.Range creates an iterator, then ToArray allocates a large contiguous array.
        var data = Enumerable.Range(0, itemCount).ToArray();
        
        // 2. We use an untyped list (ArrayList equivalent in feeling, though we use List<object> here)
        // This causes BOXING for every single integer we add to it.
        var mixedData = new List<object>();
        foreach (var i in data)
        {
            mixedData.Add(i); // int is boxed into an object on the heap
        }

        int totalLength = 0;
        
        // 3. We loop and use LINQ .Where and .Select on a hot path
        // This causes hidden enumerator allocations
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
        
        // 4. String concatenation in a loop.
        // Strings are immutable, so this allocates a brand new string array every single iteration
        // leaving the old ones for the Garbage Collector.
        for (int i = 0; i < 5; i++)
        {
            result += "Value: " + val.ToString() + ", ";
        }

        return result;
    }
}
