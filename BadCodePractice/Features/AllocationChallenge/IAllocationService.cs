namespace BadCodePractice.Features.AllocationChallenge;

public interface IAllocationService
{
    string Name { get; }
    int ProcessData(int itemCount);
}
