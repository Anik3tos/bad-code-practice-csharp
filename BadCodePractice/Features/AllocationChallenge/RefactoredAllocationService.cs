using System.Text;

namespace BadCodePractice.Features.AllocationChallenge;

public class RefactoredAllocationService : IAllocationService
{
    public string Name => "Refactored Allocations (StringBuilder, No Boxing)";

    public int ProcessData(int itemCount)
    {
        // 1. Avoid Enumerable.Range and ToArray. Just use a normal loop.
        // 2. Pre-allocate collections with known capacities to prevent resizing.
        // 3. Use properly typed collections (List<int>) to avoid boxing primitives into objects.
        var data = new List<int>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            data.Add(i); // No boxing!
        }

        int totalLength = 0;

        // Reusing a single StringBuilder is one of the best ways to eliminate string churn in loops
        var sb = new StringBuilder(128);

        // 4. Avoid LINQ in extreme hot paths, use simple loops and if statements
        foreach (var val in data)
        {
            if (val % 2 == 0)
            {
                totalLength += ExtractValue(val, sb);
            }
        }

        return totalLength;
    }

    private int ExtractValue(int val, StringBuilder sb)
    {
        sb.Clear();
        
        for (int i = 0; i < 5; i++)
        {
            // 5. StringBuilder appends without allocating new string objects on every step
            sb.Append("Value: ").Append(val).Append(", ");
        }

        // We don't even need to call sb.ToString() if we just need the length!
        // But assuming we did, it's 1 allocation instead of 15.
        return sb.Length;
    }
}
